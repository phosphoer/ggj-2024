using UnityEngine;

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
  private RecipeDefinition[] _recipes = null;

  [SerializeField]
  private RecipeDefinition _invalidRecipe = null;

  private bool _isConstructing = false;
  private float _constructionTimer = 0;
  private RecipeDefinition _activeRecipe = null;

  private void Awake()
  {
    _interactable.InteractionTriggered += OnInteract;
    _inventory.ItemAdded += OnItemAdded;
    _inventory.ItemRemoved += OnItemRemoved;
    _interactable.enabled = _inventory.Items.Count > 0;

    foreach (var fx in _fxConstruction)
      fx.Stop();
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
    if (item != null && item.ItemDefinition.IsIngredient)
    {
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

    _interactable.enabled = _inventory.Items.Count > 0;
  }

  private void OnItemRemoved(ItemDefinition definition)
  {
    _interactable.enabled = _inventory.Items.Count > 0;
  }

  private void OnInteract(InteractionController controller)
  {
    if (!_isConstructing)
    {
      _activeRecipe = GetRecipeForIngredients();
      StartCooking();
    }
  }

  private void StartCooking()
  {
    _isConstructing = true;
    _interactable.enabled = false;
    _constructionTimer = 0;

    foreach (var fx in _fxConstruction)
      fx.Play();
  }

  private void StopCooking()
  {
    _isConstructing = false;

    foreach (var ingredientSlot in _ingredientSlots)
    {
      foreach (Transform child in ingredientSlot)
      {
        var dehydrate = child.gameObject.AddComponent<UIHydrate>();
        dehydrate.DestroyOnDehydrate = true;
        dehydrate.Dehydrate();
      }
    }

    foreach (var fx in _fxConstruction)
      fx.Stop();

    _inventory.ClearItems();

    ItemDefinition outputItemDef = _activeRecipe.Result;
    ItemController outputItem = Instantiate(outputItemDef.Prefab);
    outputItem.transform.position = _inventory.ItemSpawnAnchor.position;
    outputItem.Rigidbody.AddForce((Random.insideUnitCircle.OnXZPlane() + Vector3.up) * 3, ForceMode.VelocityChange);

    UIHydrate hydrate = outputItem.gameObject.AddComponent<UIHydrate>();
    hydrate.Hydrate();
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