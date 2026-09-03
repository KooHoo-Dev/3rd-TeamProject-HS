using System;
using System.Collections;
using DG.Tweening;
using HelloServer;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace WeGotOne
{
    public class DayLoop : SingletonBase<DayLoop>
    {
        public enum DayType
        {
            Day,
            Night
        }
        
        [Header("연출")]
        [SerializeField] private SpriteRenderer nightSprite;
        [SerializeField] private Light2D dayLight;
        [SerializeField] private float transitionTime = 1f;

        [Header("시간 주기 설정")] [SerializeField] 
        private float cycleDuration = 10f;
        
        [Header("낮/밤 타이머 UI")]
        [SerializeField] public Image DayTimer;
        
        private DayType currentDayType = DayType.Day;
        private float elapsedTime;
        private bool isHost;

        public DayType CurrentDayType => currentDayType;
        
        // 호스트의 경과 시간을 외부에 노출하기 위한 프로퍼티 (스냅샷 전송용)
        public float ElapsedTime => elapsedTime;
        
        // DayLoop는 "지금 낮/밤 이 바뀐 사실만 발행 
        // 스폰 로직, 연출 등 실제 처리는 구독자가 판단

       
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => RoomClient.Instance != null && RoomClient.Instance.Me != null);

            isHost = RoomClient.Instance.Me != null && RoomClient.Instance.Me.IsHost;
            UpdateClockUI();
        }
        

        private void Update()
        {   
            elapsedTime += Time.deltaTime;
            
            // 오직 호스트만 "주기 끝에 다다랐을 때 밤낮을 바꾸는 물리적 판정"을 주도합니다.
            if (isHost)
            {
                if (elapsedTime >= cycleDuration)
                {
                    elapsedTime -= cycleDuration; // -= 처리를 해야 프레임 잔여 시간 누수가 없습니다.
                    DayType next = currentDayType == DayType.Day ? DayType.Night : DayType.Day;
                    SetDayType(next);
                }
            }
            
            // 매 프레임의 시간의 흐름에 맞춰서 UI 이미지 갱신
            UpdateClockUI();
            
        }
        
        
        
        // [호스트 전용 진입] 시간 도달에 의해 낮/밤 을 전환
        public void SetDayType(DayType dayType)
        {
            if (currentDayType == dayType)  return;

            currentDayType = dayType;
            elapsedTime = 0f; // 전환 시점 기준으로 주기를 다시 맞춤
            Debug.Log($"[DayLoop] 밤낮 전환: {dayType}");
            PlayTransitionDayChange(dayType);
            

        }
        
        // [게스트 전용] 스냅샷으로 수신한 IsDay 값을 로컬 상태에 반영

        public void DayApplyFromNetwork(bool isDay)
        {
            if (isHost) return;
            
            // 낮 밤 상태 동기화 ( 다르면 강제 전환 및 연출 재생)
            DayType networkDayType = isDay ? DayType.Day : DayType.Night;
            if (currentDayType != networkDayType)
            {
                SetDayType(networkDayType);   
            }
            
        }
        
        /// <summary>
        /// [신규 추가] 경과 시간을 기반으로 Filled UI 이미지의 채우기 비율을 실시간 계산하여 대입합니다.
        /// </summary>
        private void UpdateClockUI()
        {
            if (DayTimer == null) return;

            // 현재 주기의 진행 비율 계산 (0.0 ~ 1.0) 
            float ratio = Mathf.Clamp01(elapsedTime / cycleDuration);

            if (currentDayType == DayType.Day)
            {
                // 대낮 시작(ratio=0)일 때 fillAmount는 1 
                // 대낮 종료(ratio=1) 직전 밤이 다가오면 fillAmount는 0으로 부드럽게 감소 
                DayTimer.fillAmount = 1f - ratio;
            }
            else
            {
                // 밤 시작(ratio=0)일 때 fillAmount는 0 
                // 밤 종료(ratio=1) 직전 대낮이 다가오면 fillAmount는 1로 부드럽게 증가
                DayTimer.fillAmount = ratio;
            }
        }
        
        
        private void PlayTransitionDayChange(DayType dayType)
        {
            float targetAlpha = dayType == DayType.Night ? 1f : 0f;
            float targetIntensity = dayType == DayType.Night ? 0f : 1f;

            DOTween.Sequence()
                .Append(nightSprite.DOFade(targetAlpha, transitionTime))
                .Join(
                    DOTween.To(
                        () => dayLight.intensity,
                        value => dayLight.intensity = value,
                        targetIntensity,
                        transitionTime
                    )
                );
        }
    }
}