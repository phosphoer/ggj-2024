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

  [SerializeField]
  private List<Animator> _blingAnimators = new List<Animator>();

  private BirdMovementController.MovementMode _currentMovementMove;
  private float _currentLocomotionSpeed;
  private bool _isChanneling;

  private static readonly int kAnimMoveSpeed = Animator.StringToHash("MoveSpeed");
  private static readonly int kAnimIsFlying = Animator.StringToHash("IsFlying");
  private static readonly int kAnimIsWalking = Animator.StringToHash("IsWalking");
  private static readonly int kAnimIsPerching = Animator.StringToHash("IsPerching");
  private static readonly int kAnimIsChanneling = Animator.StringToHash("IsChanneling");


  public void AddBlingAnimator(Animator animator)
  {
    ApplyStateToAnimator(animator);
    _blingAnimators.Add(animator);
  }

  public void RemoveBlingAnimator(Animator animator)
  {
    _blingAnimators.Remove(animator);
  }

  private void Update()
  {
    ApplyStateToAnimator(_animator);
    foreach (Animator animator in _blingAnimators)
    {
      ApplyStateToAnimator(animator);
    }
  }

  public void ApplyStateToAnimator(Animator animator)
  {
    if (animator != null)
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

      animator.SetBool(kAnimIsFlying, isFlying);
      animator.SetBool(kAnimIsWalking, isWalking);
      animator.SetBool(kAnimIsPerching, isPerching);
      animator.SetBool(kAnimIsChanneling, _isChanneling);
      animator.SetFloat(kAnimMoveSpeed, speed);
    }
  }
}
