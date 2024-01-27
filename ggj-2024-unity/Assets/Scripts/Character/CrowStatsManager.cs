using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowStatsManager : MonoBehaviour
{
  [SerializeField]
  public int[] _levelXPThresholds = new int[] { };

  private int _xp= 0;

  public int Level => _level;
  private int _level= 0;

  public event System.Action<int> LeveledUp;

  public void ApplyItemStats(ItemController item)
  {
    _xp+= item.ItemDefinition.CrowXP;

    int newLevel= ComputeLevelForXP(_xp);
    if (newLevel > _level)
    {
      LeveledUp?.Invoke(newLevel);
    }
  }

  private int ComputeLevelForXP(int XP)
  {
    int level= 0;

    foreach (int xpThreshold in _levelXPThresholds)
    {
      if (XP >= xpThreshold)
      {
        level++;
      }
      else
      {
        break;
      }
    }
    return level;
  }
}
