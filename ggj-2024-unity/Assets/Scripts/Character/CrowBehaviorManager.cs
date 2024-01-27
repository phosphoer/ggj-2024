using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowBehavior : MonoBehaviour
{
  public enum BehaviorState
  {
    Wander = 0,
    Idle,
    ApproachPlayer,
    Attack,
    Dead,
    Flee
  };

  public BirdMovementController BirdMovement => _birdMovement;
  public BirdPerceptionComponent Perception => _perceptionComponent;
  public BirdAnimatorController BirdAnimator => _birdAnimator;

  [SerializeField]
  private BirdPerceptionComponent _perceptionComponent = null;

  [SerializeField]
  private BirdAnimatorController _birdAnimator = null;

  [SerializeField]
  private BirdMovementController _birdMovement = null;

  [SerializeField]
  private SoundBank _cawSound = null;

  [SerializeField]
  private GameObject _attackFX = null;

  [SerializeField]
  private SoundBank _attackSound = null;

  [SerializeField]
  private GameObject _deathFX = null;

  [SerializeField]
  private SoundBank _deathSound = null;

  // Behavior State
  private BehaviorState _behaviorState = BehaviorState.Idle;
  private float _timeInBehavior = 0.0f;
  //-- Idle --
  public float IdleMinDuration = 0.5f;
  public float IdleMaxDuration = 3.0f;
  private float _idleDuration = 0.0f;
  //-- Wander --
  public float WanderRange = 10.0f;
  //-- PlayerApproach --
  public float PlayerApproachTimeOut = 4.0f;
  public float PlayerApproachRange = 30.0f;
  //-- Attack --
  public float AttackRange = 2.0f;
  public float AttackDuration = 2.0f;
  public float AttackTurnSpeed = 5.0f;
  public float AttackCooldown = 5.0f;
  public float _timeSinceAttack = -1.0f;
  public bool HasAttackedRecently
  {
    get { return (_timeSinceAttack >= 0 && _timeSinceAttack < AttackCooldown); }
  }

  // Path Finding State
  public float WaypointTolerance = 1.0f;
  public bool DebugDrawPath = false;
  List<Vector3> _lastPath = new List<Vector3>();
  float _pathRefreshPeriod = -1.0f;
  float _pathRefreshTimer = 0.0f;
  int _pathWaypointIndex = 0;
  float _pathfollowingStuckTimer = 0.0f;
  public bool IsPathFinished
  {
    // Hit end of the path
    get { return (_pathWaypointIndex >= _lastPath.Count); }
  }
  public bool CantMakePathProgress
  {
    // Got stuck on some geomtry following current path
    get { return _pathfollowingStuckTimer > 1.0f; }
  }
  public bool IsPathStale
  {
    get
    {
      return
      (_pathRefreshPeriod >= 0.0f && _pathRefreshTimer <= 0.0f) || // time for a refresh
      IsPathFinished || // Hit end of the path
      CantMakePathProgress; // Got stuck on some geomtry following current path
    }
  }

  Vector3 _spawnLocation = Vector3.zero;

  // Throttle State
  private Vector3 _throttleTarget = Vector3.zero;
  private float _throttleUrgency = 0.5f;
  private bool _hasValidThrottleTarget = false;

  public bool DebugUsePlayerControls= false;
  private Rewired.Player _rewiredPlayer;

  private void Awake()
  {
    _rewiredPlayer = Rewired.ReInput.players.GetPlayer(0);
  }

  private void Update()
  {
    if (_behaviorState == BehaviorState.Dead)
      return;

    UpdateBehavior();
    UpdatePathRefreshTimer();
    UpdatePathFollowing();
    UpdateMoveVector();
    UpdateAnimationParameters();

    if (DebugDrawPath)
    {
      DrawPath();
    }
  }

  void UpdateBehavior()
  {
    BehaviorState nextBehavior = _behaviorState;

    // Used for attack cooldown
    if (_behaviorState != BehaviorState.Attack && _timeSinceAttack >= 0)
    {
      _timeSinceAttack += Time.deltaTime;
    }

    switch (_behaviorState)
    {
      case BehaviorState.Idle:
        nextBehavior = UpdateBehavior_Idle();
        break;
      case BehaviorState.Wander:
        nextBehavior = UpdateBehavior_Wander();
        break;
    case BehaviorState.ApproachPlayer:
      nextBehavior = UpdateBehavior_ApproachPlayer();
      break;
    //case BehaviorState.Cower:
    //  nextBehavior = UpdateBehavior_Cower();
    //  break;
    case BehaviorState.Attack:
        nextBehavior = UpdateBehavior_Attack();
        break;
      //case BehaviorState.Flee:
      //  nextBehavior = UpdateBehavior_Flee();
      //  break;
      case BehaviorState.Dead:
        break;
    }

    SetBehaviorState(nextBehavior);
  }

  void SetBehaviorState(BehaviorState nextBehavior)
  {
    if (nextBehavior != _behaviorState)
    {
      OnBehaviorStateExited(_behaviorState);
      OnBehaviorStateEntered(nextBehavior);
      _behaviorState = nextBehavior;

      _timeInBehavior = 0.0f;
    }
    else
    {
      _timeInBehavior += Time.deltaTime;
    }
  }

  BehaviorState UpdateBehavior_Idle()
  {
    BehaviorState nextBehavior = BehaviorState.Idle;

    // Spotted the player
    if (_perceptionComponent.CanHearPlayer)
    {
      nextBehavior = BehaviorState.ApproachPlayer;
    }
    // Been in idle too long, go somewhere else
    else if (_timeInBehavior >= _idleDuration)
    {
      nextBehavior = BehaviorState.Wander;
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Wander()
  {
    BehaviorState nextBehavior = BehaviorState.Wander;

    // Spotted the player
    if (_perceptionComponent.CanHearPlayer)
    {
      nextBehavior = BehaviorState.ApproachPlayer;
    }
    // Have we reached our path destination, chill for a bit
    else if (IsPathFinished || CantMakePathProgress)
    {
      nextBehavior = BehaviorState.Idle;
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_ApproachPlayer()
  {
    BehaviorState nextBehavior = BehaviorState.ApproachPlayer;

    Vector3 attackTarget = GetCurrentPlayerLocation();

    if (IsWithingDistanceToTarget2D(attackTarget, AttackRange))
    {
      nextBehavior = BehaviorState.Attack;
    }

    // Can see and keep pathing to the player?
    if (nextBehavior == BehaviorState.ApproachPlayer)
    {
      // Use the current player location rather than stale perception location to prevent oscillation
      Vector3 pathTarget = GetCurrentPlayerLocation();

      if (IsPathStale)
      {
        if (PathFindManager.Instance.IsPointTraversable(pathTarget))
        {
          if (RecomputePathTo(pathTarget))
          {
            nextBehavior = BehaviorState.ApproachPlayer;
          }
          else
          {
            // Can't path to target, flee
            nextBehavior = BehaviorState.Flee;
          }
        }
        else
        {
          // Target location not traversable, flee
          nextBehavior = BehaviorState.Flee;
        }
      }
      else
      {
        // Path not stale, keep approaching
        nextBehavior = BehaviorState.ApproachPlayer;
      }
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Attack()
  {
    BehaviorState nextBehavior = BehaviorState.Attack;

    if (_timeInBehavior > AttackDuration)
    {
      nextBehavior = BehaviorState.Flee;
    }
    //else
    //{
    // TODO: Throttle at enemy
    //  SetThrottleTarget()
    //}

    return nextBehavior;
  }

  void OnBehaviorStateExited(BehaviorState oldBehavior)
  {
    switch (oldBehavior)
    {
      case BehaviorState.Idle:
        break;
      case BehaviorState.Wander:
        break;
      case BehaviorState.ApproachPlayer:
        // Stop forcing line of sight checks 
        _perceptionComponent.ForceLineOfSightCheck = false;
        _perceptionComponent.ResetPlayerSpotTimer();
        break;
      //case BehaviorState.Cower:
      //  break;
      case BehaviorState.Attack:
        break;
    case BehaviorState.Flee:
      break;
    case BehaviorState.Dead:
        break;
    }
  }

  void OnBehaviorStateEntered(BehaviorState newBehavior)
  {
    switch (newBehavior)
    {
      case BehaviorState.Idle:
        _throttleUrgency = 0.0f; // stop
        _pathRefreshPeriod = -1.0f; // no refresh
        _idleDuration = Random.Range(IdleMinDuration, IdleMaxDuration);
        break;
      case BehaviorState.Wander:
        _throttleUrgency = 0.5f; // half speed
        _pathRefreshPeriod = -1.0f; // manual refresh
        // Pick a path to a wander target
        {
          Vector2 offset = Random.insideUnitCircle * WanderRange;
          Vector3 wanderTarget = _spawnLocation + Vector3.left * offset.x + Vector3.forward * offset.y;
          RecomputePathTo(wanderTarget);
        }
        break;
      case BehaviorState.ApproachPlayer:
        _throttleUrgency = 1.0f; // full speed
        _pathRefreshPeriod = 2.0f; // refresh path every 2 seconds while persuing player
        // Force on line of sight checks even when player is out of vision cone
        _perceptionComponent.ForceLineOfSightCheck = true;
        // Head to the player
        // If this fails we take care of it in attack update
        RecomputePathTo(GetCurrentPlayerLocation());
        break;
      //case BehaviorState.Cower:
      //  _throttleUrgency = 0.0f; // Stop and sh*t yourself
      //  _pathRefreshPeriod = -1.0f; // manual refresh
      //  _birdAnimator.PlayEmote(AIAnimatorController.EmoteState.Cower);
      //  // Set animation dead flag early so that we don't leave emote state
      //  _birdAnimator.IsDead = true;
      //  // Hide the vision cone
      //  _perceptionComponent.gameObject.SetActive(false);
      //  break;
      case BehaviorState.Attack:
        _throttleUrgency = 0.0f; // Stop and attack in place
        _pathRefreshPeriod = -1.0f; // manual refresh

        _birdAnimator.PlayEmote(BirdAnimatorController.EmoteState.Attack);
        _timeSinceAttack = 0.0f; // We just attacked

        // Play death effects to cover the transition
        if (_attackFX != null)
        {
          Instantiate(_attackFX, transform.position, Quaternion.identity);
        }

        if (AudioManager.Instance != null)
          AudioManager.Instance.PlaySound(gameObject, _attackSound);

        break;
    case BehaviorState.Flee:
      _throttleUrgency = 1.0f; // full speed
      _pathRefreshPeriod = -1.0f; // manual refresh
                                  // Head back to spawn location
                                  // If this fails we take care of it in flee update
      RecomputePathTo(_spawnLocation);
      break;
    case BehaviorState.Dead:
        // Play death effects to cover the transition
        if (_deathFX != null)
        {
          Instantiate(_deathFX, transform.position, Quaternion.identity);
        }

        if (AudioManager.Instance != null)
          AudioManager.Instance.PlaySound(gameObject, _deathSound);

        // Clean ourselves up after a moment
        Destroy(this, 0.1f);
        break;
    }
  }

  void UpdatePathRefreshTimer()
  {
    if (_pathRefreshPeriod >= 0)
    {
      _pathRefreshTimer -= Time.deltaTime;
      // Behavior decides where to recompute path too
    }
  }

  bool RecomputePathTo(Vector3 worldTarget)
  {
    _pathRefreshTimer = _pathRefreshPeriod;
    _pathWaypointIndex = 0;
    _pathfollowingStuckTimer = 0.0f;
    return PathFindManager.Instance.CalculatePathToPoint(transform.position, worldTarget, _lastPath);
  }

  void UpdatePathFollowing()
  {
    if (_pathWaypointIndex < _lastPath.Count)
    {
      // Always throttle at the next waypoint
      Vector3 waypoint = _lastPath[_pathWaypointIndex];
      Vector3 throttleTarget2d = Vector3.ProjectOnPlane(waypoint, Vector3.up);
      Vector3 position2d = Vector3.ProjectOnPlane(this.transform.position, Vector3.up);

      // Advance to the next waypoint 
      if (IsWithingDistanceToTarget2D(throttleTarget2d, WaypointTolerance))
      {
        _pathfollowingStuckTimer = 0.0f;
        _pathWaypointIndex++;
      }
      else
      {
        // If we aren't making progress toward the waypoint, increment the stuck timer
        if (_birdMovement.IsStationary())
        {
          _pathfollowingStuckTimer += Time.deltaTime;
        }
        else
        {
          _pathfollowingStuckTimer = 0.0f;
        }
      }
    }

    // Throttle at next waypoint
    if (_pathWaypointIndex < _lastPath.Count)
    {
      Vector3 waypoint = _lastPath[_pathWaypointIndex];

      SetThrottleTarget(waypoint);
    }
    else
    {
      ClearThrottleTarget();
    }
  }

  void DrawPath()
  {
    if (_lastPath.Count <= 0)
      return;

    Vector3 PrevPathPoint = _lastPath[0];
    for (int pathIndex = 1; pathIndex < _lastPath.Count; ++pathIndex)
    {
      Vector3 NextPathPoint = _lastPath[pathIndex];
      Debug.DrawLine(PrevPathPoint, NextPathPoint, Color.red);
      PrevPathPoint = NextPathPoint;
    }
  }

  bool IsWithingDistanceToTarget2D(Vector3 target, float distance)
  {
    Vector3 target2d = Vector3.ProjectOnPlane(target, Vector3.up);
    Vector3 position2d = Vector3.ProjectOnPlane(this.transform.position, Vector3.up);

    return Vector3.Distance(target2d, position2d) <= distance;
  }

  void SetThrottleTarget(Vector3 target)
  {
    _throttleTarget = target;
    _hasValidThrottleTarget = true;
  }

  void ClearThrottleTarget()
  {
    _hasValidThrottleTarget = false;
  }

  void UpdateMoveVector()
  {
    if (DebugUsePlayerControls)
    {
      float forwardAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveForwardAxis);
      float horizontalAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontalAxis);

      // Calculate move direction
      Vector3 walkDirection = transform.forward.WithY(0).normalized;
      Vector3 strafeDirection = transform.right.WithY(0).normalized;

      _birdMovement.WorldThrottle = Vector3.ClampMagnitude(walkDirection*forwardAxis + strafeDirection*horizontalAxis, 1);

      bool wantJumpAction = _rewiredPlayer.GetButtonDown(RewiredConsts.Action.Jump);
      if (_birdMovement.CanTakeOff() && wantJumpAction)
      {
        _birdMovement.TakeOff();
      }
      else if (_birdMovement.CanLand() && wantJumpAction)
      {
        _birdMovement.Land();
      }
    }
    else
    {
      Vector3 worldThrottleDirection = Vector3.zero;

      if (_hasValidThrottleTarget && _throttleUrgency > 0.0f)
      {
        worldThrottleDirection = _throttleTarget - this.transform.position;
        worldThrottleDirection.y = 0;
        worldThrottleDirection = Vector3.Normalize(worldThrottleDirection);
      }

      _birdMovement.WorldThrottle = worldThrottleDirection;
      _birdMovement.IsSprinting= _throttleUrgency > 0.5f;
    }
  }

  void UpdateAnimationParameters()
  {
    if (_birdMovement.IsMoving())
    {
      _birdAnimator.CurrentLocomotionSpeed = _birdMovement.GetSpeed();
      _birdAnimator.CurrentLocomotionState = BirdAnimatorController.LocomotionState.Move;
    }
    else
    {
      _birdAnimator.CurrentLocomotionSpeed = 0;
      _birdAnimator.CurrentLocomotionState = BirdAnimatorController.LocomotionState.Idle;
    }
  }

  Vector3 GetCurrentPlayerLocation()
  {
    PlayerActorController player= PlayerActorController.Instance;

    return player ? player.transform.position : Vector3.zero;
  }
}
