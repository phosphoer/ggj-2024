using System;
using UnityEngine;

public class CookingPotController : MonoBehaviour
{
  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private InventoryController _inventory = null;

  [SerializeField]
  private Transform[] _cookingSlots = null;

  private void Awake()
  {
    _interactable.InteractionTriggered += OnInteract;
    _inventory.ItemAdded += OnItemAdded;
    _inventory.ItemRemoved += OnItemRemoved;
  }

  private void Update()
  {
    for (int i = 0; i < _cookingSlots.Length; ++i)
    {
      Transform cookingSlot = _cookingSlots[i];
      foreach (Transform child in cookingSlot)
      {
        child.localPosition = Vector3.up * Mathf.Sin(Time.time + i) * 0.1f;
      }
    }
  }

  private void OnTriggerEnter(Collider c)
  {
    ItemController item = c.GetComponentInParent<ItemController>();
    if (item != null)
    {
      _inventory.AddItem(item);
    }
  }

  private void OnItemAdded(ItemDefinition definition)
  {
    int currentSlotIndex = _cookingSlots.WrapIndex(_inventory.Items.Count);
    Transform currentSlot = _cookingSlots[currentSlotIndex];
    ItemController item = Instantiate(definition.Prefab, currentSlot);
    item.transform.SetIdentityTransformLocal();
    item.SetPhysicsEnabled(false);
    item.SetCollidersEnabled(false);
    item.SetInteractionEnabled(false);
  }

  private void OnItemRemoved(ItemDefinition definition)
  {
  }

  private void OnInteract(InteractionController controller)
  {
  }
}