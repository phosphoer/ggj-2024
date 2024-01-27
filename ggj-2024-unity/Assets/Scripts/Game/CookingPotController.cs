using UnityEngine;

public class CookingPotController : MonoBehaviour
{
  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private InventoryController _inventory = null;

  [SerializeField]
  private ParticleSystem _fxCookingBubbles = null;

  [SerializeField]
  private ParticleSystem _fxCookingFire = null;

  [SerializeField]
  private Transform[] _cookingSlots = null;

  [SerializeField]
  private RecipeDefinition[] _recipes = null;

  [SerializeField]
  private RecipeDefinition _invalidRecipe = null;

  private bool _isCooking = false;
  private float _cookingTimer = 0;
  private RecipeDefinition _activeRecipe = null;

  private void Awake()
  {
    _interactable.InteractionTriggered += OnInteract;
    _inventory.ItemAdded += OnItemAdded;
    _inventory.ItemRemoved += OnItemRemoved;
    _interactable.enabled = _inventory.Items.Count > 0;
    _fxCookingBubbles.Stop();
    _fxCookingFire.Stop();
  }

  private void Update()
  {
    for (int i = 0; i < _cookingSlots.Length; ++i)
    {
      Transform cookingSlot = _cookingSlots[i];
      foreach (Transform child in cookingSlot)
      {
        child.localPosition = Vector3.up * Mathf.Sin(Time.time + i) * 0.1f;
      }
    }

    if (_isCooking)
    {
      _cookingTimer += Time.deltaTime;
      if (_cookingTimer >= _activeRecipe.CookDuration)
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
    int currentSlotIndex = _cookingSlots.WrapIndex(_inventory.Items.Count);
    Transform currentSlot = _cookingSlots[currentSlotIndex];
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
    if (!_isCooking)
    {
      _activeRecipe = GetRecipeForIngredients();
      StartCooking();
    }
  }

  private void StartCooking()
  {
    _isCooking = true;
    _interactable.enabled = false;
    _fxCookingBubbles.Play();
    _fxCookingFire.Play();
    _cookingTimer = 0;
  }

  private void StopCooking()
  {
    _isCooking = false;
    _fxCookingBubbles.Stop();
    _fxCookingFire.Stop();

    foreach (var cookSlot in _cookingSlots)
    {
      foreach (Transform child in cookSlot)
      {
        var dehydrate = child.gameObject.AddComponent<UIHydrate>();
        dehydrate.DestroyOnDehydrate = true;
        dehydrate.Dehydrate();
      }
    }

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