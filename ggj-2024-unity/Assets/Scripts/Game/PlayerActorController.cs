using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerActorController : Singleton<PlayerActorController>
{
  public CameraControllerPlayer CameraPlayer => _cameraPlayer;
  public InventoryController Inventory => _inventory;
  public Transform AIVisibilityTarget => _aiVisibilityTarget;

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
      if (_inventorySelector.IsVisible && _inventorySelector.SelectedItem != null)
      {
        _inventory.TossItem(_inventorySelector.SelectedItem, (transform.forward + Vector3.up) * 5);
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