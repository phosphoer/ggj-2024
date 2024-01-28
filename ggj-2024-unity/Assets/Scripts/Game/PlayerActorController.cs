using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerActorController : Singleton<PlayerActorController>
{
  public CameraControllerPlayer CameraPlayer => _cameraPlayer;
  public InventoryController Inventory => _inventory;
  public Transform AIVisibilityTarget => _aiVisibilityTarget != null ? _aiVisibilityTarget : transform;

  [SerializeField]
  private Transform _aiVisibilityTarget = null;

  [SerializeField]
  private ActorController _actor = null;

  [SerializeField]
  private Animator _animator = null;

  [SerializeField]
  private InteractionController _interactionController = null;

  [SerializeField]
  private InventoryController _inventory = null;

  [SerializeField]
  private InventorySelector _inventorySelector = null;

  [SerializeField]
  private CameraControllerPlayer _cameraPlayerPrefab = null;

  [SerializeField]
  private PerchController[] _staffPerches = null;

  private Rewired.Player _rewiredPlayer;
  private CameraControllerPlayer _cameraPlayer;
  private CrowBehaviorManager _commandingCrow= null;
  private CrowTarget _currentCrowTarget= null;

  private static readonly int kAnimMoveSpeed = Animator.StringToHash("MoveSpeed");
  private static readonly int kAnimIsPickingUp = Animator.StringToHash("IsPickingUp");
  private static readonly int kAnimIsCalling = Animator.StringToHash("IsCalling");
  private static readonly int kAnimIsAttacking = Animator.StringToHash("IsAttacking");

  private static readonly float kSelectCosAngleThreshold= Mathf.Cos(30.0f);

  private void Awake()
  {
    Instance = this;
    _rewiredPlayer = Rewired.ReInput.players.GetPlayer(0);
    _cameraPlayer = Instantiate(_cameraPlayerPrefab);
    _cameraPlayer.TargetTransform = transform;
    _inventory.ItemAdded += OnItemAdded;
    _inventory.ItemRemoved += OnItemRemoved;
  }

  private void Update()
  {
    // Reset some animator state 
    _animator.SetBool(kAnimIsCalling, false);
    _animator.SetBool(kAnimIsAttacking, false);
    _animator.SetBool(kAnimIsPickingUp, false);

    // Move
    float forwardAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveForwardAxis);
    float horizontalAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontalAxis);
    _actor.MoveAxis = new Vector2(horizontalAxis, forwardAxis);

    float moveSpeed = Mathf.Clamp01(_actor.Motor.Velocity.magnitude);
    _animator.SetFloat(kAnimMoveSpeed, moveSpeed);

    // If there's something we can interact with 
    if (_interactionController.ClosestInteractable != null)
    {
      if (_rewiredPlayer.GetButtonDown(RewiredConsts.Action.Interact))
      {
        _animator.SetBool(kAnimIsPickingUp, true);
        _interactionController.TriggerInteraction();
      }
    }

    // Toss items 
    if (_rewiredPlayer.GetButtonDown(RewiredConsts.Action.Toss))
    {
      if (_inventorySelector.IsVisible && _inventorySelector.SelectedItem != null)
      {
        _animator.SetBool(kAnimIsPickingUp, true);
        _inventory.TossItem(_inventorySelector.SelectedItem, (transform.forward + Vector3.up) * 4);
        _inventorySelector.Hide();
      }
      else if (!_inventorySelector.IsVisible)
      {
        _inventorySelector.Show();
      }
    }

    // Select item to toss
    if (_rewiredPlayer.GetNegativeButtonDown(RewiredConsts.Action.SelectItem))
    {
      _inventorySelector.SelectPrevious();
    }
    if (_rewiredPlayer.GetButtonDown(RewiredConsts.Action.SelectItem))
    {
      _inventorySelector.SelectNext();
    }

    // Caw ?
    if (_rewiredPlayer.GetButtonUp(RewiredConsts.Action.Caw) && _commandingCrow == null)
    {
      // Ask for a crow to come to us
      SummonClosestCrow();

      _animator.SetBool(kAnimIsCalling, true);
    }

    // While we have a crow to command, look for the best target for them
    if (_commandingCrow != null)
    {
      // Find the best crow target 
      CrowTarget bestCrowTarget= FindBestCrowTarget();

      // Update highlight on target change
      if (bestCrowTarget != _currentCrowTarget)
      {
        if (_currentCrowTarget != null)
        {
          _currentCrowTarget.UnselectHighlight();
        }

        if (bestCrowTarget != null)
        {
          bestCrowTarget.SelectHighlight();
        }

        _currentCrowTarget= bestCrowTarget;
      }
    }

    // Prep crow command
    if (_rewiredPlayer.GetButtonTimedPress(RewiredConsts.Action.Caw, 0.5f))
    {
      _commandingCrow= GetCommandableCrow();

      if (_commandingCrow != null)
      {
        foreach (var crowTarget in CrowTarget.Instances)
        {
          crowTarget.ShowTargetHighlight();
        }
      }
    }
    // Execute crow command
    else if (_commandingCrow != null && _currentCrowTarget != null)
    {
      _commandingCrow.FetchCrowTarget(_currentCrowTarget);
      _commandingCrow= null;
      _currentCrowTarget= null;

      foreach (var crowTarget in CrowTarget.Instances)
      {
        crowTarget.HideTargetHighlight();
      };
    }

    // Attack
    if (_rewiredPlayer.GetButtonDown(RewiredConsts.Action.Attack))
    {
      _animator.SetBool(kAnimIsAttacking, true);
    }

    // Camera controls
    float cameraHorizontalAxis = Mathf.Clamp(_rewiredPlayer.GetAxis(RewiredConsts.Action.CameraHorizontalAxis), -1f, 1f);
    float cameraVerticalAxis = Mathf.Clamp(_rewiredPlayer.GetAxis(RewiredConsts.Action.CameraVerticalAxis), -1f, 1f);
    _cameraPlayer.AxisX += cameraHorizontalAxis * Time.deltaTime * 100;
    _cameraPlayer.AxisY += cameraVerticalAxis * Time.deltaTime * 100;
  }

  public CrowTarget FindBestCrowTarget()
  {
    Vector3 rayOrigin= _cameraPlayer.MountPoint.position;
    Vector3 rayForward= _cameraPlayer.MountPoint.forward;

    // Find the best crow target 
    CrowTarget bestCrowTarget= null;
    float bestCrowTargetScore= -1.0f;
    foreach (var crowTarget in CrowTarget.Instances)
    {
      if (crowTarget.Perch && !crowTarget.Perch.IsPerchReserved())
      {
        Vector3 targetPosition= crowTarget.transform.position;
        Vector3 cameraToTarget= Vector3.Normalize(targetPosition - rayOrigin);

        float cosAngle= Vector3.Dot(cameraToTarget, rayForward);
        if (cosAngle >= kSelectCosAngleThreshold)
        {
          if (bestCrowTarget == null || cosAngle > bestCrowTargetScore)
          {
            bestCrowTarget= crowTarget;
            bestCrowTargetScore= cosAngle;
          }
        }
      }
    }

    return bestCrowTarget;
  }

  public bool SummonClosestCrow()
  {
    if (StaffPerchAvailable())
    {
      CrowBehaviorManager bestCrow = null;
      float bestCrowDistance = 0.0f;
      foreach (CrowBehaviorManager crow in CrowBehaviorManager.Instances)
      {
        float crowDistance = 0.0f;
        if (crow.CanSummonCrow(out crowDistance))
        {
          if (bestCrow == null || crowDistance < bestCrowDistance)
          {
            bestCrow = crow;
            bestCrowDistance = crowDistance;
          }
        }
      }

      if (bestCrow != null)
      {
        return bestCrow.SummonCrow();
      }
    }

    return false;
  }

  public CrowBehaviorManager GetCommandableCrow()
  {
    foreach (PerchController perch in _staffPerches)
    {
      CrowBehaviorManager perchedBird= perch.GetPerchedBird();
      if (perchedBird != null)
      {
        return perchedBird;
      }
    }

    return null;
  }

  public bool HasSummonedCrows()
  {
    foreach (PerchController perch in _staffPerches)
    {
      if (perch.IsPerchReserved())
      {
        return true;
      }
    }

    return false;
  }

  public bool StaffPerchAvailable()
  {
    foreach (PerchController perch in _staffPerches)
    {
      if (!perch.IsPerchReserved())
      {
        return true;
      }
    }

    return false;
  }

  public Transform ReserveStaffPerch(CrowBehaviorManager bird)
  {
    foreach (PerchController perch in _staffPerches)
    {
      if (perch.ReservePerch(bird))
      {
        return perch.transform;
      }
    }

    return null;
  }

  public void LeaveStaffPerch(CrowBehaviorManager bird)
  {
    foreach (PerchController perch in _staffPerches)
    {
      perch.LeavePerch(bird);
    }
  }

  private void OnItemAdded(ItemDefinition definition)
  {
    Debug.Log($"Item added to player inventory: {definition.name}");
  }

  private void OnItemRemoved(ItemDefinition definition)
  {
    Debug.Log($"Item removed from player inventory: {definition.name}");
  }
}