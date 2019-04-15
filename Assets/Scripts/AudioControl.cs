using System.Collections;
using UnityEngine;

public class AudioControl : MonoBehaviour
{
    public static AudioControl instance;

    AudioSource audioSource;
    public AudioClip moveSound;
    public AudioClip normalSound;
    public AudioClip trapSound;
    public AudioClip lightSound;
    public AudioClip flashSound;
    public AudioClip muscleSound;
    public AudioClip penetrateSound;
    public AudioClip doubleSound;
    //public GameObject blastSound;
    public AudioClip[] blastSound;
    public static int blastCount = 0;

    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void playMoveSound()
    {
        audioSource.PlayOneShot(moveSound);
    }

    public void playNormalSound()
    {
        //audioSource.PlayOneShot(normalSound);
        playBlastSound();
    }
    
    public void playLightSound()
    {
        audioSource.PlayOneShot(lightSound);
    }

    public void playFlashSound()
    {
        audioSource.PlayOneShot(flashSound);
    }

    public void playMuscleSound()
    {
        audioSource.PlayOneShot(muscleSound);
    }

    public void playPenetrateSound()
    {
        audioSource.PlayOneShot(penetrateSound);
    }

    public void playDoubleSound()
    {
        audioSource.PlayOneShot(doubleSound);
    }

    public void playTrapSound()
    {
        audioSource.PlayOneShot(trapSound);
        playBlastSound();
    }

    public void playBlastSound()
    {
        audioSource.PlayOneShot(blastSound[blastCount]);
        blastCount++;
        blastCount %= blastSound.Length;
    }
}
