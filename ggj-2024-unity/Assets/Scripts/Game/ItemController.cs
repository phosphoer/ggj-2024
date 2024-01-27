using UnityEngine;
using System.Collections.Generic;

public class ItemController : MonoBehaviour
{
  public static IReadOnlyList<ItemController> Instances => _instances;

  public Rigidbody Rigidbody => _rb;
  public bool WasThrown { get; set; }
  public bool IsBeingCollected { get; set; }

  public ItemDefinition ItemDefinition;

  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private Rigidbody _rb = null;

  private List<Collider> _childColliders = new();
  private static List<ItemController> _instances = new();

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
    _instances.Add(this);

    GetComponentsInChildren<Collider>(_childColliders);
  }

  private void OnDestroy()
  {
    _instances.Remove(this);
  }

  private void OnInteraction(InteractionController controller)
  {
    PlayerActorController.Instance.Inventory.AddItem(this);
  }
}