using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
  public static IReadOnlyList<Interactable> Instances => _instances;

  public event System.Action<InteractionController> InteractionTriggered;

  public Transform InteractionUIAnchor => _interactionUIAnchor;
  public float InteractionUIHeight => _interactionUIHeight;
  public InteractableUI InteractableUIPrefab => _interactableUIPrefab;
  public string InteractionText => _disabledStack.Count > 0 ? _disabledStack[_disabledStack.Count - 1] : _interactionText;

  public float InteractionRadius
  {
    get { return _interactionRadius; }
    set { _interactionRadius = value; }
  }

  public bool IsInteractionEnabled
  {
    get { return _disabledStack.Count == 0 && _enableInteraction && enabled; }
  }

  public bool AutoInteract
  {
    get { return _autoInteract; }
    set { _autoInteract = value; }
  }

  public bool EnableInteraction
  {
    get => _enableInteraction;
    set => _enableInteraction = true;
  }

  public bool RequiresLineOfSight => _requiresLineOfSight;

  [SerializeField]
  private InteractableUI _interactableUIPrefab = null;

  [SerializeField]
  private Transform _interactionUIAnchor = null;

  [SerializeField]
  private float _interactionUIHeight = 0.5f;

  [SerializeField]
  private string _interactionText = "Interact";

  [SerializeField]
  private float _interactionRadius = 2.0f;

  [SerializeField]
  private bool _requiresLineOfSight = false;

  [SerializeField]
  private bool _autoInteract = false;

  [SerializeField]
  private bool _enableInteraction = true;

  [SerializeField]
  private bool _disableOnInteract = false;

  private List<string> _disabledStack = new List<string>();

  private static List<Interactable> _instances = new List<Interactable>();

  private void OnEnable()
  {
    _instances.Add(this);

    if (_interactionUIAnchor == null)
    {
      _interactionUIAnchor = transform;
    }
  }

  private void OnDisable()
  {
    _instances.Remove(this);
  }

  public IEnumerator WaitForInteractAsync(bool enableAndDisable = true)
  {
    bool didInteract = false;
    System.Action<InteractionController> onInteract = (InteractionController) =>
    {
      didInteract = true;
    };

    InteractionTriggered += onInteract;

    if (enableAndDisable)
      enabled = true;

    while (!didInteract)
      yield return null;

    InteractionTriggered -= onInteract;

    if (enableAndDisable)
      enabled = false;
  }

  public void PushDisabledState()
  {
    _disabledStack.Add(string.Empty);
  }

  public void PopDisabledState()
  {
    if (_disabledStack.Count > 0)
    {
      _disabledStack.RemoveAt(_disabledStack.Count - 1);
    }
  }

  public void PushDisabledState(string disabledText)
  {
    _disabledStack.Add(disabledText);
  }

  public void PopDisabledState(string disabledText)
  {
    _disabledStack.Remove(disabledText);
  }

  public void TriggerInteraction(InteractionController controller)
  {
    if (IsInteractionEnabled)
    {
      InteractionTriggered?.Invoke(controller);

      if (_disableOnInteract)
        enabled = false;
    }
  }

#if UNITY_EDITOR
  private void OnDrawGizmosSelected()
  {
    if (_interactionUIAnchor != null)
    {
      Gizmos.color = Color.white;
      Gizmos.DrawWireSphere(transform.position + Vector3.up * _interactionUIHeight, 0.1f);
    }
  }
#endif
}