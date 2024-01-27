using UnityEngine;

public class GameController : Singleton<GameController>
{
  public enum GameStage
  {
    Daytime,
    Nighttime
  }

  public GameStage CurrentStage => _gameStage;
  private GameStage _gameStage = GameStage.Daytime;

  private void Start()
  {
    MainCamera.Instance.CameraStack.PushController(PlayerActorController.Instance.CameraPlayer);
  }
}