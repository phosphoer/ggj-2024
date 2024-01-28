using UnityEngine;

public class RadioController : MonoBehaviour
{
  [SerializeField]
  private SoundBank _sfxRadio = null;

  [SerializeField]
  private SoundBank _sfxToggle = null;

  [SerializeField]
  private Interactable _interactable = null;

  private AudioManager.AudioInstance _audioInstance = null;
  private bool _isPlaying = true;

  private void Awake()
  {
    _interactable.InteractionTriggered += OnInteractionTriggered;
  }

  private void OnInteractionTriggered(InteractionController controller)
  {
    AudioManager.Instance.PlaySound(gameObject, _sfxToggle);
    if (_audioInstance != null && _audioInstance.AudioSource.isPlaying)
    {
      _audioInstance.AudioSource.Stop();
      _isPlaying = false;
    }
    else
    {
      PlayNextSong();
    }
  }

  private void Update()
  {
    if (_audioInstance == null || !_audioInstance.AudioSource.isPlaying)
    {
      if (_isPlaying)
        PlayNextSong();
    }
  }

  private void PlayNextSong()
  {
    _isPlaying = true;
    _audioInstance = AudioManager.Instance.PlaySoundClip(gameObject, _sfxRadio, Random.Range(0, _sfxRadio.AudioClips.Length));
  }
}