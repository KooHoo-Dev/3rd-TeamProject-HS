using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HelloServer;
using UnityEngine.InputSystem;


namespace WeGotOne
{
    public class TutorialManager : SceneSingleton<TutorialManager>
    {   
        public enum TutorialStep
        {
            NotStarted,
            KeyboardGuide,    // 1. 키 안내 단계
            WaitAllReady,     // 3. F키 동시에 꾹 누르기 대기 단계
            StormActive,      // 4. 폭풍 이미지 활성화 (3초 타이머) 단계
            Falling,          // 5. 바닥 꺼지고 낙하하는 단계
            Completed         // 7. 착륙 후 최종 가이드 단계
        }
        
        [Header("Tutorial UI Components")]
        [SerializeField] private GameObject tutorialCanvas;
        [SerializeField] private GameObject keyboardImage;
        [SerializeField] private GameObject stormImage;
        [SerializeField] private GameObject arrivedIslandImage; // (예비용)
        [SerializeField] private GameObject textBoxPanel;
        [SerializeField] private TMP_Text instructionText;
        
        [Header("User Ready CheckBox UI")]
        [SerializeField] private GameObject checkBoxUICanvas;
        [SerializeField] private GameObject playerCheckBox_First;
        [SerializeField] private GameObject playerCheckBox_Second;
        [SerializeField] private GameObject playerCheckBox_Thrid;
        [SerializeField] private GameObject playerCheckBox_Foruth;
        [SerializeField] private GameObject startButton;
        
        [Header("World Elements")]
        [SerializeField] private GameObject floor; // Lobby Floor
        [SerializeField] private Collider2D landingZoneTrigger; // LandingZoneTrigger
        [SerializeField] private GameObject[] shipBoundaries; // 진입 장벽 콜라이더들
        
        private TutorialStep currentStep = TutorialStep.NotStarted;
        private bool isHost;
        private bool isStormCoroutineRunning = false;
        private bool isStartButtonPressed = false;

        public bool IsStartButtonPressed => isStartButtonPressed;
        private bool IsHost => RoomClient.Instance != null && 
                               RoomClient.Instance.Me != null && 
                               RoomClient.Instance.Me.IsHost;

        public bool IsTargetChange = false;
        
        public bool IsTutorialEnd = false;
        public int ReadyCount => readyCount;
        
        private int readyCount;
        private int totalCount;

        private GameObject[] CheckBoxes => new[]
        {
            playerCheckBox_First, playerCheckBox_Second, playerCheckBox_Thrid, playerCheckBox_Foruth
        };
        
        
        private void Start()
        {
            // 0. 게임 시작 시에는 튜토리얼 UI Canvas 비활성화
            if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
            
            // 방 입장 동기화 시점에 활성화하려면 외부(RoomClient 등)에서 
            // TutorialManager.Instance.StartTutorial()을 호출해 주면 편리합니다.
            StartCoroutine(WaitAndCheckHost());
        }

        private IEnumerator WaitAndCheckHost()
        {
            yield return new WaitUntil(() => RoomClient.Instance != null && RoomClient.Instance.Me != null);
            isHost = IsHost;
        }
        
        // [외부 호출용] 처음 호스트/게스트가 룸에 입장했을 때 호출
        public void StartTutorial()
        {
            currentStep = TutorialStep.KeyboardGuide;

            if (IsTutorialEnd == false)
            {
                tutorialCanvas.SetActive(true);
                checkBoxUICanvas.SetActive(false);
                startButton.SetActive(false);
                keyboardImage.SetActive(true);
            }
            textBoxPanel.SetActive(true);
        
            // 1. 텍스트 문장 보여줌
            instructionText.text = "마우스 좌, 우 클릭으로 벽을 집고 W,S,A,D 으로 \n팔 방향을 조정하여 이동 할 수 있습니다. ";
        }
        
        private void Update()
        {
            //if(IsTutorialEnd)
                //tutorialCanvas.SetActive(false);
            
            if (currentStep == TutorialStep.NotStarted || currentStep == TutorialStep.Completed) return;

            // 키보드 상태 감지 (새 인풋 시스템)
            if (Keyboard.current == null) return;

            if (currentStep == TutorialStep.KeyboardGuide && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                keyboardImage.SetActive(false);
                checkBoxUICanvas.SetActive(true);
                if(isHost) startButton.SetActive(true);
                instructionText.text = "출발할 준비가 완료되면 [F] 키를 꾹 눌러주세요.";
                currentStep = TutorialStep.WaitAllReady;
            }
            
            if (RoomClient.IsInitialized
                && RoomClient.Instance.Me != null
                && RoomClient.Instance.Me.IsHost
                && RoomClient.Instance.SnapshotMessage != null)
            {
                SnapshotMessage snapshotMessage = RoomClient.Instance.SnapshotMessage;
                bool hostHolding = RoomClient.Instance.LocalPlayer.IsBuild;
                int guestHoldingCount = 0;
                if (RoomClient.Instance.GuestInputGroupMessage != null)
                {
                    foreach (var input in RoomClient.Instance.GuestInputGroupMessage.Inputs)
                    {
                        if (input != null && input.IsBuildKeyHold) guestHoldingCount++;
                    }
                }
                readyCount = (hostHolding ? 1 : 0) + guestHoldingCount;
                UpdateReadyState(readyCount, snapshotMessage.TutorialTotalCount, IsStartButtonPressed, IsTutorialEnd);
            }
        }

        // 2. [요청 사양] Storm 이미지 3초 작동 코루틴
        private IEnumerator PlayStormSequence()
        {
            isStormCoroutineRunning = true;
            IsTutorialEnd = true;
            
            currentStep = TutorialStep.StormActive;

            // Storm 이미지 활성화
            startButton.SetActive(false);
            if(AudioManager.Instance.sfxClips[(int)AudioManager.Sfx.Thunder])
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.Thunder);
            stormImage.SetActive(true);
            instructionText.text = "갑자기 폭풍이 몰아칩니다!";
            
            // 3초 대기
            yield return new WaitForSeconds(5f);

            // Storm 이미지 비활성화
            stormImage.SetActive(false);
            //textBoxPanel.SetActive(false);
            arrivedIslandImage.SetActive(true);

            // 5. GameLobyArea의 Floor 비활성화 (낙하 유도)
            if (floor != null) floor.SetActive(false);
            checkBoxUICanvas.SetActive(false);
            
            instructionText.text = "아 안돼..! 꼬마!";
            currentStep = TutorialStep.Falling;
            
            isStormCoroutineRunning = false;
        }

        // 3. [요청 사양] 호스트 컴퓨터에서만 낙하 트리거 판정
        public void OnPlayerLanded()
        {
            if (currentStep != TutorialStep.Falling) return;

            // 6. 경계 콜라이더들 모두 비활성화
            foreach (GameObject boundary in shipBoundaries)
            {
                if (boundary != null) boundary.SetActive(false);
            }
            
            // 7. 땅에 떨어진 후 텍스트 문구 교체
            
            currentStep = TutorialStep.Completed;
            arrivedIslandImage.SetActive(false); // 떨어진 후 바로 암전 끄기
            GameManager.Instance.IsCamTargetChange = true;
            DayLoop.Instance.isStarted = true;
            // 5초 뒤 UI 패널을 꺼줍니다.
            StartCoroutine(CloseTutorialUIPanel(5f));
        }

        private IEnumerator CloseTutorialUIPanel(float delay)
        {   
            instructionText.text = "여긴 어디야..? 꼬마..?";
            yield return new WaitForSeconds(10f);
            
            instructionText.text = "낚시하기 딱 좋은 날이군..! 꼬마!";
            yield return new WaitForSeconds(delay);
            
            tutorialCanvas.SetActive(false);
        }

        // 매 스냅샷마다 호출되어 체크박스를 채우고, 전원 준비 시 폭풍 연출을 시작합니다.
        // 호스트/게스트 모두 각자 로컬에서 동일하게 호출되어 독립적으로 진행합니다.
        // ==========================================
        public void UpdateReadyState(int readyCount, int totalCount, bool isStartButtonPress, bool isTutorialEnd)
        {
            var boxes = CheckBoxes;
            for (int i = 0; i < boxes.Length; i++)
            {
                if (boxes[i] == null) continue;
                boxes[i].SetActive(i < readyCount);
            }
            
            isStartButtonPressed = isStartButtonPress;
            IsTutorialEnd = isTutorialEnd;
            this.readyCount = readyCount;
            this.totalCount = totalCount;

            if ((IsTutorialEnd && currentStep == TutorialStep.WaitAllReady )
                || (currentStep == TutorialStep.WaitAllReady 
                && isStartButtonPressed
                && totalCount > 0 
                && readyCount >= totalCount) )
            {
                StartCoroutine(PlayStormSequence());
            }
        }

        public void HandleStartButton()
        {
            isStartButtonPressed = true;
            UpdateReadyState(readyCount, totalCount, isStartButtonPressed, IsTutorialEnd);
        }
    }
}
        
   