using UnityEngine;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
  public event System.Action<ItemController> PickupStarted;
  public event System.Action<ItemDefinition> ItemAdded;
  public event System.Action<ItemDefinition> ItemRemoved;

  public IReadOnlyList<ItemDefinition> Items => _items;
  public Transform ItemSpawnAnchor => _itemSpawnAnchor;
  public Transform ItemCollectedAnchor => _itemCollectAnchor;

  [SerializeField]
  private Transform _itemSpawnAnchor = null;

  [SerializeField]
  private Transform _itemCollectAnchor = null;

  private List<ItemDefinition> _items = new();
  private List<ItemController> _pendingItemPickups = new();
  private List<float> _pendingItemPickupTimers = new();
  private List<Vector3> _pendingItemPickupOrigins = new();

  private const float kPickupDuration = 1f;

  public int GetItemCount(ItemDefinition itemDefinition)
  {
    int total = 0;
    foreach (var item in _items)
    {
      if (item == itemDefinition)
        total += 1;
    }

    return total;
  }

  public bool MatchesRecipe(RecipeDefinition recipe)
  {
    foreach (var ingredient in recipe.Ingredients)
    {
      if (GetItemCount(ingredient.Item) != ingredient.Count)
        return false;
    }

    return _items.Count == recipe.GetTotalIngredientCount();
  }

  public void AddItem(ItemController item)
  {
    if (!item.IsBeingCollected && !_pendingItemPickups.Contains(item))
    {
      item.SetCollidersEnabled(false);
      item.SetPhysicsEnabled(false);
      item.SetInteractionEnabled(false);
      item.IsBeingCollected = true;
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

  public bool RemoveItem(ItemDefinition item)
  {
    bool wasRemoved = _items.Remove(item);
    ItemRemoved?.Invoke(item);
    return wasRemoved;
  }

  public void ClearItems()
  {
    while (_items.Count > 0)
    {
      RemoveItem(_items[0]);
    }
  }

  public ItemController TossItem(ItemDefinition item, Vector3 force, bool markAsThrown = true)
  {
    if (RemoveItem(item))
    {
      ItemController itemController = Instantiate(item.Prefab);
      itemController.transform.position = _itemSpawnAnchor.position;
      itemController.transform.rotation = Random.rotation;
      itemController.Rigidbody.AddForce(force, ForceMode.VelocityChange);
      itemController.WasThrown = markAsThrown;
      itemController.SetInteractionEnabled(false);
      itemController.SetCollidersEnabled(false);
      itemController.StartCoroutine(Tween.DelayCall(1, () =>
      {
        if (itemController != null)
          itemController.SetInteractionEnabled(true);
      }));

      itemController.StartCoroutine(Tween.DelayCall(0.25f, () =>
      {
        if (itemController != null)
          itemController.SetCollidersEnabled(true);
      }));

      UIHydrate hydrate = itemController.gameObject.AddComponent<UIHydrate>();
      hydrate.Hydrate();

      return itemController;
    }

    return null;
  }

  private void Awake()
  {
    if (_itemCollectAnchor == null)
      _itemCollectAnchor = _itemSpawnAnchor;
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
      item.transform.position = Vector3.Lerp(pickupOrigin, _itemCollectAnchor.position, pickupT);
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