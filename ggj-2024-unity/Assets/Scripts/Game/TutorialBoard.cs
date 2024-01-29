using System;
using UnityEngine;

public class TutorialBoard : MonoBehaviour
{
  [SerializeField]
  private Interactable _interactable = null;

  private void Awake()
  {
    _interactable.InteractionTriggered += OnInteract;
  }

  private void OnInteract(InteractionController controller)
  {
    var uiHydrate = gameObject.AddComponent<UIHydrate>();
    uiHydrate.DestroyOnDehydrate = true;
    uiHydrate.Dehydrate();
  }
}