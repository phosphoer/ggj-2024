using UnityEngine;

public class GameController : MonoBehaviour
{
  private void Start()
  {
    MainCamera.Instance.CameraStack.PushController(PlayerActorController.Instance.CameraPlayer);
  }
}