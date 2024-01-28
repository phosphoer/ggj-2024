using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

  private IEnumerator Start()
  {
    _ingredientCountText.gameObject.SetActive(false);

    UIHydrate uiHydrate = GetComponent<UIHydrate>();
    if (uiHydrate != null)
    {
      while (uiHydrate.IsAnimating || transform.localScale.x < Mathf.Epsilon)
        yield return null;
    }

    Recipe = _initialRecipe;
  }

  private void EnsureRecipe()
  {
    Quaternion originalRotation = transform.rotation;
    transform.rotation = Quaternion.identity;

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
        itemBounds.size = transform.InverseTransformDirection(itemBounds.size);
        float maxSize = Mathf.Max(itemBounds.size.x, itemBounds.size.y, itemBounds.size.z);
        float scaleFactor = (_itemMaxSize / maxSize) * 0.75f;
        itemIcon.transform.localScale *= scaleFactor;

        itemBounds = itemIcon.gameObject.GetHierarchyBounds();
        itemBounds.size = transform.InverseTransformDirection(itemBounds.size);
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

      // Duplicate the list onto the reverse side 
      Transform boardOtherSide = Instantiate(_recipeListRoot, transform);
      boardOtherSide.localPosition = boardOtherSide.localPosition.WithZ(-boardOtherSide.localPosition.z);
      boardOtherSide.localRotation = Quaternion.Euler(0, 180, 0);
      boardOtherSide.localPosition = boardOtherSide.localPosition.WithX(-boardOtherSide.localPosition.x);

      transform.rotation = originalRotation;
    }
  }
}