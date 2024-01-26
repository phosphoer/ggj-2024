using UnityEngine;

public class PlayerUI : Singleton<PlayerUI>
{
  public WorldAttachedUI WorldUI = null;

  [SerializeField]
  private Canvas _canvas = null;

  private void Awake()
  {
    Instance = this;
  }

  private void Start()
  {
    _canvas.worldCamera = MainCamera.Instance.Camera;
    _canvas.planeDistance = 1f;
  }
}