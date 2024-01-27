using UnityEngine;

public class CameraControllerPlayer : CameraControllerBase
{
  public Transform TargetTransform;
  public Vector3 LookOffset = new Vector3(0, 1, 0);

  [SerializeField]
  private Transform _yawRoot = null;

  [SerializeField]
  private Transform _pitchRoot = null;

  private float _anglePitch;
  private float _angleYaw;

  public override void CameraStart()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  public override void CameraStop()
  {
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public override void CameraUpdate()
  {
    _angleYaw += AxisX * Time.deltaTime * 500;
    _anglePitch += AxisY * Time.deltaTime * -500;

    _anglePitch = Mathf.Clamp(_anglePitch, 0, 60);

    _yawRoot.localRotation = Quaternion.Euler(0, _angleYaw, 0);
    _pitchRoot.localRotation = Quaternion.Euler(_anglePitch, 0, 0);

    Vector3 targetPos = TargetTransform.position + LookOffset;
    transform.position = targetPos;
    MountPoint.LookAt(TargetTransform.position + LookOffset);
  }
}