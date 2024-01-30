using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreepyLookAt : MonoBehaviour
{

    public Renderer ThisRenderer;

    bool _enabled = true;
    Vector3 originalRot = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if(ThisRenderer == null) _enabled=false;
        originalRot = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if(_enabled && !ThisRenderer.isVisible)
        {
            transform.LookAt(PlayerActorController.Instance.transform);
            transform.localEulerAngles = new Vector3(originalRot.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }
    }
}
