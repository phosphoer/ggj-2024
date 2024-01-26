using UnityEngine;

public class CameraControllerPlayer : CameraControllerBase
{
  public Transform TargetTransform;

  public Vector3 TargetOffset = new Vector3(5, 5, -5);

  public override void CameraStart()
  {
  }

  public override void CameraStop()
  {
  }

  public override void CameraUpdate()
  {
    Vector3 targetPos = TargetTransform.position + TargetOffset;
    MountPoint.position = targetPos;
    MountPoint.LookAt(TargetTransform.position);
  }
}