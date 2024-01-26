using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIHydrate : MonoBehaviour
{
  public event System.Action Hydrated;
  public event System.Action Dehydrated;

  public bool IsAnimating => _isAnimating;
  public bool IsHydrated => _isHydrated;

  public bool HydrateOnEnable = false;
  public bool StartDehydrated = false;

  [SerializeField]
  private Transform _targetTransform = null;

  [SerializeField]
  private bool _useNormalizedScale = true;

  [SerializeField]
  private bool _enableRandomDelay = false;

  private Vector3 _startScale;
  private bool _isHydrated;
  private bool _isDehydrated;
  private bool _isAnimating;
  private float _animationTimer;
  private System.Action _finishCallback;

  private const float kHydrateTime = 0.45f;
  private const float kDehydrateTime = 0.45f;

  [ContextMenu("Hydrate")]
  public void HydrateIfNecesssary()
  {
    if (!_isHydrated)
      Hydrate();
  }

  [ContextMenu("Dehydrate")]
  public void DehydrateIfNecessary()
  {
    if (_isHydrated)
      Dehydrate();
  }

  public void Hydrate(System.Action finishCallback = null)
  {
    _isHydrated = true;
    _isDehydrated = false;
    _isAnimating = true;
    _finishCallback = finishCallback;
    _animationTimer = 0;
    enabled = true;
    gameObject.SetActive(true);
    _targetTransform.localScale = Vector3.zero;
  }

  public void Dehydrate(System.Action finishCallback = null)
  {
    _isHydrated = false;
    _isDehydrated = true;
    _isAnimating = true;
    _finishCallback = finishCallback;
    _animationTimer = 0;
    enabled = true;
    gameObject.SetActive(true);
  }

  private void Awake()
  {
    if (_targetTransform == null)
      _targetTransform = transform;

    _startScale = _useNormalizedScale ? Vector3.one : _targetTransform.localScale;

    if (StartDehydrated)
    {
      _targetTransform.localScale = Vector3.zero;
      _isHydrated = false;
      enabled = false;
      gameObject.SetActive(false);
    }
    else
    {
      _isHydrated = true;
    }
  }

  private void OnEnable()
  {
    if (HydrateOnEnable && !_isDehydrated)
    {
      Hydrate();
    }
  }

  private void OnDisable()
  {
  }

  private void Update()
  {
    _animationTimer += Time.unscaledDeltaTime;
    float duration = _isHydrated ? kHydrateTime : kDehydrateTime;
    var curve = _isHydrated ? GameGlobals.Instance.UIHydrateCurve : GameGlobals.Instance.UIDehydrateCurve;
    float animT = Mathf.Clamp01(_animationTimer / duration);
    float tCurve = curve.Evaluate(animT);
    _targetTransform.localScale = Vector3.one * tCurve;

    if (animT >= 1)
    {
      _isAnimating = false;
      enabled = false;

      if (_isHydrated)
      {
        Hydrated?.Invoke();
        _finishCallback?.Invoke();
      }
      else
      {
        Dehydrated?.Invoke();
        _finishCallback?.Invoke();
        gameObject.SetActive(false);
      }
    }
  }
}