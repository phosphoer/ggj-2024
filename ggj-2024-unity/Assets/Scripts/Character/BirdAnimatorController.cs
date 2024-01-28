using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdAnimatorController : MonoBehaviour
{
  public BirdMovementController.MovementMode CurrentMovementMode
  {
    get { return _currentMovementMove; }
    set { _currentMovementMove = value; }
  }

  public float CurrentLocomotionSpeed
  {
    get { return _currentLocomotionSpeed; }
    set { _currentLocomotionSpeed = value; }
  }

  public bool IsChanneling
  {
    get { return _isChanneling; }
    set { _isChanneling = value; }
  }

  [SerializeField]
  private Animator _animator = null;

  private BirdMovementController.MovementMode _currentMovementMove;
  private float _currentLocomotionSpeed;
  private bool _isChanneling;

  private static readonly int kAnimMoveSpeed = Animator.StringToHash("MoveSpeed");
  private static readonly int kAnimIsFlying = Animator.StringToHash("IsFlying");
  private static readonly int kAnimIsWalking = Animator.StringToHash("IsWalking");
  private static readonly int kAnimIsPerching = Animator.StringToHash("IsPerching");
  private static readonly int kAnimIsChanneling = Animator.StringToHash("IsChanneling");

  private void Update()
  {
    if (_animator != null)
    {
      bool isFlying= false;
      bool isWalking= false;
      bool isPerching= false;
      float speed= 0.0f;

      switch(_currentMovementMove)
      {
      case BirdMovementController.MovementMode.Walking:
      case BirdMovementController.MovementMode.Landing:
        isWalking= true;
        speed= _currentLocomotionSpeed;
        break;
      case BirdMovementController.MovementMode.TakeOffWindup:
      case BirdMovementController.MovementMode.TakeOff:
      case BirdMovementController.MovementMode.Flying:
      case BirdMovementController.MovementMode.Falling:
        isFlying= true;
        break;
      case BirdMovementController.MovementMode.Perching:
      case BirdMovementController.MovementMode.Perched:
        isPerching= true;
        break;
      }

      _animator.SetBool(kAnimIsFlying, isFlying);
      _animator.SetBool(kAnimIsWalking, isWalking);
      _animator.SetBool(kAnimIsPerching, isPerching);
      _animator.SetBool(kAnimIsChanneling, _isChanneling);
      _animator.SetFloat(kAnimMoveSpeed, speed);
    }
  }
}
