using UnityEngine;
using System.Collections.Generic;

public class CrowTarget : MonoBehaviour
{
  public static IReadOnlyList<CrowTarget> Instances => _instances;

  [SerializeField]
  private GameObject _targetedHighlightPrefab = null;

  [SerializeField]
  private ItemDefinition _itemReward = null;

  private RectTransform _targetHighlightRoot;

  private static List<CrowTarget> _instances = new();

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

  private void OnEnable()
  {
    _instances.Add(this);
  }

  private void OnDisable()
  {
    _instances.Remove(this);
  }
}