using UnityEngine;
using System.Collections;

public class BlastControl : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CheckAsync());
    }

    void Update()
    {
        
    }

    public void CheckDestory()
    {
        StartCoroutine(CheckAsync());
    }

    IEnumerator CheckAsync()
    {
        while(gameObject.activeSelf)
        {
            if (transform.position.y < -2)
            {
                Destroy(gameObject);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
