using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowBehavior : MonoBehaviour
{
  [SerializeField]
  private BirdActorController _actor = null;

  private Rewired.Player _rewiredPlayer;

  public enum BehaviorType
  {

  }

  private void Awake()
  {
    _rewiredPlayer = Rewired.ReInput.players.GetPlayer(0);
  }

  private void Update()
  {
    float forwardAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveForwardAxis);
    float horizontalAxis = _rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontalAxis);

    _actor.MoveAxis = new Vector2(horizontalAxis, forwardAxis);

    if (!_actor.IsFlying())
    {
      bool takeOff = _rewiredPlayer.GetButtonDown(RewiredConsts.Action.Jump);
      if (takeOff)
      {
        _actor.Jump();
      }
    }
  }

}
