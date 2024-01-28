using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayAudioOnStart : MonoBehaviour
{
    public bool SetRandomPitch = true;
    public Vector2 PitchRangeScale = new Vector2(1.1f,0.9f);

    float initPitch = 1f;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource src = gameObject.GetComponent<AudioSource>();
        initPitch = src.pitch;

        if(SetRandomPitch)
        {
            src.pitch = initPitch * Random.Range(PitchRangeScale.x, PitchRangeScale.y);
        }
        src.Play();
    }
}
