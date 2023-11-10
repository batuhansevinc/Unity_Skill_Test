using System.Collections;
using TMPro;
using Assignment01.Managers;
using UnityEngine;

public class UIController : MonoBehaviour
{
   [SerializeField] GameObject _particleEffect;
   [SerializeField] TextMeshProUGUI _levelText;
   [SerializeField] TextMeshProUGUI _sceneLevelText;
   [SerializeField] AudioSource _confettiSound;

   void Start()
   {
      UpdateLevelText();
   }

   IEnumerator StartUpdateLevelText()
   {
      yield return new WaitForSeconds(0.25f);
      int currentProgress = LevelManager.Instance.CurrentLevel;
      _levelText.text = "Level " + currentProgress;
      _sceneLevelText.text = "Level " + currentProgress;
   }

   public void UpdateLevelText()
   {
      StartCoroutine(StartUpdateLevelText());
   }


   public void ParticleEffectEnable()
   {
      _particleEffect.SetActive(true);
      _confettiSound.Play();
      StartCoroutine(ParticleEffectDisable());
   }

   IEnumerator ParticleEffectDisable()
   {
      yield return new WaitForSeconds(2f);
      _particleEffect.SetActive(false);
   }
}
