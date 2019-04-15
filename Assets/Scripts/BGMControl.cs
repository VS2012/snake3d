using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMControl : MonoBehaviour
{
    AudioSource AudioBGM;
    public static BGMControl instance;

    void Awake()
    {
        instance = this;
        AudioBGM = GetComponent<AudioSource>();
    }

    void Start()
    {
        

    }

    public void Pause()
    {
        AudioBGM.Pause();
    }

    public void Play()
    {
        AudioBGM.Play();
    }
}
