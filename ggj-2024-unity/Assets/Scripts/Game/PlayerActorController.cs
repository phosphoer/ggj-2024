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

  private void Awake()
  {
    Instance = this;
    _rewiredPlayer = Rewired.ReInput.players.GetPlayer(0);
    _cameraPlayer = Instantiate(_cameraPlayerPrefab);
    _cameraPlayer.TargetTransform = transform;
    _inventory.ItemAdded += OnItemAdded;
    _inventory.ItemRemoved += OnItemRemoved;

    // TODO: gather the perches from the staff specifically?
    _staffPerches= this.GetComponentsInChildren<PerchController>();
  }

  private void Update()
  {
    // Move
    float forwardAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveForwardAxis);
    float horizontalAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontalAxis);
    _actor.MoveAxis = new Vector2(horizontalAxis, forwardAxis);

    // If there's something we can interact with 
    if (_interactionController.ClosestInteractable != null)
    {
      if (_rewiredPlayer.GetButtonDown(RewiredConsts.Action.Interact))
      {
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
    }

    // Attack
    if (_rewiredPlayer.GetButtonDown(RewiredConsts.Action.Attack))
    {
    }

    // Camera controls
    float cameraHorizontalAxis = Mathf.Clamp(_rewiredPlayer.GetAxis(RewiredConsts.Action.CameraHorizontalAxis), -1, 1);
    float cameraVerticalAxis = Mathf.Clamp(_rewiredPlayer.GetAxis(RewiredConsts.Action.CameraVerticalAxis), -1, 1);
    _cameraPlayer.AxisX = cameraHorizontalAxis;
    _cameraPlayer.AxisY = cameraVerticalAxis;
  }

  public Transform ReserveStaffPerch(CrowBehavior bird)
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

  public void LeaveStaffPerch(CrowBehavior bird)
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