using UnityEngine;
using System.Collections.Generic;

public class CraftBenchController : MonoBehaviour
{
  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private InventoryController _inventory = null;

  [SerializeField]
  private ParticleSystem[] _fxConstruction = null;

  [SerializeField]
  private Transform[] _ingredientSlots = null;

  [SerializeField]
  private bool _allowInvalidRecipe = false;

  [SerializeField]
  private bool _allowInvalidIngredients = true;

  [SerializeField]
  private RecipeDefinition[] _recipes = null;

  [SerializeField]
  private RecipeDefinition _invalidRecipe = null;

  [SerializeField]
  private TMPro.TMP_Text _recipeCostText = null;

  [SerializeField]
  private string _returnMaterialString = "Return Materials";

  [SerializeField]
  private string _craftItemString = "Craft {0}";

  [SerializeField]
  private SoundBank _sfxIngredientAdded = null;

  [SerializeField]
  private SoundBank _sfxCraftingStarted = null;

  [SerializeField]
  private SoundBank _sfxCraftingComplete = null;

  [SerializeField]
  private SoundBank _sfxCraftingFail = null;

  private bool _isConstructing = false;
  private float _constructionTimer = 0;
  private RecipeDefinition _activeRecipe = null;
  private List<ItemController> _pendingIngredients = new();
  private AudioManager.AudioInstance _audioCookingInstance;

  private void Awake()
  {
    _inventory.ItemAdded += OnItemAdded;
    _inventory.ItemRemoved += OnItemRemoved;
    _interactable.InteractionTriggered += OnInteract;
    _interactable.enabled = _inventory.Items.Count > 0;

    foreach (var fx in _fxConstruction)
      fx.Stop();

    UpdateInteractable();

    if (_recipeCostText != null && _recipes.Length > 0)
    {
      _recipeCostText.text = $"{_recipes[0].Ingredients[0].Count:00}";
    }
  }

  private void Update()
  {
    for (int i = 0; i < _ingredientSlots.Length; ++i)
    {
      Transform cookingSlot = _ingredientSlots[i];
      foreach (Transform child in cookingSlot)
      {
        child.localPosition = Vector3.up * Mathf.Sin(Time.time + i) * 0.1f;
      }
    }

    if (_isConstructing)
    {
      _constructionTimer += Time.deltaTime;
      if (_constructionTimer >= _activeRecipe.CookDuration)
        StopCooking();
    }
  }

  private void OnTriggerEnter(Collider c)
  {
    ItemController item = c.GetComponentInParent<ItemController>();
    if (item != null && item.ItemDefinition.IsIngredient && item.WasThrown)
    {
      if (!_allowInvalidIngredients)
      {
        bool isValid = false;
        foreach (var recipe in _recipes)
          isValid |= recipe.RequiresIngredient(item.ItemDefinition);

        if (!isValid)
          return;
      }

      _inventory.AddItem(item);
    }
  }

  private void OnItemAdded(ItemDefinition definition)
  {
    int currentSlotIndex = _ingredientSlots.WrapIndex(_inventory.Items.Count);
    Transform currentSlot = _ingredientSlots[currentSlotIndex];
    ItemController item = Instantiate(definition.Prefab, currentSlot);
    item.transform.SetIdentityTransformLocal();
    item.transform.localRotation = Random.rotation;
    item.SetPhysicsEnabled(false);
    item.SetCollidersEnabled(false);
    item.SetInteractionEnabled(false);

    _pendingIngredients.Add(item);

    if (_sfxIngredientAdded != null)
      AudioManager.Instance.PlaySound(gameObject, _sfxIngredientAdded);

    UpdateInteractable();
  }

  private void OnItemRemoved(ItemDefinition definition)
  {
    Debug.Log($"Item removed: {definition.Name}");
    for (int i = 0; i < _pendingIngredients.Count; ++i)
    {
      ItemController pendingIngredient = _pendingIngredients[i];
      if (pendingIngredient.ItemDefinition == definition)
      {
        Debug.Log($"Removing pending ingredient {pendingIngredient.ItemDefinition.Name}");
        var dehydrate = pendingIngredient.gameObject.AddComponent<UIHydrate>();
        dehydrate.DestroyOnDehydrate = true;
        dehydrate.Dehydrate();
        _pendingIngredients.RemoveAt(i);
        break;
      }
    }

    UpdateInteractable();
  }

  private void OnInteract(InteractionController controller)
  {
    if (!_isConstructing)
    {
      _activeRecipe = GetRecipeForIngredients();
      if (_activeRecipe == _invalidRecipe && !_allowInvalidRecipe)
      {
        while (_inventory.Items.Count > 0)
          _inventory.TossItem(_inventory.Items[0], Random.insideUnitSphere.WithY(1f) * 3, markAsThrown: false);
      }
      else
      {
        StartCooking();
      }
    }
  }

  private void UpdateInteractable()
  {
    _interactable.enabled = _inventory.Items.Count > 0;
    if (!_allowInvalidRecipe)
    {
      RecipeDefinition pendingRecipe = GetRecipeForIngredients();
      if (pendingRecipe == _invalidRecipe)
      {
        _interactable.SetInteractionText(_returnMaterialString);
      }
      else
      {
        _interactable.SetInteractionText(string.Format(_craftItemString, pendingRecipe.Result.Name));
      }
    }
  }

  private void StartCooking()
  {
    _isConstructing = true;
    _interactable.enabled = false;
    _constructionTimer = 0;

    if (_sfxCraftingStarted != null)
      AudioManager.Instance.PlaySound(gameObject, _sfxCraftingStarted);

    foreach (var fx in _fxConstruction)
      fx.Play();
  }

  private void StopCooking()
  {
    _isConstructing = false;

    foreach (var fx in _fxConstruction)
      fx.Stop();

    _inventory.ClearItems();

    ItemDefinition outputItemDef = _activeRecipe.Result;
    _inventory.AddItem(outputItemDef);
    _inventory.TossItem(outputItemDef, (Random.insideUnitCircle.OnXZPlane() + Vector3.up) * 3, markAsThrown: false);

    if (_sfxCraftingStarted != null)
      AudioManager.Instance.StopSound(gameObject, _sfxCraftingStarted);

    if (outputItemDef == _invalidRecipe)
    {
      if (_sfxCraftingFail != null)
        AudioManager.Instance.PlaySound(gameObject, _sfxCraftingFail);
    }
    else
    {
      if (_sfxCraftingComplete != null)
        AudioManager.Instance.PlaySound(gameObject, _sfxCraftingComplete);
    }
  }

  private RecipeDefinition GetRecipeForIngredients()
  {
    foreach (var recipe in _recipes)
    {
      if (_inventory.MatchesRecipe(recipe))
        return recipe;
    }

    return _invalidRecipe;
  }
}