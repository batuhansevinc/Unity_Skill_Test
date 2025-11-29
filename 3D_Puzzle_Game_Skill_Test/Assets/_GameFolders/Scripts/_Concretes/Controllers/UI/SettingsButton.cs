namespace BufoGames.Uis
{
    public class SettingsButton : BaseButtonWithGameEvents
    {
        protected override void HandleOnButtonClicked()
        {
            _buttonEvent.InvokeEvents();
        }
    }
}