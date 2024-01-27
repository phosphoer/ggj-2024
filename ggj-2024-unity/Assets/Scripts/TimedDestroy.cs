using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    public float Countdown = 1f;
    public GameObject ToDestroy;

    float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= Countdown){
            if(ToDestroy != null) Destroy(ToDestroy);
            else Destroy(gameObject);
        }
    }
}
