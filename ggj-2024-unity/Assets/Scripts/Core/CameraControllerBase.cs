using UnityEngine;

public abstract class CameraControllerBase : MonoBehaviour
{
  public float AxisX { get; set; }
  public float AxisY { get; set; }
  public float AxisZ { get; set; }
  public Transform MountPoint => _mountPoint != null ? _mountPoint : transform;

  public float FieldOfView = 65.0f;

  [SerializeField]
  private Transform _mountPoint = null;

  public void MatchCameraTransform()
  {
    MountPoint.SetPositionAndRotation(MainCamera.Instance.transform.position, MainCamera.Instance.transform.rotation);
  }

  public abstract void CameraStart();
  public abstract void CameraUpdate();
  public abstract void CameraStop();

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.white;
    Gizmos.matrix = transform.localToWorldMatrix;
    Gizmos.DrawFrustum(Vector3.zero, FieldOfView, 1, 0.1f, Screen.width / Screen.height);
    Gizmos.matrix = Matrix4x4.identity;
  }
}