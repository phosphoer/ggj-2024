using UnityEngine;

[CreateAssetMenu(fileName = "new-resource", menuName = "Game/Resource Definition")]
public class ResourceDefinition : ScriptableObject
{
  public string Name = "An Item";
  public GameController.GameStage SpawnPhase = GameController.GameStage.Daytime;
  public float MinSpawnFrequency = 10.0f;
  public float MaxSpawnFrequency = 60.0f;
  public ResourceController Prefab = null;
}