using System;
using UnityEngine;

public class InventorySelector : MonoBehaviour
{
  public bool IsVisible => _isVisible;
  public ItemDefinition SelectedItem => _selectedItem?.ItemDefinition;

  [SerializeField]
  private InventoryController _inventory = null;

  [SerializeField]
  private Transform _itemDisplayAnchor = null;

  private ItemController _selectedItem;
  private int _selectedIndex;
  private bool _isVisible;

  public void Show()
  {
    Debug.Log($"InventorySelector: Show");
    _isVisible = true;
    RefreshDisplay();
  }

  public void Hide()
  {
    Debug.Log($"InventorySelector: Hide");
    _isVisible = false;
    if (_selectedItem != null)
    {
      Destroy(_selectedItem.gameObject);
      _selectedItem = null;
    }
  }

  public void SelectNext()
  {
    Show();
    _selectedIndex = _inventory.Items.ClampIndex(_selectedIndex + 1);
    RefreshDisplay();
  }

  public void SelectPrevious()
  {
    Show();
    _selectedIndex = _inventory.Items.ClampIndex(_selectedIndex - 1);
    RefreshDisplay();
  }

  private void Awake()
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