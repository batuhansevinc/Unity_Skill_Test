using UnityEngine;

namespace BufoGames.Uis
{
    public class ReTryButton : BatuhanSevinc.Uis.BaseButton
    {
        [SerializeField] AudioSource _buttonClickSound;

        protected override void HandleOnButtonClicked()
        {
            if (_buttonClickSound != null)
            {
                _buttonClickSound.Play();
            }

            base.HandleOnButtonClicked();
        }
    }
}
