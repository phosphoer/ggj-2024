using UnityEngine;

public class InventorySelector : MonoBehaviour
{
  public ItemDefinition SelectedItem => _selectedItem?.ItemDefinition;

  [SerializeField]
  private InventoryController _inventory = null;

  [SerializeField]
  private Transform _itemDisplayAnchor = null;

  private ItemController _selectedItem;
  private int _selectedIndex;

  public void Show()
  {
    _selectedIndex = 0;
    RefreshDisplay();
  }

  public void Hide()
  {
  }

  public void SelectNext()
  {
  }

  public void SelectPrevious()
  {
  }

  private void RefreshDisplay()
  {
    if (_selectedItem != null)
    {
      Destroy(_selectedItem.gameObject);
      _selectedItem = null;
    }

    _selectedIndex = _inventory.Items.ClampIndex(_selectedIndex);
    if (_selectedIndex < _inventory.Items.Count)
    {
      ItemDefinition itemDef = _inventory.Items[_selectedIndex];
      ItemController item = Instantiate(itemDef.Prefab, _itemDisplayAnchor);
      item.transform.SetIdentityTransformLocal();
      item.SetPhysicsEnabled(false);
      item.SetCollidersEnabled(false);
      item.SetInteractionEnabled(false);
      _selectedItem = item;
    }
  }
}