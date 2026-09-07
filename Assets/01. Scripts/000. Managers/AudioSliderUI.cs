using UnityEngine;
using UnityEngine.UI;

namespace WeGotOne
{
    public class AudioSliderUI : MonoBehaviour
    {
        public enum AudioType
        {
            Bgm,
            Sfx
        }

        [SerializeField] private Slider slider;
        [SerializeField] private AudioType type;

        private void OnEnable()
        {
            switch (type)
            {
                case AudioType.Bgm:
                    slider.value = OptionManager.Instance.BgmVolume;
                    break;

                case AudioType.Sfx:
                    slider.value = OptionManager.Instance.SfxVolume;
                    break;
            }
        }

        public void OnValueChanged(float value)
        {
            switch (type)
            {
                case AudioType.Bgm:
                    OptionManager.Instance.SetBgmVolume(value);
                    break;

                case AudioType.Sfx:
                    OptionManager.Instance.SetSfxVolume(value);
                    break;
            }
        }
    }
}