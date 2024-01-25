using UnityEngine;

public class MonoBehaviourEvents : MonoBehaviour
{
  public event System.Action EventAwake;
  public event System.Action EventOnEnable;
  public event System.Action EventOnDisable;
  public event System.Action EventStart;
  public event System.Action EventOnDestroy;

  private void Awake()
  {
    EventAwake?.Invoke();
  }

  private void OnEnable()
  {
    EventOnEnable?.Invoke();
  }

  private void OnDisable()
  {
    EventOnDisable?.Invoke();
  }

  private void Start()
  {
    EventStart?.Invoke();
  }

  private void OnDestroy()
  {
    EventOnDestroy?.Invoke();
  }
}