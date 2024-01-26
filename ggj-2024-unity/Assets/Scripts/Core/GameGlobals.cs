using UnityEngine;

[CreateAssetMenu(fileName = "GameGlobals", menuName = "Game/Game Globals")]
public class GameGlobals : ScriptableObject
{
  public static GameGlobals Instance { get; private set; }

  public AnimationCurve UIHydrateCurve;
  public AnimationCurve UIDehydrateCurve;

  private void OnEnable()
  {
    Instance = this;
  }
}