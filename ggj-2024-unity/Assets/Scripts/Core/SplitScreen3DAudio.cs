using UnityEngine;

public class Splitscreen3DAudio : MonoBehaviour
{
  public static event System.Action<Splitscreen3DAudio> Added;
  public static event System.Action<Splitscreen3DAudio> Removed;

  public float Range = 10.0f;
  public float Volume = 1.0f;
  public bool GetAudioSourcesOnStart = false;

  [SerializeField]
  private AudioSource[] _audioSources = new AudioSource[0];

  public void SetAudioSource(AudioSource audioSource)
  {
    _audioSources = new AudioSource[] { audioSource };
  }

  public void Update3DAudio()
  {
    if (Splitscreen3DAudioManager.Instance == null)
    {
      return;
    }

    var minDistance = Mathf.Infinity;
    foreach (SplitscreenAudioListener audioListener in Splitscreen3DAudioManager.Instance.AudioListeners)
    {
      var dist = Vector3.Distance(audioListener.transform.position, transform.position);
      minDistance = Mathf.Min(minDistance, dist);
    }

    for (var i = 0; i < _audioSources.Length; ++i)
    {
      var t = 1.0f - Mathf.Clamp01(minDistance / Range);
      float desiredVolume = Mathf.Lerp(0, Volume, t);
      _audioSources[i].volume = desiredVolume;
    }
  }

  private void Start()
  {
    if (GetAudioSourcesOnStart)
      _audioSources = GetComponents<AudioSource>();

    Update3DAudio();
  }

  private void OnEnable()
  {
    if (Added != null)
    {
      Added(this);
    }
  }

  private void OnDisable()
  {
    if (Removed != null)
    {
      Removed(this);
    }
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(transform.position, Range);
  }
}