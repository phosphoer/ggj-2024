using UnityEngine;
using System.Collections.Generic;

public class CrowTarget : MonoBehaviour
{
  public static IReadOnlyList<CrowTarget> Instances => _instances;

  public bool IsVisible => _targetHighlightRoot != null;

  public float GatherTimeLvl0 = 30.0f;
  public float GatherTimeLvl1 = 25.0f;
  public float GatherTimeLvl2 = 20.0f;
  public float GatherTimeLvl3 = 15.0f;
  public float GatherTimeLvl4 = 10.0f;
  public float GatherTimeLvl5 = 5.0f;

  [SerializeField]
  private GameObject _targetedHighlightPrefab = null;

  [SerializeField]
  private ItemDefinition _itemReward = null;

  [SerializeField]
  private PerchController _perch = null;
  public PerchController Perch => _perch;

  private RectTransform _targetHighlightRoot;

  private static List<CrowTarget> _instances = new();

  public bool IsInLineOfSight()
  {
    return Vector3.Distance(transform.position, PlayerActorController.Instance.transform.position) < PlayerActorController.kSelectMaxRadius;
  }

  public ItemDefinition GetItemRewardDefinition()
  {
    return _itemReward;
  }

  public void ShowTargetHighlight()
  {
    if (_targetHighlightRoot == null)
    {
      _targetHighlightRoot = PlayerUI.Instance.WorldUI.ShowItem(transform, Vector3.zero);
      Instantiate(_targetedHighlightPrefab, _targetHighlightRoot);
    }
  }

  public void HideTargetHighlight()
  {
    if (_targetHighlightRoot != null)
    {
      PlayerUI.Instance.WorldUI.HideItem(_targetHighlightRoot);
    }
  }

  public void SelectHighlight()
  {
    // Todo: proper highlight
    if (_targetHighlightRoot != null)
      _targetHighlightRoot.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
  }

  public void UnselectHighlight()
  {
    // Todo: proper highlight
    if (_targetHighlightRoot != null)
      _targetHighlightRoot.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
  }

  private void OnEnable()
  {
    _instances.Add(this);
  }

  private void OnDisable()
  {
    _instances.Remove(this);
  }
}