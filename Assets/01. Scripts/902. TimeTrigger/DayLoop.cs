using System;
using System.Collections;
using DG.Tweening;
using HelloServer;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

        }
        

        private void Update()
        {
            // 판정은 호스트에서만 하고 결과(IsDay) 만 스냅샷으로 전파 
            if (isHost == false) return;

            elapsedTime += Time.deltaTime;
            Debug.Log($"남은시간 {elapsedTime} , 주기 {cycleDuration}");
            if (elapsedTime < cycleDuration) return;

            
            DayType next = currentDayType == DayType.Day ? DayType.Night : DayType.Day;
            
            SetDayType(next);
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