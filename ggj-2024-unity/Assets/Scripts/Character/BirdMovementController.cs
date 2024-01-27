using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;

public class BirdMovementController : MonoBehaviour, ICharacterController
{
  public KinematicCharacterMotor Motor => _motor;
  public Vector3 LastAirVelocity => _lastAirVelocity;

  public Vector3 WorldThrottle;
  public bool IsSprinting;

  public float Drag = 1;
  public float MoveAirAccel = 5;
  public float MoveAccel = 5;
  public float AirSpeed = 3;
  public float MoveSpeed = 5;
  public float SprintSpeed = 10;
  public float RotateSpeed = 5;
  public float TakeoffPower = 10;
  public float PerchingTime = 0.25f;
  public float JumpScalableForwardSpeed = 1;
  public bool AllowJumpingWhenSliding = true;

  public enum MovementMode
  {
    Walking,
    Falling,
    TakeOffWindup,
    TakeOff,
    Flying,
    Landing,
    Perching,
    Perched
  }

  [SerializeField]
  private MovementMode _movementMode = MovementMode.Perched;
  public MovementMode MoveMode => _movementMode;

  [SerializeField]
  private float _timeInMovementMode= 0.0f;

  [SerializeField]
  private KinematicCharacterMotor _motor = null;

  [SerializeField]
  private Transform _visualRoot = null;

  [SerializeField]
  private float _standHeight = 1.6f;

  [SerializeField]
  private float _standCapsuleHeight = 2f;

  private bool _firedTakeoffImpulse= false;
  private bool _hasReachedTakeoffApex= false;
  private Vector3 _targetPerchLocation = Vector3.zero;
  private Transform _targetPerchTransform = null;
  private Vector3 _lastAirVelocity;
  private Collider[] _probedColliders = new Collider[8];

  public float GetSpeed()
  {
    return _motor.Velocity.magnitude;
  }

  public bool IsMoving()
  {
    return GetSpeed() > 0.01f;
  }

  public bool IsStationary()
  {
    return !IsMoving();
  }

  public bool IsPerching()
  {
    return _movementMode == MovementMode.Perching || _movementMode == MovementMode.Perched;
  }

  public bool CanWalk()
  {
    return _movementMode == MovementMode.Walking;
  }

  public bool CanTakeOff()
  {
    return _movementMode == MovementMode.Walking || _movementMode == MovementMode.Perched;
  }

  public void TakeOff()
  {
    bool isOnGround= (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround);

    if (_movementMode == MovementMode.Walking && isOnGround)
    {
      _firedTakeoffImpulse= false;
      _hasReachedTakeoffApex= false;
      SetMovementMode(MovementMode.TakeOffWindup);
    }
    else if (_movementMode == MovementMode.Perched)
    {
      SetMovementMode(MovementMode.Flying);
    }
  }

  public bool CanLand()
  {
    return _movementMode == MovementMode.Flying;
  }

  public void Land()
  {
    if (_movementMode == MovementMode.Flying)
    {
      _firedTakeoffImpulse= false;
      _hasReachedTakeoffApex= false;
      SetMovementMode(MovementMode.Landing);
    }
  }

  public void Perch(Vector3 pathDestinationLocation, Transform pathDestinationTransform)
  {
    if (_movementMode == MovementMode.Flying)
    {
      _targetPerchLocation= pathDestinationLocation;
      _targetPerchTransform= pathDestinationTransform;
      SetMovementMode(MovementMode.Perching);
    }
  }

  private void Awake()
  {
    _motor.CharacterController = this;
  }

  private void Update()
  {
    MovementMode newMovementMode= _movementMode;

    if (_movementMode == MovementMode.Perching)
    {
      Vector3 sourceLocation= this.transform.position;
      Vector3 targetLocation= _targetPerchTransform != null ? _targetPerchTransform.position : _targetPerchLocation;

      _timeInMovementMode+= Time.deltaTime;

      // Blend into the perch location
      this.transform.position =  Mathfx.Damp(sourceLocation, targetLocation, 0.25f, Time.deltaTime);

      if (_timeInMovementMode >= PerchingTime)
      {
        // Snap to target location
        this.transform.position= targetLocation;
        newMovementMode= MovementMode.Perched;
      }
    }

    SetMovementMode(newMovementMode);
  }

  public void BeforeCharacterUpdate(float deltaTime)
  {
  }

  public void AfterCharacterUpdate(float deltaTime)
  {
    MovementMode newMovementMode= _movementMode;

    switch (_movementMode)
    {
    case MovementMode.Walking:
      if (!Motor.GroundingStatus.IsStableOnGround)
      {
        newMovementMode= MovementMode.Falling;
      }
      break;
    case MovementMode.Falling:
      if (Motor.GroundingStatus.IsStableOnGround)
      {
        newMovementMode= MovementMode.Walking;
      }
      break;
    case MovementMode.TakeOffWindup:
      if (_firedTakeoffImpulse)
      {
        newMovementMode= MovementMode.TakeOff;
      }
      break;
    case MovementMode.TakeOff:
      if (!_hasReachedTakeoffApex && ProjectVelocityOnGravity() >= 0.0f)
      {
        _hasReachedTakeoffApex= true;
        newMovementMode= MovementMode.Flying;
      }
      break;
    case MovementMode.Flying:
      // Stay in this state unil we are told to leave it
      break;
    case MovementMode.Landing:
      if (Motor.GroundingStatus.IsStableOnGround)
      {
        newMovementMode= MovementMode.Walking;
      }
      break;
    case MovementMode.Perching:
      break;
    case MovementMode.Perched:
      // If our perch turned out to be on the ground, just go straigh to walking
      if (Motor.GroundingStatus.IsStableOnGround)
      {
        newMovementMode= MovementMode.Walking;
      }
      break;
    }

    SetMovementMode(newMovementMode);
  }

  private void SetMovementMode(MovementMode newMode)
  {
    if (newMode != _movementMode)
    {

      // When leaving Perched, turn the motor back on
      if (_movementMode == MovementMode.Perched)
      {
        _motor.enabled= true;

        // Detach from the perch
        if (_targetPerchTransform != null && this.transform.parent == _targetPerchTransform)
        {
          this.transform.parent= null;
          _targetPerchTransform= null;
        }
      }

      // When entering Perching, turn the motor off
      if (newMode == MovementMode.Perching)
      {
        _motor.enabled= false;
      }
      // When entering Perched, attach to the target perch transform
      else if (newMode == MovementMode.Perched && _targetPerchTransform != null)
      {
        this.transform.parent= _targetPerchTransform;
      }

      _timeInMovementMode= 0.0f;
      _movementMode= newMode;
    }
  }

  public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
  {
    // In all states 
    Vector3 moveDir = Motor.Velocity.WithY(0);
    if (moveDir.sqrMagnitude > 0)
    {
      Quaternion desiredRot = Quaternion.LookRotation(moveDir);
      currentRotation = Mathfx.Damp(currentRotation, desiredRot, 0.25f, deltaTime * RotateSpeed);
    }
  }

  public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    switch (_movementMode)
    {
    case MovementMode.Walking:
      UpdateWalkingVelocity(ref currentVelocity, deltaTime);
      break;
    case MovementMode.Falling:
      UpdateFallingVelocity(ref currentVelocity, deltaTime);
      break;
    case MovementMode.TakeOffWindup:
      UpdateTakeOffWindUpVelocity(ref currentVelocity, deltaTime);
      break;
    case MovementMode.TakeOff:
      UpdateTakeOffVelocity(ref currentVelocity, deltaTime);
      break;
    case MovementMode.Flying:
      UpdateFlyingVelocity(ref currentVelocity, deltaTime);
      break;
    case MovementMode.Landing:
      UpdateLandingVelocity(ref currentVelocity, deltaTime);
      break;
    case MovementMode.Perching:
      break;
    case MovementMode.Perched:
      break;
    }
  }

  private void UpdateWalkingVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    // Ground movement
    if (Motor.GroundingStatus.IsStableOnGround)
    {
      // Apply movement modifiers
      float currentSpeed = 1;
      if (IsSprinting)
        currentSpeed *= SprintSpeed;
      else
        currentSpeed *= MoveSpeed;

      float currentVelocityMagnitude = currentVelocity.magnitude;

      Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

      // Reorient velocity on slope
      currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

      // Calculate target velocity
      Vector3 inputRight = Vector3.Cross(WorldThrottle, Motor.CharacterUp);
      Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * WorldThrottle.magnitude;
      Vector3 targetMovementVelocity = reorientedInput * currentSpeed;

      // Smooth movement Velocity
      currentVelocity = Mathfx.Damp(currentVelocity, targetMovementVelocity, 0.25f, deltaTime * MoveAccel);
    }
  }

  private void UpdateFallingVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    ApplyAirControl(ref currentVelocity, deltaTime);
    ApplyGravity(ref currentVelocity, deltaTime);
    ApplyDrag(ref currentVelocity, deltaTime);
  }

  private void UpdateTakeOffWindUpVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    if (!_firedTakeoffImpulse)
    {
      // Calculate jump direction before ungrounding
      Vector3 jumpDirection = Motor.CharacterUp;
      if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
      {
        jumpDirection = Motor.GroundingStatus.GroundNormal;
      }

      // Makes the character skip ground probing/snapping on its next update. 
      // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
      Motor.ForceUnground();

      // Add to the return velocity and reset jump state
      currentVelocity += (jumpDirection * TakeoffPower) - Vector3.Project(currentVelocity, Motor.CharacterUp);
      currentVelocity += (WorldThrottle * JumpScalableForwardSpeed);

      _firedTakeoffImpulse= true;
    }

    ApplyGravity(ref currentVelocity, deltaTime);
    ApplyDrag(ref currentVelocity, deltaTime);
  }

  private void UpdateTakeOffVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    ApplyGravity(ref currentVelocity, deltaTime);
    ApplyDrag(ref currentVelocity, deltaTime);

    _lastAirVelocity = currentVelocity;
  }

  private void UpdateFlyingVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    ApplyAirControl(ref currentVelocity, deltaTime);
    ApplyDrag(ref currentVelocity, deltaTime);

    _lastAirVelocity = currentVelocity;
  }

  private void UpdateLandingVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    ApplyGravity(ref currentVelocity, deltaTime);
    ApplyDrag(ref currentVelocity, deltaTime);

    _lastAirVelocity = currentVelocity;
  }

  private void ApplyAirControl(ref Vector3 currentVelocity, float deltaTime)
  {
    // Add move input
    if (WorldThrottle.sqrMagnitude > 0f)
    {
      Vector3 addedVelocity = WorldThrottle * MoveAirAccel * deltaTime;
      Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

      // Limit air velocity from inputs
      if (currentVelocityOnInputsPlane.magnitude < AirSpeed)
      {
        // clamp addedVel to make total vel not exceed max vel on inputs plane
        Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, AirSpeed);
        addedVelocity = newTotal - currentVelocityOnInputsPlane;
      }
      else
      {
        // Make sure added vel doesn't go in the direction of the already-exceeding velocity
        if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
        {
          addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
        }
      }

      // Prevent air-climbing sloped walls
      if (Motor.GroundingStatus.FoundAnyGround)
      {
        if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
        {
          Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
          addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
        }
      }

      // Apply added velocity
      currentVelocity += addedVelocity;
    }
  }

  private void ApplyGravity(ref Vector3 currentVelocity, float deltaTime)
  {
    currentVelocity += Physics.gravity * deltaTime;
  }

  private void ApplyDrag(ref Vector3 currentVelocity, float deltaTime)
  {
    currentVelocity *= (1f / (1f + (Drag * deltaTime)));
  }

  public void PostGroundingUpdate(float deltaTime)
  {
    // Handle landing and leaving ground
    if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
    {
      OnLanded();
    }
    else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
    {
      OnLeaveStableGround();
    }
  }

  public float ProjectVelocityOnGravity()
  {
    return Vector3.Dot(Motor.Velocity, Physics.gravity);
  }

  public bool IsColliderValidForCollisions(Collider coll)
  {
    return true;
  }

  public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
  {
  }

  public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
  {
  }

  public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
  {
  }

  public void OnDiscreteCollisionDetected(Collider hitCollider)
  {
  }

  protected void OnLanded()
  {
    //TODO: Trigger landing particle FX
    //TODO: Trigger landing sound
  }

  protected void OnLeaveStableGround()
  {
  }
}