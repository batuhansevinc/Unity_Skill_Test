namespace Assignment01.Uis
{
    public class SettingsButton : BaseButtonWithGameEvents
    {
        protected override void HandleOnButtonClicked()
        {
            _buttonEvent.InvokeEvents();
        }
    }
}