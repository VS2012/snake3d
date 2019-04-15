using UnityEngine;
using System.Collections;

public class BarrierControl : BasicCube
{
    private Vector3 location = new Vector3();
    private Vector3 moveD = new Vector3();
    private float speed = 0.01f;
    private float displacement = 0;
    //private float totalLength = 0;
    private Vector3 instantPos = new Vector3();

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if(displacement >= 1)
        {
            return;
        }
        displacement += speed;
        instantPos += speed * moveD;
        transform.position = instantPos;
    }

    public void setPos(Vector3 pos)
    {
        location = pos;
        instantPos = pos;

        if(pos[0] == 1)
        {
            instantPos.x = 0;
            moveD = new Vector3(1, 0, 0);
            transform.Rotate(0, 0, 90);
        }
        else if (pos[1] == 1)
        {
            instantPos.y = 0;
            moveD = new Vector3(0, 1, 0);
        }
        else if (pos[2] == 1)
        {
            instantPos.z = 0;
            moveD = new Vector3(0, 0, 1);
            transform.Rotate(90, 0, 0);
        }
        transform.position = instantPos;
    }

    /*
    public void Blast()
    {
        StartCoroutine(BlastAsync(4));
        AudioControl.instance.playBlastSound();
    }

    IEnumerator BlastAsync(int num)
    {
        blastCube.GetComponent<Renderer>().material = gameObject.GetComponent<Renderer>().material;
        Vector3 position = new Vector3();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    position.x = location[0] - 0.375f + i * 0.25f;
                    position.y = location[1] - 0.375f + j * 0.25f;
                    position.z = location[2] - 0.375f + k * 0.25f;
                    Instantiate(blastCube, position, Quaternion.identity);
                }
            }
        }
        Destroy(gameObject);
        yield return new WaitForEndOfFrame();
    }
    */
}
