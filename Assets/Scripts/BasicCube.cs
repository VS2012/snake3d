using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCube : MonoBehaviour
{
    public GameObject blastCube;

    public void BlastAndDestory()
    {
        //Blast(3, true, true);
    }

    public void Blast(int count = 3, bool playSound = true)
    {
        StartCoroutine(BlastAsync(count));
        if (playSound)
            AudioControl.instance.playBlastSound();
        NativeInterface.TapTicImpact();
    }

    IEnumerator BlastAsync(int count)
    {
        float subCubeLength = 1 / (float)count;
        float physicsLength = subCubeLength + 0.2f / count;
        float offset = (subCubeLength / 2) * (count - 1);
        blastCube.GetComponent<Renderer>().material = gameObject.GetComponent<Renderer>().material;
        blastCube.transform.localScale = new Vector3(physicsLength, physicsLength, physicsLength);
        Vector3 position = new Vector3();

        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                for (int k = 0; k < count; k++)
                {
                    position.x = transform.localPosition.x - offset + i * subCubeLength;
                    position.y = transform.localPosition.y - offset + j * subCubeLength;
                    position.z = transform.localPosition.z - offset + k * subCubeLength;
                    Instantiate(blastCube, position, Quaternion.identity);
                }
            }
        }
        yield return new WaitForEndOfFrame();
    }
}

