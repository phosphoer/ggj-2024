using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerchController : MonoBehaviour
{
  public static IReadOnlyList<PerchController> Instances => _instances;
  private static List<PerchController> _instances = new();

  public bool IsWalkablePerch= false;
  public bool IsPublicPerch= false;

  private CrowBehaviorManager _owningBird = null;

  private void Awake()
  {
    _instances.Add(this);
  }

  private void OnDestroy()
  {
    _instances.Remove(this);
  }

  public bool IsPerchReserved()
  {
    return _owningBird != null;
  }

  public CrowBehaviorManager GetOwningBird()
  {
    return _owningBird;
  }

  public CrowBehaviorManager GetPerchedBird()
  {
    if (_owningBird != null)
    {
      // Only return the owning bird if it has actually attached itself to the perch
      return _owningBird.transform.parent == this.transform ? _owningBird : null;
    }

    return null;
  }

  public bool ReservePerch(CrowBehaviorManager bird)
  {
    if (!IsPerchReserved())
    {
      _owningBird= bird;
      return true;
    }

    return false;
  }

  public bool LeavePerch(CrowBehaviorManager bird)
  {
    if (_owningBird == bird)
    {
      _owningBird= null;
      return true;
    }

    return false;
  }
}
