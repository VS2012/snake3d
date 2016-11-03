using UnityEngine;
using System.Collections;

public class BlastControl : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        if (transform.position.x < -1 || transform.position.y < -1 || transform.position.z < -1)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
