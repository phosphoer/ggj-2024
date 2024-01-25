using System.Collections.Generic;

public class Splitscreen3DAudioManager : Singleton<Splitscreen3DAudioManager>
{
  public IEnumerable<SplitscreenAudioListener> AudioListeners { get { return _audioListeners; } }

  private List<Splitscreen3DAudio> _audioSources = new List<Splitscreen3DAudio>();
  private List<SplitscreenAudioListener> _audioListeners = new List<SplitscreenAudioListener>();

  private void Awake()
  {
    Instance = this;

    _audioListeners.AddRange(FindObjectsOfType<SplitscreenAudioListener>());
    _audioSources.AddRange(FindObjectsOfType<Splitscreen3DAudio>());

    Splitscreen3DAudio.Added += OnAudioSourceAdded;
    Splitscreen3DAudio.Removed += OnAudioSourceRemoved;
    SplitscreenAudioListener.Added += OnAudioListenerAdded;
    SplitscreenAudioListener.Removed += OnAudioListenerRemoved;
  }

  private void Update()
  {
    foreach (Splitscreen3DAudio audioSource in _audioSources)
    {
      if (audioSource != null)
      {
        audioSource.Update3DAudio();
      }
    }
  }

  private void OnAudioSourceAdded(Splitscreen3DAudio audioSource)
  {
    _audioSources.Add(audioSource);
    audioSource.Update3DAudio();
  }

  private void OnAudioSourceRemoved(Splitscreen3DAudio audioSource)
  {
    _audioSources.Remove(audioSource);
  }

  private void OnAudioListenerAdded(SplitscreenAudioListener audioListener)
  {
    _audioListeners.Add(audioListener);
  }

  private void OnAudioListenerRemoved(SplitscreenAudioListener audioListener)
  {
    _audioListeners.Remove(audioListener);
  }
}