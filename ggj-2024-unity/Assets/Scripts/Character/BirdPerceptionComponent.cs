using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdPerceptionComponent : MonoBehaviour
{
  public float VisionDistance = 10;
  public float RefreshInterval = 0.1f;
  public bool DrawDebug = true;

  private float _refreshTimer = 0.0f;

  private ItemController _nearbyFood = null;
  public ItemController NearbyFood
  {
    get { return _nearbyFood; }
  }
  public bool SeesNearbyFood
  {
    get { return _nearbyFood != null; }
  }

  private PerchController _nearbyPublicPerch = null;
  public PerchController NearbyPublicPerch => _nearbyPublicPerch;
  public bool SeesNearbyPublicPerch
  {
    get { return _nearbyPublicPerch != null; }
  }

  void Start()
  {
    _refreshTimer = Random.Range(0, RefreshInterval); // Randomly offset that that minimize AI spawned the same frame updating at the same time
  }

  void Update()
  {
    _refreshTimer -= Time.deltaTime;
    if (_refreshTimer <= 0)
    {
      _refreshTimer = RefreshInterval;
      RefreshNearbyFoodInformation();
      RefreshNearbyPerchInformation();
      RedrawVisionRadius();
    }
  }

  void RefreshNearbyFoodInformation()
  {
    _nearbyFood= null;

    float closestDistance= 0.0f;
    foreach (ItemController item in ItemController.Instances)
    {
      if (item.ItemDefinition.IsCrowFood && !item.IsBeingCollected)
      {
        float foodDistance = Vector3.Distance(transform.position, item.transform.position);

        if (foodDistance > VisionDistance)
          continue;

        if (_nearbyFood == null || foodDistance < closestDistance)
        {
          closestDistance= foodDistance;
          _nearbyFood= item;
        }
      }
    }
  }

  void RefreshNearbyPerchInformation()
  {
    _nearbyPublicPerch= null;

    float closestDistance= 0.0f;
    foreach (PerchController perch in PerchController.Instances)
    {
      if (perch.IsPublicPerch && !perch.IsPerchReserved())
      {
        float perchDistance = Vector3.Distance(transform.position, perch.transform.position);

        if (perchDistance > VisionDistance)
          continue;

        if (_nearbyPublicPerch == null || perchDistance < closestDistance)
        {
          closestDistance= perchDistance;
          _nearbyPublicPerch= perch;
        }
      }
    }
  }

  void RedrawVisionRadius()
  {
    if (DrawDebug)
    {
      Vector3 origin = transform.position;
      Vector3 forward = transform.forward;
      Vector3 up = transform.up;
      Vector3 right = transform.right;
      int subdiv = 20;

      Vector3 prevFrontPoint = origin + right * VisionDistance;
      for (int j = 0; j < 2; j++)
      {
        float radius = VisionDistance + (float)j * 0.25f;

        for (int i = 1; i <= subdiv; ++i)
        {
          float radians = Mathf.Deg2Rad * ((float)i * 360.0f / (float)subdiv);
          Vector3 nextFrontPoint = origin + right * radius * Mathf.Cos(radians) + forward * radius * Mathf.Sin(radians);

          Debug.DrawLine(
            prevFrontPoint, nextFrontPoint,
            SeesNearbyFood || SeesNearbyPublicPerch ? Color.green : Color.gray,
            _refreshTimer);
          prevFrontPoint = nextFrontPoint;
        }
      }
    }
  }
}
