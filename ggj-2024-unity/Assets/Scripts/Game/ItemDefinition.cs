using UnityEngine;

[CreateAssetMenu(fileName = "new-item", menuName = "Game/Item Definition")]
public class ItemDefinition : ScriptableObject
{
  public string Name = "An Item";
  public ItemController Prefab = null;
  public bool IsIngredient = true;
}