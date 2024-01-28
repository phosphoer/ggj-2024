using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowAttachmentManager : MonoBehaviour
{
  public List<GameObject> LevelBlingTable = new List<GameObject>();

  [SerializeField]
  private BirdAnimatorController _birdAnimationController= null;

  [SerializeField]
  private CrowStatsManager _statsManager= null; 

  private GameObject _blingObject = null;
  private Animator _blingAnimator = null;
  private int _currentBlingLevel= -1;

  // Start is called before the first frame update
  void Start()
  {
    if (_statsManager != null)
    {
      _statsManager.LeveledUp += OnLeveledUp;
    }
  }

  private void OnLeveledUp(int newLevel)
  {
    if (_currentBlingLevel != newLevel)
    {
      if (_blingObject)
      {
        if (_birdAnimationController != null)
        {
          _birdAnimationController.RemoveBlingAnimator(_blingAnimator);
        }

        Destroy(_blingObject);
        _blingObject= null;
      }

      GameObject newPrefab= GetBlingPrefabForLevel(newLevel);
      if (newPrefab != null)
      {
        _blingObject= Instantiate(newPrefab, this.transform);

        _blingAnimator= _blingObject.GetComponent<Animator>();

        if (_birdAnimationController != null)
        {
          _birdAnimationController.AddBlingAnimator(_blingAnimator);
        }
      }

      _currentBlingLevel= newLevel;
    }
  }

  private GameObject GetBlingPrefabForLevel(int newLevel)
  {
    int blingIndex= newLevel - 1;

    if (blingIndex >= 0 && LevelBlingTable.Count > 0)
    {
      if (blingIndex < LevelBlingTable.Count)
      {
        return LevelBlingTable[blingIndex];
      }
      else
      {
        return LevelBlingTable[LevelBlingTable.Count - 1];
      }
    }
    
    return null;
  }
}
