using Assignment01.Abstracts.Uis;

namespace Assignment01.Uis
{
    public class VibrationSettingsButton : BaseSettingsSwitchButton
    {
        const string VIBRATION_KEY = "vibration_key";
        
        void Start()
        {
            LoadData(VIBRATION_KEY);
            SetValue(_isOpen, VIBRATION_KEY,UnmuteCallback,MuteCallback);
        }
        protected override void HandleOnToggleValueChanged(bool isOn) 
        {
            base.HandleOnToggleValueChanged(isOn);
            SetValue(isOn, VIBRATION_KEY, UnmuteCallback, MuteCallback);
        }
        void UnmuteCallback()
        {
            
        }

        void MuteCallback()
        {
           
        }
        
    }
}