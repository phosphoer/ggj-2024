using UnityEngine;

public class CookingPotController : MonoBehaviour
{
  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private InventoryController _inventory = null;

  private void Awake()
  {
    _interactable.InteractionTriggered += OnInteract;
  }

  private void OnInteract(InteractionController controller)
  {
  }
}