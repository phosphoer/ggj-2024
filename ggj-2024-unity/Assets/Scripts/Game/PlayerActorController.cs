using UnityEngine;

public class PlayerActorController : MonoBehaviour
{
  [SerializeField]
  private ActorController _actor = null;

  private Rewired.Player _rewiredPlayer;

  private void Awake()
  {
    _rewiredPlayer = Rewired.ReInput.players.GetPlayer(0);
  }

  private void Update()
  {
    float forwardAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveForwardAxis);
    float horizontalAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontalAxis);
    _actor.MoveAxis = new Vector2(horizontalAxis, forwardAxis);
  }
}