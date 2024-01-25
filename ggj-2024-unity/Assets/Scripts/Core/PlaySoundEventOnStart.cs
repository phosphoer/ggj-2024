using UnityEngine;
using System.Collections.Generic;

public class PlaySoundEventOnStart : MonoBehaviour
{
  public SoundBank SoundBank;
  public float FadeInTime;
  public string GroupPlayIntervalName;
  public float MinPlayInterval = 1;

  private static Dictionary<string, float> _lastPlayTimeMap = new Dictionary<string, float>();

  public AudioManager.AudioInstance GetAudioInstance()
  {
    return AudioManager.Instance.GetAudioInstance(gameObject, SoundBank);
  }

  private void Start()
  {
    if (!string.IsNullOrEmpty(GroupPlayIntervalName))
    {
      float lastPlayTime;
      if (!_lastPlayTimeMap.TryGetValue(GroupPlayIntervalName, out lastPlayTime))
        lastPlayTime = 0;

      if (Time.unscaledTime > lastPlayTime + MinPlayInterval)
      {
        _lastPlayTimeMap[GroupPlayIntervalName] = Time.unscaledTime;
      }
      else
      {
        return;
      }
    }

    if (FadeInTime > 0)
    {
      StartCoroutine(AudioManager.QueueFadeInSoundRoutine(gameObject, SoundBank, 1.0f, FadeInTime));
    }
    else
    {
      StartCoroutine(AudioManager.QueuePlaySoundRoutine(gameObject, SoundBank));
    }
  }
}