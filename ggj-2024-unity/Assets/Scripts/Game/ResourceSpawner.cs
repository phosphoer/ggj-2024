using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
  [SerializeField]
  private ResourceDefinition _resourceDefinition = null;

  [SerializeField]
  private RangedFloat _respawnInterval = new RangedFloat(15, 30);

  [SerializeField]
  private bool _initiallySpawned= false;

  private ResourceController _spawnedResource = null;
  private float _respawnTimer = 0;

  private void DisownSpawnedResource()
  {
    if (_spawnedResource != null)
    {
      _spawnedResource.ParentSpawner= null;
    }

    _spawnedResource= null;
  }

  private void Start()
  {
    if (_initiallySpawned)
    {
      SpawnResource();
    }

    _respawnTimer = _respawnInterval.RandomValue;
  }

  private void Update()
  {
    //if (GameController.Instance.CurrentStage == _resourceDefinition.SpawnPhase)
    {
      if (_spawnedResource == null)
      {
        _respawnTimer -= Time.deltaTime;
        if (_respawnTimer <= 0)
        {
          _respawnTimer = _respawnInterval.RandomValue;

          SpawnResource();
        }
      }
    }
  }

  private void SpawnResource()
  {
      Vector3 spawnPos = transform.position;
      _spawnedResource = Instantiate(_resourceDefinition.Prefab, spawnPos, transform.rotation);
      _spawnedResource.ParentSpawner= this;
  }
}
