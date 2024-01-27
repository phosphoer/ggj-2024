using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFX : MonoBehaviour
{

    public float LifeTime = 1f;
    public AnimationCurve TextScale = new AnimationCurve();
    public AnimationCurve TextYFloat = new AnimationCurve();
    public AnimationCurve TextXFloat = new AnimationCurve();

    float timer = 0f;
    Vector3 initScale = Vector3.one;

    // Start is called before the first frame update
    void Start()
    {
        initScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        timer+=Time.deltaTime;
        float perc = timer/LifeTime;

        transform.localScale = initScale * TextScale.Evaluate(perc);
        transform.Translate(Vector3.up * TextYFloat.Evaluate(perc) * Time.deltaTime);
        transform.Translate(Vector3.right * TextXFloat.Evaluate(perc) * Time.deltaTime);

        if(timer >= LifeTime) Destroy(gameObject);
    }
}
