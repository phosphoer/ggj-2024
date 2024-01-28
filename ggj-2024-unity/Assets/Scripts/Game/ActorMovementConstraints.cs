using UnityEngine;
using System.Collections.Generic;

public class ActorMovementConstraints : MonoBehaviour
{
  [SerializeField]
  private int _wallCount = 8;

  [SerializeField]
  private int _wallRaycastCount = 5;

  [SerializeField]
  private RangedFloat _wallRadiusRange = new RangedFloat(1, 5);

  [SerializeField]
  private float _wallHeight = 1;

  [SerializeField]
  private float _wallWidth = 1;

  [SerializeField]
  private LayerMask _raycastMask = default;

  [SerializeField]
  private LayerMask _invalidTerrain = default;

  private List<Transform> _walls = new();
  private Transform _wallRoot;

  private void Awake()
  {
    _wallRoot = new GameObject("ActorMovementConstraints").transform;

    for (int i = 0; i < _wallCount; ++i)
    {
      Transform wall = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
      wall.parent = _wallRoot;
      wall.localScale = new Vector3(_wallWidth, _wallHeight, 0.1f);
      wall.gameObject.layer = gameObject.layer;
      wall.GetComponent<Renderer>().enabled = false;
      wall.gameObject.AddComponent<Rigidbody>().isKinematic = true;
      _walls.Add(wall);
    }
  }

  private void LateUpdate()
  {
    for (int i = 0; i < _walls.Count; ++i)
    {
      Transform wall = _walls[i];
      float angle = (360f / _wallCount) * i;
      Vector3 wallDirection = Quaternion.Euler(0, angle, 0) * transform.forward;

      float furthestSafeDistance = _wallRadiusRange.MinValue;
      for (int raycastIndex = 0; raycastIndex < _wallRaycastCount; ++raycastIndex)
      {
        float raycastDistanceT = raycastIndex / Mathf.Max(1f, _wallRaycastCount - 1f);
        float raycastDistance = _wallRadiusRange.Lerp(raycastDistanceT);
        Vector3 testPos = transform.position + wallDirection * raycastDistance + Vector3.up;
        Debug.DrawLine(testPos, testPos + Vector3.down * _wallHeight);

        RaycastHit hitInfo;
        bool hitSomething = Physics.Raycast(testPos, Vector3.down, out hitInfo, _wallHeight, _raycastMask, QueryTriggerInteraction.Ignore);
        if (hitSomething && _invalidTerrain.ContainsLayer(hitInfo.collider.gameObject.layer))
        {
          Debug.DrawLine(testPos, hitInfo.point, Color.red);
          break;
        }
        else
          furthestSafeDistance = raycastDistance;
      }

      wall.position = transform.position + wallDirection * furthestSafeDistance;
      wall.LookAt(transform.position);
    }
  }
}