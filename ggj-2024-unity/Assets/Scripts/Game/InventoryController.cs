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

  public void AddItem(ItemController item)
  {
    if (!_pendingItemPickups.Contains(item))
    {
      item.SetCollidersEnabled(false);
      item.Rigidbody.isKinematic = true;
      _pendingItemPickups.Add(item);
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

      return itemController;
    }

    return null;
  }

  private void Update()
  {
    // Suck in items that we've picked up
    for (int i = 0; i < _pendingItemPickups.Count; ++i)
    {
      ItemController item = _pendingItemPickups[i];
      item.transform.position = Mathfx.Damp(item.transform.position, transform.position, 0.25f, Time.deltaTime * 8);
      item.transform.localScale = Mathfx.Damp(item.transform.localScale, Vector3.zero, 0.25f, Time.deltaTime);

      float dist = Vector3.Distance(item.transform.position, transform.position);
      if (dist < 0.2f)
      {
        _pendingItemPickups.RemoveAt(i);
        AddItem(item.ItemDefinition);
        Destroy(item.gameObject);
      }
    }
  }
}