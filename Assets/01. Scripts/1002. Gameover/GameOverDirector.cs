using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WeGotOne
{
    public class GameOverDirector : MonoBehaviour
    {
        [Header("Hierarchy Targets")] [SerializeField]
        private CanvasGroup gameOverPanelGroup; // GameOverPanel

        [SerializeField] private RectTransform monsterRect; // Geodude
        [SerializeField] private RectTransform gameText; // Game
        [SerializeField] private RectTransform overText; // Over
        [SerializeField] private RectTransform retryButton; // Button

        [Header("Drop Settings")] [Tooltip("화면 위쪽으로 띄워놓을 낙하 거리 (기본 1000)")] [SerializeField]
        private float dropDistance = 1000f;
        [SerializeField] private float delayAfterMonster = 0.15f; // 꼬마돌 착지 후 GAME 떨어질 때까지의 텀
        [SerializeField] private float delayBetweenTexts = 0.25f;

        [SerializeField] private float monsterDropDuration = 0.32f; // 몬스터 낙하 속도
        [SerializeField] private float textDropDuration = 0.28f; // 텍스트 낙하 속도

        [Header("Camera & Audio")] [SerializeField]
        private Transform cameraTransform; // Main Camera (비워둘 시 자동 탐색)

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip landSfx;
        [SerializeField] private AudioClip textSlamSfx;

        // 원래 씬 뷰에 배치된 목표 좌표 및 스케일 자동 캐싱
        private Vector2 monsterTargetPos;
        private Vector2 gameTargetPos;
        private Vector2 overTargetPos;
        private Vector3 buttonScale;
        private bool isInitialized = false;

        private void Awake()
        {
            InitPositions();
        }

        private void InitPositions()
        {
            if (isInitialized) return;

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            // 씬 뷰에 배치해둔 최종 위치/크기 저장
            if (monsterRect != null) monsterTargetPos = monsterRect.anchoredPosition;
            if (gameText != null) gameTargetPos = gameText.anchoredPosition;
            if (overText != null) overTargetPos = overText.anchoredPosition;
            if (retryButton != null) buttonScale = retryButton.localScale;

            isInitialized = true;
        }

        private void OnEnable()
        {
            PlayGameOverSequence();
        }

        public void PlayGameOverSequence()
        {
            InitPositions();

            // 1. 기존 진행 중인 트윈 초기화
            DOTween.Kill(transform);
            if (monsterRect != null) monsterRect.DOKill();
            if (gameText != null) gameText.DOKill();
            if (overText != null) overText.DOKill();
            if (retryButton != null) retryButton.DOKill();

            // 2. 화면 위쪽 바깥으로 Y축 순간이동 세팅
            if (gameOverPanelGroup != null) gameOverPanelGroup.alpha = 0f;

            if (monsterRect != null)
            {
                monsterRect.anchoredPosition = monsterTargetPos + new Vector2(0f, dropDistance);
                monsterRect.localScale = Vector3.one;
            }

            if (gameText != null)
            {
                gameText.anchoredPosition = gameTargetPos + new Vector2(0f, dropDistance);
                gameText.localScale = Vector3.one;
            }

            if (overText != null)
            {
                overText.anchoredPosition = overTargetPos + new Vector2(0f, dropDistance);
                overText.localScale = Vector3.one;
            }

            if (retryButton != null)
            {
                retryButton.localScale = Vector3.zero;
            }

            // 3. DOTween 타임라인 시퀀스 시작
            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true); // Time.timeScale = 0 환경 대응

            // 배경 어두워지기
            if (gameOverPanelGroup != null)
            {
                seq.Append(gameOverPanelGroup.DOFade(1f, 0.2f));
            }

            // [1단계] 꼬마돌 위에서 바닥으로 급강하 (가속 낙하)
            if (monsterRect != null)
            {
                // 배경이 어두워짐과 동시에 위에서 떨어지도록 Join
                seq.Join(monsterRect.DOAnchorPos(monsterTargetPos, monsterDropDuration).SetEase(Ease.InQuad));

                // 착지 임팩트: 바닥 찍는 순간 착지음 + 카메라 셰이크 + 찌그러짐(Squash)
                seq.AppendCallback(() =>
                {
                    if (audioSource && landSfx) audioSource.PlayOneShot(landSfx);
                    if (cameraTransform != null)
                        cameraTransform.DOShakePosition(0.25f, strength: 0.35f, vibrato: 20, randomness: 90f);

                    // 바닥 강타 시 위아래로 살짝 찌그러졌다가 펴지는 묵직한 물리 탄성
                    monsterRect.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.08f)
                        .OnComplete(() => monsterRect.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack));
                });
            }

            seq.AppendInterval(delayAfterMonster);

            // [2단계] 'GAME' 텍스트 위에서 쿵! 떨어지며 바운스
            if (gameText != null)
            {
                seq.Append(gameText.DOAnchorPos(gameTargetPos, textDropDuration).SetEase(Ease.OutBounce));
                seq.AppendCallback(() =>
                {
                    if (audioSource && textSlamSfx) audioSource.PlayOneShot(textSlamSfx);
                });
            }

            seq.AppendInterval(delayBetweenTexts);
            
            // [3단계] 'OVER' 텍스트 위에서 쾅! 떨어지며 묵직한 바운스 + 추가 진동
            if (overText != null)
            {
                seq.Append(overText.DOAnchorPos(overTargetPos, textDropDuration).SetEase(Ease.OutBounce));
                seq.AppendCallback(() =>
                {
                    if (audioSource && textSlamSfx) audioSource.PlayOneShot(textSlamSfx);
                    if (cameraTransform != null)
                        cameraTransform.DOShakePosition(0.15f, strength: 0.2f, vibrato: 15);
                });
            }

            // [4단계] 메인 메뉴 버튼 팝업
            if (retryButton != null)
            {
                seq.AppendInterval(0.12f);
                seq.Append(retryButton.DOScale(buttonScale, 0.22f).SetEase(Ease.OutBack));
            }
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
            if (monsterRect != null) monsterRect.DOKill();
            if (gameText != null) gameText.DOKill();
            if (overText != null) overText.DOKill();
            if (retryButton != null) retryButton.DOKill();
        }
    }
}