using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerchController : MonoBehaviour
{
  private CrowBehavior _owningBird = null;

  bool IsPerchReserved()
  {
    return _owningBird != null;
  }

  public bool ReservePerch(CrowBehavior bird)
  {
    if (!IsPerchReserved())
    {
      _owningBird= bird;
      return true;
    }

    return false;
  }

  public bool LeavePerch(CrowBehavior bird)
  {
    if (_owningBird == bird)
    {
      _owningBird= null;
      return true;
    }

    return false;
  }
}
