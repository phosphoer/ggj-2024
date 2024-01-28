using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineController : MonoBehaviour
{
  public int NeededCoins= 10;

  [SerializeField]
  private int _depositCount= 0;

  [SerializeField]
  private InventoryController _inventory = null;

  [SerializeField]
  private ItemDefinition _coinItem = null;

  [SerializeField]
  public GameObject[] _deadTrees = new GameObject[] { };

  [SerializeField]
  public GameObject[] _aliveTrees = new GameObject[] { };

  public event System.Action CoinDeposited;
  public event System.Action ShrineFilled;

  private void OnTriggerEnter(Collider c)
  {
    if (_depositCount < NeededCoins)
    {
      ItemController item = c.GetComponentInParent<ItemController>();
      if (item != null && item.WasThrown && item.ItemDefinition == _coinItem)
      {
        _depositCount++;

        _inventory.AddItem(item);
        CoinDeposited?.Invoke();

        if (_depositCount >= NeededCoins)
        {
          ShrineFilled?.Invoke();

          foreach (GameObject deadTree in _deadTrees)
          {
            deadTree.SetActive(false);
          }

          foreach (GameObject aliveTree in _aliveTrees)
          {
            aliveTree.SetActive(true);
          }
        }
      }
    }
  }
}
