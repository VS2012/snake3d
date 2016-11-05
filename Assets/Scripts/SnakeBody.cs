using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnakeBody : MonoBehaviour
{
    public GameObject blastCube;
    public SnakeBody preBody;
    public Vector3 instantPos = new Vector3();//瞬时位置
    public Vector3 currentPos = new Vector3();

    private Vector3 appearScale = Vector3.zero;

    public int posX;
    public int posY;
    public int posZ;

    public Vector3 preDirection;
    public Vector3 moveDirection;
    public bool appeared = false;
    public bool appending = false;
    public bool eat = false;
    //public bool gameover = false;
    public bool isFlash = false;
    public bool isDouble = false;
    public bool isSwitch = false;

    void Start()
    {
        //StartCoroutine(Appear());
    }


    void FixedUpdate()
    {
        if (SnakeControl.gameOver)
        {
            return;
        }

        if (!appeared)
        {
            //Debug.Log(appearScale);
            transform.localScale = appearScale;
            transform.Rotate(0, Time.deltaTime * 180, 0);

            appearScale.x += 2 * Time.deltaTime;
            appearScale.y += 2 * Time.deltaTime;
            appearScale.z += 2 * Time.deltaTime;

            if (appearScale.x > 1)
            {
                appearScale.x = 1;
                appearScale.y = 1;
                appearScale.z = 1;
                transform.localScale = appearScale;
                appeared = true;
            }
        }

        if (appending)
        {

        }

        if (eat)
        {
            DoMove();
        }
    }

    private void DoMove()
    {
        instantPos += SnakeControl.atomDistance * moveDirection;
        transform.position = instantPos;
    }

    public void setPos(Vector3 newPos)
    {
        instantPos.Set(newPos.x, newPos.y, newPos.z);
        currentPos.Set(newPos.x, newPos.y, newPos.z);
        posX = (int)newPos.x;
        posY = (int)newPos.y;
        posZ = (int)newPos.z;
    }

    public void moveOneStep()
    {
        currentPos += moveDirection;
        setPos(currentPos);
    }

    public void Appear()
    {
        gameObject.transform.localScale = Vector3.zero;
        appearScale = Vector3.zero;
        appeared = false;
    }

    public void Blast()
    {
        blastCube.GetComponent<Renderer>().material = gameObject.GetComponent<Renderer>().material;
        Vector3 position = new Vector3();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    position.x = posX - 0.375f + i * 0.25f;
                    position.y = posY - 0.375f + j * 0.25f;
                    position.z = posZ - 0.375f + k * 0.25f;
                    Instantiate(blastCube, position, Quaternion.identity);
                }
            }
        }
    }
}
