using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SplitscreenLayout
{
  public static event System.Action LayoutUpdated;

  private List<Camera> _activeCameras = new List<Camera>();

  private static Rect[] _gridLayout4 = new Rect[4]
  {
    new Rect(0.0f, 0.5f, 0.5f, 0.5f),
    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
    new Rect(0.5f, 0.0f, 0.5f, 0.5f),
    new Rect(0.0f, 0.0f, 0.5f, 0.5f),
  };
  private static Rect[] _gridLayout3 = new Rect[3]
  {
    new Rect(0, 0, 0.5f, 1.0f),
    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
    new Rect(0.5f, 0.0f, 0.5f, 0.5f),
  };

  private static Rect[] _gridLayout2 = new Rect[2]
  {
    new Rect(0.0f, 0.0f, 0.5f, 1.0f),
    new Rect(0.5f, 0.0f, 0.5f, 1.0f),
  };

  private static Rect[] _gridLayout1 = new Rect[1]
  {
    new Rect(0.0f, 0.0f, 1.0f, 1.0f),
  };

  public void SetEnabled(bool bEnable)
  {
    foreach (Camera camera in _activeCameras)
    {
      camera.enabled = bEnable;
    }
  }

  public void AddCamera(Camera cam)
  {
    _activeCameras.Add(cam);
    UpdateViewports();
  }

  public void RemoveCamera(Camera cam)
  {
    _activeCameras.Remove(cam);
    UpdateViewports();
  }

  private void UpdateViewports()
  {
    Rect[] gridLayout = _gridLayout4;
    if (_activeCameras.Count == 3)
      gridLayout = _gridLayout3;
    else if (_activeCameras.Count == 2)
      gridLayout = _gridLayout2;
    else if (_activeCameras.Count == 1)
      gridLayout = _gridLayout1;

    for (int i = 0; i < _activeCameras.Count; ++i)
    {
      _activeCameras[i].rect = gridLayout[i];
    }

    LayoutUpdated?.Invoke();
  }
}