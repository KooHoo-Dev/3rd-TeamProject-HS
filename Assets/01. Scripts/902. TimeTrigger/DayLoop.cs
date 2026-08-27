using DG.Tweening;
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
        [SerializeField] private SpriteRenderer nightSprite;
        [SerializeField] private Light2D dayLight;

        [SerializeField] private float transitionTime = 1f;

        private DayType currentDayType = DayType.Day;

        public void SetDayType(DayType dayType)
        {
            if (currentDayType == dayType)
                return;

            currentDayType = dayType;

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