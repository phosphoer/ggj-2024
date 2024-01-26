using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;

public class ActorController : MonoBehaviour, ICharacterController
{
  public KinematicCharacterMotor Motor => _motor;
  public Vector3 LastAirVelocity => _lastAirVelocity;

  public Vector2 MoveAxis;
  public bool IsSprinting;

  public float Drag = 1;
  public float MoveAirAccel = 5;
  public float MoveAccel = 5;
  public float AirSpeed = 3;
  public float MoveSpeed = 5;
  public float SprintSpeed = 10;
  public float JumpPower = 1;
  public float JumpScalableForwardSpeed = 1;
  private float JumpPostGroundingGraceTime = 0f;
  private float JumpPreGroundingGraceTime = 0f;
  public bool AllowJumpingWhenSliding = true;

  [SerializeField]
  private KinematicCharacterMotor _motor = null;

  [SerializeField]
  private Transform _visualRoot = null;

  [SerializeField]
  private float _standHeight = 1.6f;

  [SerializeField]
  private float _standCapsuleHeight = 2f;

  private bool _isJumpQueued;
  private bool _jumpedThisFrame;
  private float _timeSinceJumpRequested;
  private bool _jumpConsumed;
  private float _timeSinceLastAbleToJump;
  private Vector3 _lastAirVelocity;
  private Collider[] _probedColliders = new Collider[8];

  public void Jump()
  {
    _timeSinceJumpRequested = 0;
    _isJumpQueued = true;
  }

  private void Awake()
  {
    _motor.CharacterController = this;
  }

  private void Update()
  {
  }

  public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
  {
  }

  public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    // Calculate move direction
    Vector3 walkDirection = transform.forward.WithY(0).normalized;
    Vector3 strafeDirection = transform.right.WithY(0).normalized;
    Vector3 moveVec = Vector3.ClampMagnitude((walkDirection * MoveAxis.y + strafeDirection * MoveAxis.x), 1);

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
      Vector3 inputRight = Vector3.Cross(moveVec, Motor.CharacterUp);
      Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveVec.magnitude;
      Vector3 targetMovementVelocity = reorientedInput * currentSpeed;

      // Smooth movement Velocity
      currentVelocity = Mathfx.Damp(currentVelocity, targetMovementVelocity, 0.25f, deltaTime * MoveAccel);
    }
    // Air movement 
    else
    {
      // Add move input
      if (moveVec.sqrMagnitude > 0f)
      {
        Vector3 addedVelocity = moveVec * MoveAirAccel * deltaTime;
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

      // Gravity and drag
      currentVelocity += Physics.gravity * deltaTime;
      currentVelocity *= (1f / (1f + (Drag * deltaTime)));

      _lastAirVelocity = currentVelocity;
    }

    // Handle jumping
    _jumpedThisFrame = false;
    _timeSinceJumpRequested += deltaTime;
    if (_isJumpQueued)
    {
      // See if we actually are allowed to jump
      if (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
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
        currentVelocity += (jumpDirection * JumpPower) - Vector3.Project(currentVelocity, Motor.CharacterUp);
        currentVelocity += (moveVec * JumpScalableForwardSpeed);
        _isJumpQueued = false;
        _jumpConsumed = true;
        _jumpedThisFrame = true;
      }
    }
  }

  public void BeforeCharacterUpdate(float deltaTime)
  {
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

  public void AfterCharacterUpdate(float deltaTime)
  {
    // Handle jump-related values
    {
      // Handle jumping pre-ground grace period
      if (_isJumpQueued && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
      {
        _isJumpQueued = false;
      }

      if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
      {
        // If we're on a ground surface, reset jumping values
        if (!_jumpedThisFrame)
        {
          _jumpConsumed = false;
        }

        _timeSinceLastAbleToJump = 0f;
      }
      else
      {
        // Keep track of time since we were last able to jump (for grace period)
        _timeSinceLastAbleToJump += deltaTime;
      }
    }
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
  }

  protected void OnLeaveStableGround()
  {
  }
}