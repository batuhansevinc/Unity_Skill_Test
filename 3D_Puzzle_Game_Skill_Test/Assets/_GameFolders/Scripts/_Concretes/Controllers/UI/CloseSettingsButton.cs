namespace BufoGames.Uis
{
    public class CloseSettingsButton : BaseButtonWithGameEvents
    {
        protected override void HandleOnButtonClicked()
        {
            _buttonEvent.InvokeEvents();
        }
    }
}