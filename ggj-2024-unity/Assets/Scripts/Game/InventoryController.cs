using UnityEngine;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
  public event System.Action<ItemController> PickupStarted;
  public event System.Action<ItemDefinition> ItemAdded;
  public event System.Action<ItemDefinition> ItemRemoved;

  public IReadOnlyList<ItemDefinition> Items => _items;

  [SerializeField]
  private Transform _itemSpawnAnchor = null;

  private List<ItemDefinition> _items = new();
  private List<ItemController> _pendingItemPickups = new();
  private List<float> _pendingItemPickupTimers = new();
  private List<Vector3> _pendingItemPickupOrigins = new();

  private const float kPickupDuration = 1f;

  public void AddItem(ItemController item)
  {
    if (!_pendingItemPickups.Contains(item))
    {
      item.SetCollidersEnabled(false);
      item.SetPhysicsEnabled(false);
      item.SetInteractionEnabled(false);
      _pendingItemPickups.Add(item);
      _pendingItemPickupTimers.Add(0);
      _pendingItemPickupOrigins.Add(item.transform.position);
      PickupStarted?.Invoke(item);
    }
  }

  public void AddItem(ItemDefinition item)
  {
    _items.Add(item);
    ItemAdded?.Invoke(item);
  }

  public void RemoveItem(ItemDefinition item)
  {
    _items.Remove(item);
    ItemRemoved?.Invoke(item);
  }

  public ItemController TossItem(ItemDefinition item, Vector3 force)
  {
    if (_items.Remove(item))
    {
      ItemController itemController = Instantiate(item.Prefab);
      itemController.transform.position = _itemSpawnAnchor.position;
      itemController.transform.rotation = Random.rotation;
      itemController.Rigidbody.AddForce(force, ForceMode.VelocityChange);
      itemController.SetInteractionEnabled(false);
      itemController.StartCoroutine(Tween.DelayCall(1, () =>
      {
        if (itemController != null)
          itemController.SetInteractionEnabled(true);
      }));

      return itemController;
    }

    return null;
  }

  private void Update()
  {
    // Suck in items that we've picked up
    for (int i = 0; i < _pendingItemPickups.Count; ++i)
    {
      _pendingItemPickupTimers[i] += Time.deltaTime;
      float pickupTimer = _pendingItemPickupTimers[i];
      Vector3 pickupOrigin = _pendingItemPickupOrigins[i];
      float pickupT = Mathf.SmoothStep(0, 1, pickupTimer / kPickupDuration);

      ItemController item = _pendingItemPickups[i];
      item.transform.position = Vector3.Lerp(pickupOrigin, _itemSpawnAnchor.position, pickupT);
      item.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, pickupT);

      if (pickupT >= 1)
      {
        _pendingItemPickups.RemoveAt(i);
        _pendingItemPickupOrigins.RemoveAt(i);
        _pendingItemPickupTimers.RemoveAt(i);
        AddItem(item.ItemDefinition);
        Destroy(item.gameObject);
      }
    }
  }
}