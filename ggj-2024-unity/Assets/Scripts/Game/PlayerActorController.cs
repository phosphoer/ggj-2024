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

  private Rewired.Player _rewiredPlayer;
  private CameraControllerPlayer _cameraPlayer;

  [SerializeField]
  private PerchController[] _staffPerches = new PerchController[] { };

  private static readonly int kAnimMoveSpeed = Animator.StringToHash("MoveSpeed");
  private static readonly int kAnimIsPickingUp = Animator.StringToHash("IsPickingUp");
  private static readonly int kAnimIsCalling = Animator.StringToHash("IsCalling");
  private static readonly int kAnimIsAttacking = Animator.StringToHash("IsAttacking");

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
      Debug.Log($"Toss pressed, inventory visible = {_inventorySelector.IsVisible}");
      if (_inventorySelector.IsVisible && _inventorySelector.SelectedItem != null)
      {
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
    if (_rewiredPlayer.GetButtonDown(RewiredConsts.Action.Caw))
    {
      _animator.SetBool(kAnimIsCalling, true);
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