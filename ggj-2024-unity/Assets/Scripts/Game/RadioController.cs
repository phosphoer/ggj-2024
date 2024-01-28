using UnityEngine;

public class RadioController : MonoBehaviour
{
  [SerializeField]
  private SoundBank _sfxRadio = null;

  private AudioManager.AudioInstance _audioInstance = null;

  private void Update()
  {
    if (_audioInstance == null || !_audioInstance.AudioSource.isPlaying)
    {
      _audioInstance = AudioManager.Instance.PlaySoundClip(gameObject, _sfxRadio, Random.Range(0, _sfxRadio.AudioClips.Length));
    }
  }
}