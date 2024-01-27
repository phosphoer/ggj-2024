using UnityEngine;
using System.Collections.Generic;

public class ItemController : MonoBehaviour
{
  public Rigidbody Rigidbody => _rb;

  public ItemDefinition ItemDefinition;

  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private Rigidbody _rb = null;

  private List<Collider> _childColliders = new();

  public void SetCollidersEnabled(bool collidersEnabled)
  {
    foreach (var c in _childColliders)
      c.enabled = collidersEnabled;
  }

  public void SetInteractionEnabled(bool interactionEnabled)
  {
    _interactable.enabled = interactionEnabled;
  }

  public void SetPhysicsEnabled(bool physicsEnabled)
  {
    _rb.isKinematic = !physicsEnabled;
  }

  private void Awake()
  {
    _interactable.InteractionTriggered += OnInteraction;

    GetComponentsInChildren<Collider>(_childColliders);
  }

  private void OnInteraction(InteractionController controller)
  {
    PlayerActorController.Instance.Inventory.AddItem(this);
  }
}