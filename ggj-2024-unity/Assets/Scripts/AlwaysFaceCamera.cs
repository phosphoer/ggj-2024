using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{

    Transform cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = FindObjectsOfType<Camera>()[0].transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(cam != null)
        {
            transform.LookAt(cam);
            transform.forward *= -1;
        }
    }
}
