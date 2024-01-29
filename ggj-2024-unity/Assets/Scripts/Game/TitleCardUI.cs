using UnityEngine;
using System.Collections;

public class TitleCardUI : UIPageBase
{
  public AnimationCurve _titleCardAlphaCurve = default;

  [SerializeField]
  private CanvasGroup _titleCardAlpha = null;

  private IEnumerator Start()
  {
    yield return Tween.CustomTween(20, t =>
    {
      _titleCardAlpha.alpha = _titleCardAlphaCurve.Evaluate(t);
    });

    Hide();
  }
}