using UnityEngine;

public class SplitscreenAudioListener : MonoBehaviour
{
  public static event System.Action<SplitscreenAudioListener> Added;
  public static event System.Action<SplitscreenAudioListener> Removed;

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
}