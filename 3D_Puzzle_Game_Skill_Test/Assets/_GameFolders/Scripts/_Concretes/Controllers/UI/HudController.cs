using TMPro;
using UnityEngine;

namespace BufoGames.Uis
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _levelText;
        [SerializeField] TextMeshProUGUI _sceneLevelText;

        public void Initialize()
        {
            if (_levelText == null && _sceneLevelText == null)
            {
                Debug.LogError($"{nameof(HudController)} on '{name}' requires at least one level text reference.");
            }
        }

        public void Deinitialize()
        {
        }

        public void SetLevelText(int level)
        {
            string value = "Level " + level;

            if (_levelText != null)
            {
                _levelText.text = value;
            }

            if (_sceneLevelText != null)
            {
                _sceneLevelText.text = value;
            }
        }
    }
}
