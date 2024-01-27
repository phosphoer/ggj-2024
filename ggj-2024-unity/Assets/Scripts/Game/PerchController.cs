using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerchController : MonoBehaviour
{
  private CrowBehaviorManager _owningBird = null;

  public bool IsPerchReserved()
  {
    return _owningBird != null;
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
