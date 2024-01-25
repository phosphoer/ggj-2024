using UnityEngine;

public class CameraControllerDynamic : CameraControllerBase
{
  public float AxisSensitivty = 1;
  public float MaxAngle = 5.0f;
  public float HandheldScale = 1;

  private Vector2 _smoothAxis;
  private Vector3 _baseLocalDirection;
  private Vector3 _panningOffset;
  private Transform _panningTarget;

  public void SetBaseDirection()
  {
    SetBaseDirection(MountPoint.forward);
  }

  public void SetBaseDirection(Vector3 directionOverride)
  {
    if (transform.parent != null)
    {
      _baseLocalDirection = transform.parent.InverseTransformDirection(directionOverride);
    }
    else
    {
      _baseLocalDirection = directionOverride;
    }
  }

  public Vector3 GetBaseWorldDirection()
  {
    if (transform.parent != null)
    {
      return transform.parent.TransformDirection(_baseLocalDirection);
    }

    return _baseLocalDirection;
  }

  public void SetPanningTarget(Transform panningTarget)
  {
    _panningTarget = panningTarget;

    if (_panningTarget != null)
      _panningOffset = MountPoint.position - panningTarget.position;
  }

  private void Awake()
  {
    SetBaseDirection();
  }

  public override void CameraStart()
  {
    SetBaseDirection();
  }

  public override void CameraStop()
  {

  }

  public override void CameraUpdate()
  {
    float dt = Time.unscaledDeltaTime;
    float y = AxisX * AxisSensitivty * 5;
    float x = AxisY * -AxisSensitivty * 5;

    if (HandheldScale != 0)
    {
      float freq = 0.2f;
      float time = Time.unscaledTime * freq;
      float noiseX = Mathf.Clamp(Mathf.PerlinNoise(time, time + 20) - 0.5f, -0.5f, 0.5f);
      float noiseY = Mathf.Clamp(Mathf.PerlinNoise(time + 100, time + 130) - 0.5f, -0.5f, 0.5f);
      x += noiseX * HandheldScale * 3;
      y += noiseY * HandheldScale * 3;
    }

    _smoothAxis = Mathfx.Damp(_smoothAxis, new Vector2(x, y), 0.25f, dt * 5.0f);

    // Rotate mount point
    MountPoint.Rotate(0, _smoothAxis.y * dt, 0, Space.World);
    MountPoint.Rotate(_smoothAxis.x * dt, 0, 0, Space.Self);

    // Clamp rotation 
    Vector3 baseWorldDir = GetBaseWorldDirection();
    float angleToBase = Vector3.Angle(baseWorldDir, MountPoint.forward);
    float maxDelta = Mathf.Max(angleToBase - MaxAngle, 0);
    Quaternion baseRot = Quaternion.LookRotation(baseWorldDir);
    MountPoint.rotation = Mathfx.Damp(MountPoint.rotation, baseRot, 0.5f, dt * maxDelta);

    if (_panningTarget != null)
    {
      MountPoint.position = _panningTarget.position + _panningOffset;
    }
  }
}
