using UnityEngine;

[System.Serializable]
public class RecipeIngredient
{
  public ItemDefinition Item;
  public int Count;
}

[CreateAssetMenu(fileName = "new-recipe", menuName = "Game/Recipe Definition")]
public class RecipeDefinition : ScriptableObject
{
  public ItemDefinition Result;
  public float CookDuration = 5;
  public RecipeIngredient[] Ingredients;

  public int GetTotalIngredientCount()
  {
    int total = 0;
    foreach (var ingredient in Ingredients)
    {
      total += ingredient.Count;
    }

    return total;
  }
}