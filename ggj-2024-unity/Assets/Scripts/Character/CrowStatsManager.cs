using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowStatsManager : MonoBehaviour
{
  private int _xp= 0;

  public void ApplyItemStats(ItemController item)
  {
    _xp+= item.ItemDefinition.CrowXP;
  }
}
