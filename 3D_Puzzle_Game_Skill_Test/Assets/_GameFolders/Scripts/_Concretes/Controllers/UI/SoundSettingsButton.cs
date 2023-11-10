using Assignment01.Abstracts.Uis;
using UnityEngine;
using UnityEngine.Audio;

namespace Assignment01.Uis
{
    public class SoundSettingsButton : BaseSettingsSwitchButton
    {
        const string SOUND_KEY = "sound_key";

        [SerializeField] string _parameterValue;
        [SerializeField] AudioMixerGroup _audioMixerGroup;
        
        void Start()
        {
            LoadData(SOUND_KEY);
            SetValue(_isOpen, SOUND_KEY, UnmuteCallback, MuteCallback);
        }

        // Override the base class method to extend its behavior
        protected override void HandleOnToggleValueChanged(bool isOn) 
        {
            base.HandleOnToggleValueChanged(isOn);
            SetValue(isOn, SOUND_KEY, UnmuteCallback, MuteCallback);
        }

        void UnmuteCallback()
        {
            //Debug.Log("Unmuting sound.");
            _audioMixerGroup.audioMixer.SetFloat(_parameterValue, 1);
        }

        void MuteCallback()
        {
            //Debug.Log("Muting sound.");
            _audioMixerGroup.audioMixer.SetFloat(_parameterValue, -80f);
        }
    }
}
