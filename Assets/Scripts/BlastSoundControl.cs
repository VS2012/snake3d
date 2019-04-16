using System.Collections;
using UnityEngine;

public class BlastSoundControl : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip[] blastSound;
    private WaitForSeconds waitInterval;

    private void Awake()
    {
        waitInterval = new WaitForSeconds(0.1f);
        audioSource = GetComponent<AudioSource>();
    }
    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        Debug.Log("playBlastSound " + AudioControl.blastCount);
#endif
        StartCoroutine(playSound(blastSound[AudioControl.blastCount]));
        AudioControl.blastCount++;
        AudioControl.blastCount %= blastSound.Length;
    }

    IEnumerator playSound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
        while (audioSource.isPlaying)
            yield return waitInterval;
        Destroy(gameObject);
    }
}
