using UnityEngine;

public class PlayerActorController : Singleton<PlayerActorController>
{
  public CameraControllerPlayer CameraPlayer => _cameraPlayer;

  [SerializeField]
  private ActorController _actor = null;

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
  }

  private void Update()
  {
    float forwardAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveForwardAxis);
    float horizontalAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontalAxis);
    _actor.MoveAxis = new Vector2(horizontalAxis, forwardAxis);
  }
}