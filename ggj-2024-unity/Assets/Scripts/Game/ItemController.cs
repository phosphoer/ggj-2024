using UnityEngine;
using System.Collections.Generic;

public class ItemController : MonoBehaviour
{
  public ItemDefinition ItemDefinition;

  [SerializeField]
  private Interactable _interactable = null;

  private List<Collider> _childColliders = new();

  public void SetCollidersEnabled(bool collidersEnabled)
  {
    foreach (var c in _childColliders)
      c.enabled = collidersEnabled;
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