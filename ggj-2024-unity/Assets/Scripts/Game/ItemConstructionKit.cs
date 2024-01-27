using UnityEngine;

public class ItemConstructionKit : MonoBehaviour
{
  [SerializeField]
  private ItemController _itemController = null;

  [SerializeField]
  private GameObject _constructPrefab = null;

  [SerializeField]
  private ParticleSystem _fxConstructPoofPrefab = null;

  [SerializeField]
  private LayerMask _constructTerrainMask = default;

  private float _restTimer;

  private const float kConstructionRestThreshold = 0.5f;

  private void Update()
  {
    if (_itemController.WasThrown)
    {
      if (_itemController.Rigidbody.velocity.sqrMagnitude <= Mathf.Epsilon)
      {
        _restTimer += Time.deltaTime;
      }
      else
      {
        _itemController.SetInteractionEnabled(false);
        _restTimer = 0;
      }

      if (_restTimer > kConstructionRestThreshold)
      {
        _restTimer = 0;
        ConstructItem();
      }
    }
  }

  private void ConstructItem()
  {
    enabled = false;

    RaycastHit hitInfo;
    if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hitInfo, 10, _constructTerrainMask))
    {
      _itemController.SetPhysicsEnabled(false);
      _itemController.SetCollidersEnabled(false);
      _itemController.SetInteractionEnabled(false);

      GameObject constructedItem = Instantiate(_constructPrefab, transform.parent);
      constructedItem.transform.position = hitInfo.point;
      constructedItem.transform.up = hitInfo.normal;

      ParticleSystem fxConstruct = Instantiate(_fxConstructPoofPrefab);
      fxConstruct.transform.SetPositionAndRotation(constructedItem.transform.position, constructedItem.transform.rotation);
      var fxSettings = fxConstruct.main;
      fxSettings.stopAction = ParticleSystemStopAction.Destroy;
      fxConstruct.Play();

      var hydrate = constructedItem.gameObject.AddComponent<UIHydrate>();
      hydrate.Hydrate();

      var dehydrate = gameObject.AddComponent<UIHydrate>();
      dehydrate.DestroyOnDehydrate = true;
      dehydrate.Dehydrate();
    }
    else
    {
      _itemController.SetInteractionEnabled(true);
    }
  }
}