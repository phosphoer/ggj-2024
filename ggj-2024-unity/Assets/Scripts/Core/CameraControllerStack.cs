using UnityEngine;
using System.Collections.Generic;

public class CameraControllerStack : MonoBehaviour
{
  public Camera Camera
  {
    get { return _camera; }
    set { _camera = value; }
  }

  public Camera UICamera
  {
    get { return _uiCamera; }
    set { _uiCamera = value; }
  }

  public IReadOnlyList<CameraControllerBase> Stack => _cameraControllers;

  public CameraControllerBase CurrentCameraController
  {
    get { return _cameraControllers.Count > 0 ? _cameraControllers[_cameraControllers.Count - 1] : null; }
  }

  public float CurrentFovOverride
  {
    get { return _fovStack.Count > 0 ? _fovStack[_fovStack.Count - 1].Value : -1; }
  }

  [SerializeField]
  private Camera _camera = null;

  [SerializeField]
  private Camera _uiCamera = null;

  private List<CameraControllerBase> _cameraControllers = new List<CameraControllerBase>();
  private List<KeyValuePair<string, float>> _fovStack = new List<KeyValuePair<string, float>>();

  private Transform _proxyTransform = null;
  private bool _isShuttingDown;
  private List<CameraShakeInfo> _activeShakes = new List<CameraShakeInfo>();

  private struct CameraShakeInfo
  {
    public float Duration;
    public float Timer;
    public float Magnitude;
  }

  public void CameraShakeFromPosition(Vector3 fromPos, float radius, float magnitude, float duration)
  {
    float dist = Vector3.Distance(_camera.transform.position, fromPos);
    float shakeScale = 1.0f - Mathf.Clamp01(dist / radius);
    if (shakeScale > 0)
    {
      CameraShake(shakeScale * magnitude, duration);
    }
  }

  public void CameraShake(float magnitude, float duration)
  {
    _activeShakes.Add(new CameraShakeInfo()
    {
      Duration = duration,
      Timer = 0,
      Magnitude = magnitude
    });
  }

  public void PushController(CameraControllerBase cameraController)
  {
    if (_cameraControllers.Contains(cameraController))
    {
      return;
    }

    _cameraControllers.Add(cameraController);
    cameraController.CameraStart();
    EnsureCameraStack();
  }

  public void PopController(CameraControllerBase cameraController)
  {
    if (_cameraControllers.Remove(cameraController))
    {
      if (cameraController != null)
        cameraController.CameraStop();

      EnsureCameraStack();
    }
  }

  public void SwitchController(CameraControllerBase cameraController)
  {
    PopCurrentController();
    PushController(cameraController);
  }

  public void PopCurrentController()
  {
    PopController(CurrentCameraController);
  }

  public void PushFovOverride(string key, float fov)
  {
    _fovStack.Add(new KeyValuePair<string, float>(key, fov));
  }

  public void PopFovOverride(string key)
  {
    for (int i = 0; i < _fovStack.Count; ++i)
    {
      if (_fovStack[i].Key == key)
      {
        _fovStack.RemoveAt(i);
        return;
      }
    }
  }

  public void ClearStack()
  {
    _cameraControllers.Clear();
    EnsureCameraStack();
  }

  public void SnapTransformToTarget(float maxDist = 0)
  {
    if (_proxyTransform.localPosition.magnitude > maxDist)
    {
      _proxyTransform.localPosition = Vector3.zero;
      _proxyTransform.localRotation = Quaternion.identity;
    }
  }

  private void LateUpdate()
  {
    if (_cameraControllers.Count > 0)
    {
      if (CurrentCameraController == null)
      {
        PopCurrentController();
        return;
      }

      // Update the current controller
      CurrentCameraController.CameraUpdate();

      // Align camera with current mount point
      _proxyTransform.localPosition = Mathfx.Damp(_proxyTransform.localPosition, Vector3.zero, 0.5f, Time.unscaledDeltaTime * 3.0f);
      _proxyTransform.localRotation = Mathfx.Damp(_proxyTransform.localRotation, Quaternion.identity, 0.5f, Time.unscaledDeltaTime * 3.0f);

      // Handle FOV
      float desiredFov = CurrentCameraController.FieldOfView;
      if (_fovStack.Count > 0)
        desiredFov = _fovStack[_fovStack.Count - 1].Value;

      _camera.fieldOfView = Mathfx.Damp(_camera.fieldOfView, desiredFov, 0.25f, Time.unscaledDeltaTime * 2.0f);

      // Camera shake
      for (int i = 0; i < _activeShakes.Count; ++i)
      {
        CameraShakeInfo shakeInfo = _activeShakes[i];
        shakeInfo.Timer += Time.unscaledDeltaTime;
        _activeShakes[i] = shakeInfo;

        if (shakeInfo.Timer < shakeInfo.Duration)
        {
          float shakeT = 1.0f - Mathf.Clamp01(shakeInfo.Timer / shakeInfo.Duration);
          _proxyTransform.position += Random.insideUnitSphere * shakeInfo.Magnitude * shakeT * Time.unscaledDeltaTime * 30;
        }
        else
        {
          _activeShakes.RemoveAt(i);
          i -= 1;
        }
      }

      // Match actual camera transform to the target
      _camera.transform.SetPositionAndRotation(_proxyTransform.position, _proxyTransform.rotation);
    }
  }

  private void OnApplicationQuit()
  {
    _isShuttingDown = true;
  }

  private void OnProxyDestroyed()
  {
    if (!_isShuttingDown)
      CreateProxyTransform();
  }

  private void CreateProxyTransform()
  {
    _proxyTransform = new GameObject($"{name}-proxy").transform;
    _proxyTransform.SetPositionAndRotation(_camera.transform.position, _camera.transform.rotation);

    var objEvents = _proxyTransform.gameObject.AddComponent<MonoBehaviourEvents>();
    objEvents.EventOnDestroy += OnProxyDestroyed;
  }

  private bool IsTransformFucked(Transform t)
  {
    Vector3 pos = t.position;
    return float.IsNaN(pos.x)
      || float.IsNaN(pos.y)
      || float.IsNaN(pos.z)
      || Mathf.Abs(pos.x) > 10000
      || Mathf.Abs(pos.y) > 500
      || Mathf.Abs(pos.z) > 10000;
  }

  private bool IsCameraFucked()
  {
    return IsTransformFucked(_proxyTransform) || IsTransformFucked(_camera.transform);
  }

  private void RecoverFuckedCamera()
  {
    _proxyTransform.position = Vector3.zero;
    _camera.transform.position = _proxyTransform.position;
  }

  private void EnsureCameraStack()
  {
    if (_proxyTransform == null)
    {
      CreateProxyTransform();
    }

    if (_cameraControllers.Count > 0)
    {
      CameraControllerBase activeController = _cameraControllers[_cameraControllers.Count - 1];
      if (activeController != null)
      {
        activeController.enabled = true;
        _proxyTransform.SetParent(activeController.MountPoint, true);
      }
    }
    else
    {
      _proxyTransform.SetParent(null, true);
    }
  }
}