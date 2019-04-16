using UnityEngine;
using System.Collections;

public class BlastControl : MonoBehaviour
{
    private WaitForSeconds checkInterval;

    void Start()
    {
        checkInterval = new WaitForSeconds(0.1f);
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
            yield return checkInterval;
        }
    }
}
