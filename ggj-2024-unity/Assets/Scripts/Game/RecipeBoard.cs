using UnityEngine;
using System.Collections.Generic;

public class RecipeBoard : MonoBehaviour
{
  public RecipeDefinition Recipe
  {
    get => _recipe;
    set
    {
      _recipe = value;
      EnsureRecipe();
    }
  }

  [SerializeField]
  private Transform _recipeBoardRoot = null;

  [SerializeField]
  private Transform _recipeListRoot = null;

  [SerializeField]
  private RecipeDefinition _initialRecipe = null;

  [SerializeField]
  private float _itemVerticalSpacing = 0.5f;

  [SerializeField]
  private float _itemMaxSize = 0.4f;

  [SerializeField]
  private TMPro.TMP_Text _ingredientCountText = null;

  private RecipeDefinition _recipe;

  private void Start()
  {
    _ingredientCountText.gameObject.SetActive(false);
    Recipe = _initialRecipe;
  }

  private void EnsureRecipe()
  {
    if (_recipe != null)
    {
      for (int i = 0; i < _recipe.Ingredients.Length; ++i)
      {
        var ingredient = _recipe.Ingredients[i];
        ItemController itemIcon = Instantiate(ingredient.Item.Prefab, _recipeListRoot);
        itemIcon.SetCollidersEnabled(false);
        itemIcon.SetInteractionEnabled(false);
        itemIcon.SetPhysicsEnabled(false);

        Bounds itemBounds = itemIcon.gameObject.GetHierarchyBounds();
        Vector3 itemSizeLocal = transform.InverseTransformDirection(itemBounds.size);
        if (itemSizeLocal.x < itemSizeLocal.y && itemSizeLocal.x < itemSizeLocal.z)
        {
          itemIcon.transform.right = transform.forward;
          itemIcon.transform.localScale = itemIcon.transform.localScale.WithX(0.1f);
        }
        else if (itemSizeLocal.y < itemSizeLocal.x && itemSizeLocal.y < itemSizeLocal.z)
        {
          itemIcon.transform.up = transform.forward;
          itemIcon.transform.localScale = itemIcon.transform.localScale.WithY(0.1f);
        }
        else if (itemSizeLocal.z < itemSizeLocal.x && itemSizeLocal.z < itemSizeLocal.y)
        {
          itemIcon.transform.forward = transform.forward;
          itemIcon.transform.localScale = itemIcon.transform.localScale.WithZ(0.1f);
        }

        itemBounds = itemIcon.gameObject.GetHierarchyBounds();
        float maxSize = Mathf.Max(itemBounds.size.x, itemBounds.size.y, itemBounds.size.z);
        float scaleFactor = (_itemMaxSize / maxSize) * 0.75f;
        itemIcon.transform.localScale *= scaleFactor;

        itemBounds = itemIcon.gameObject.GetHierarchyBounds();
        Vector3 desiredPos = Vector3.down * i * _itemVerticalSpacing;
        Vector3 iconBoundsOffset = (itemIcon.transform.position - itemBounds.center);
        itemIcon.transform.localPosition = desiredPos + iconBoundsOffset;
        itemIcon.transform.localPosition = itemIcon.transform.localPosition.WithZ(0);

        TMPro.TMP_Text countText = Instantiate(_ingredientCountText, _recipeListRoot);
        countText.text = $"x{ingredient.Count}";
        countText.transform.localPosition = desiredPos - Vector3.right * _itemMaxSize;
        countText.gameObject.SetActive(true);
      }

      _recipeBoardRoot.localScale = _recipeBoardRoot.localScale.WithY(_itemMaxSize * _recipe.Ingredients.Length - _recipeListRoot.localPosition.y);
    }
  }
}