namespace Assignment01.Uis
{
    public class CloseSettingsButton : BaseButtonWithGameEvents
    {
        protected override void HandleOnButtonClicked()
        {
            _buttonEvent.InvokeEvents();
        }
    }
}