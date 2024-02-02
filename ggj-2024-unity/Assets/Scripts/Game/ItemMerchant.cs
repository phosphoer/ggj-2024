using UnityEngine;

public class ItemMerchant : MonoBehaviour
{
  [SerializeField]
  private InventoryController _inventory = null;

  [SerializeField]
  private ItemDefinition _coinItem = null;

  private void OnTriggerEnter(Collider c)
  {
    ItemController item = c.GetComponentInParent<ItemController>();
    if (item != null && item.WasThrown && item.ItemDefinition != _coinItem)
    {
      _inventory.AddItem(item);

      Vector3 toPlayer = PlayerActorController.Instance.transform.position - _inventory.ItemSpawnAnchor.position;
      for (int i = 0; i < item.ItemDefinition.ShopValue; ++i)
      {
        _inventory.AddItem(_coinItem);
        _inventory.TossItem(_coinItem, toPlayer.normalized.WithY(1) * 2 + Random.insideUnitSphere * 0.5f, markAsThrown: false);
      }
    }
  }
}