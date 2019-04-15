using UnityEngine;
using System.Collections;

public class SnakeBody : BasicCube
{
    public SnakeBody preBody;
    public Vector3 instantPos = new Vector3(); //瞬时位置
    public Vector3 currentPos = new Vector3();
    private Vector3 appearScale = Vector3.zero;

    //public int posX;
    //public int posY;
    //public int posZ;

    public Vector3 preDirection;
    public Vector3 moveDirection;

    public CubeType cubeType = CubeType.CommonCube;

    public bool appeared = false;
    public bool appending = false;
    public bool eat = false;

    void Start()
    {
        Appear();
        //StartCoroutine(Appear());
        //blastCube.GetComponent<Renderer>().material = gameObject.GetComponent<Renderer>().material;
    }


    void FixedUpdate()
    {
        if (SnakeControl.gameOver)
        {
            return;
        }

        /*
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
        */

        if (appending)
        {

        }

        if (eat)
        {
            DoMove();
        }
    }

    void DoMove()
    {
        instantPos += SnakeControl.atomDistance * moveDirection;
        transform.position = instantPos;
    }

    public void setPos(Vector3 newPos)
    {
        instantPos.Set(newPos.x, newPos.y, newPos.z);
        currentPos.Set(newPos.x, newPos.y, newPos.z);

        //posX = (int)newPos.x;
        //posY = (int)newPos.y;
        //posZ = (int)newPos.z;
    }

    public void moveOneStep()
    {
        currentPos += moveDirection;
        setPos(currentPos);
    }

    public void Appear()
    {
        gameObject.transform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        appearScale = Vector3.zero;
        StartCoroutine(AppearAsync());
        //appeared = false;
    }

    IEnumerator AppearAsync(float ratio = 1.2f)
    {
        while(appearScale.x < ratio)
        {
            transform.Rotate(0, Time.deltaTime * 180, 0);
            appearScale.x += 2 * Time.deltaTime;
            appearScale.y += 2 * Time.deltaTime;
            appearScale.z += 2 * Time.deltaTime;
            transform.localScale = appearScale;
            yield return null;
        }
        while(appearScale.x >= 1)
        {
            transform.Rotate(0, Time.deltaTime * 180, 0);
            appearScale.x -= 2 * Time.deltaTime;
            appearScale.y -= 2 * Time.deltaTime;
            appearScale.z -= 2 * Time.deltaTime;
            transform.localScale = appearScale;
            yield return null;
        }
        var stopRotate = Mathf.CeilToInt(transform.localRotation.y) % 90 + 1;
        while(transform.localRotation.y > stopRotate * 90)
        {
            transform.Rotate(0, Time.deltaTime * 180, 0);
            yield return null;
        }

        appearScale.x = 1;
        appearScale.y = 1;
        appearScale.z = 1;
        transform.localScale = appearScale;
        transform.localRotation = new Quaternion(0, 0, 0, 0);
        appeared = true;
    }

    /*
    public void Blast(int count = 3, bool playSound = true)
    {
        StartCoroutine(BlastAsync(count));
        if(playSound)
            AudioControl.instance.playBlastSound();
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
    */
}
