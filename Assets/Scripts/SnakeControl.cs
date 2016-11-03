using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class SnakeControl : MonoBehaviour
{
    public GameObject floorCube;
    public GameObject snakeHeadCube;
    public GameObject snakeBodyCube;
    private SnakeBody snakeHead;

    public Material adMaterial;
    public Material flashMaterial;
    public Material doubleMaterial;
    public Material bodyMaterial;
    public Material bodyMaterial2;
    public Material bodyMaterial3;
    public Material bodyMaterial4;
    public Material bodyMaterial5;

    private Dictionary<Vector3, SnakeBody> snakeBodyDic = new Dictionary<Vector3, SnakeBody>();
    private List<SnakeBody> snakeBodyList = new List<SnakeBody>();

    private int[, ,] moveSpace = new int[10, 10, 10];//-1代表不可移动到，0代表小蛇占用，1代表空白，2代表可以吃的块
    private int UNREACHABLE = -1;
    private int SNAKE = 0;
    private int BLANKSPACE = 1;
    private int EATBODY = 2;

    private Vector3 positiveX = new Vector3(1, 0, 0);
    private Vector3 negativeX = new Vector3(-1, 0, 0);
    private Vector3 positiveY = new Vector3(0, 1, 0);
    private Vector3 negativeY = new Vector3(0, -1, 0);
    private Vector3 positiveZ = new Vector3(0, 0, 1);
    private Vector3 negativeZ = new Vector3(0, 0, -1);

    private Vector3 gameStartPos = new Vector3(3, 0, 7);
    //public Vector3 headNextPos;
    public Vector3 nextDirection;

    public Vector2 touchBeganPos;
    public Vector2 touchEndPos;

    private const float Acceleration = 12;
    private float startV = 0.0f;
    private float displacement = 0.0f;
    private const float accLength = 0.1f;
    private const float uniformLength = 0.8f;
    public static float atomDistance;

    private bool speedUp = false;
    private const float waitTime = 0.4f;
    private float currentWaitTime = 0;

    private const float speedUpTime = 10;
    private float currentSpeedUpTime = 0;

    private const float blastTime = 0.15f;
    public static bool gameOver = false;
    private bool resuming = false;
    private bool doAtMoveBegin = false;


    //private Text lifeCountText;
    //private int lifeCount;

    private Button resumeButton;
    private Button restartButton;

    void Start()
    {
        Advertisement.Initialize("1179800");
        InitUI();
        InitMap();
        DrawMap();
        InitSnakeHead();
        GenSnakeBody();
    }

    void ResumeButtonEvent()
    {
        StopAllCoroutines();
        const string RewardedZoneId = "1179800";
        if (Advertisement.IsReady(RewardedZoneId))
        {
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show(RewardedZoneId, options);
        }
        else if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
        else
        {
            Debug.Log("not ready");
        }
        //ShowRewardedAd();
        resumeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        ResumeGame();
    }

    void RestartButtonEvent()
    {
        StopAllCoroutines();
        StartCoroutine(RestartGame());
        resumeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    public void ShowDefaultAd()
    {
        Debug.Log("ShowDefaultAd");
        if (!Advertisement.IsReady())
        {
            Debug.Log("Ads not ready for default zone");
            return;
        }
        Advertisement.Show();
    }

    public void ShowRewardedAd()
    {
        Debug.Log("ShowRewardedAd");
        const string RewardedZoneId = "1179800";

        if (!Advertisement.IsReady(RewardedZoneId))
        {
            Debug.Log(string.Format("Ads not ready for zone '{0}'", RewardedZoneId));
            return;
        }

        var options = new ShowOptions { resultCallback = HandleShowResult };
        Advertisement.Show(RewardedZoneId, options);
    }

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                //
                // YOUR CODE TO REWARD THE GAMER
                // Give coins etc.
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }

    private void InitUI()
    {
        resumeButton = GameObject.Find("ResumeButton").GetComponent<Button>();
        restartButton = GameObject.Find("RestartButton").GetComponent<Button>();

        resumeButton.onClick.AddListener(ResumeButtonEvent);
        restartButton.onClick.AddListener(RestartButtonEvent);

        resumeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    private void InitMap()
    {
        //初始化地图数组
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                for (int k = 0; k < 10; k++)
                {
                    moveSpace[i, j, k] = UNREACHABLE;
                    if (i == 0 || j == 0 || k == 0)
                    {
                        moveSpace[i, j, k] = BLANKSPACE;
                    }
                }
            }
        }
    }

    private void DrawMap()
    {
        //绘制地图
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                for (int k = 0; k < 11; k++)
                {
                    if (i == 0 || j == 0 || k == 0)
                    {
                        Instantiate(floorCube, new Vector3(i - 1, j - 1, k - 1), Quaternion.identity);
                    }
                }
            }
        }
    }

    private void InitSnakeHead()
    {
        GameObject obj = (GameObject)Instantiate(snakeHeadCube, gameStartPos, Quaternion.identity);
        snakeHead = obj.GetComponent<SnakeBody>();

        snakeHead.setPos(gameStartPos);
        snakeHead.eat = true;
        snakeHead.preDirection = negativeX;
        snakeHead.moveDirection = negativeX;

        nextDirection = negativeX;
        //headNextPos = snakeHead.currentPos + snakeHead.moveDirection;
        
        snakeBodyList.Add(snakeHead);
        moveSpace[snakeHead.posX, snakeHead.posY, snakeHead.posZ] = SNAKE;
    }

    IEnumerator ShowBlast()
    {
        foreach (var body in snakeBodyList)
        {
            body.Blast();
            body.gameObject.SetActive(false);
            yield return new WaitForSeconds(blastTime);
        }
        foreach (var body in snakeBodyDic.Values)
        {
            body.Blast();
            body.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(blastTime);
    }

    IEnumerator RestartGame()
    {
        Debug.Log("RestartGame");
        foreach (var body in snakeBodyList)
        {
            GameObject.Destroy(body.gameObject);
        }
        foreach (var body in snakeBodyDic.Values)
        {
            GameObject.Destroy(body.gameObject);
        }

        snakeBodyDic.Clear();
        snakeBodyList.Clear();

        InitMap();
        InitSnakeHead();
        GenSnakeBody();
        gameOver = false;
        resuming = false;
        speedUp = false;
        //lifeCount = 0;
        //lifeCountText.text = "X " + lifeCount;
        currentSpeedUpTime = 0;
        currentWaitTime = 0;
        Time.timeScale = 1.0f;
        yield return null;
    }

    private void ResumeGame()
    {
        resuming = true;
        gameOver = false;
        atomDistance = 0;
        //snakeHead.currentPos -= snakeHead.moveDirection;
        foreach (var body in snakeBodyList)
        {
            body.gameObject.SetActive(true);
            body.Appear();
        }
        foreach (var body in snakeBodyDic.Values)
        {
            body.gameObject.SetActive(true);
            body.Appear();
        }
    }

    void Update()
    {
        ProcessUserInput();
    }

    void FixedUpdate()
    {
        if (!gameOver && !resuming)
        {
            if (speedUp)
            {
                currentSpeedUpTime += Time.deltaTime;
                if (currentSpeedUpTime > speedUpTime)
                {
                    speedUp = false;
                    currentSpeedUpTime = 0;
                    Time.timeScale /= 2;
                }
            }
            DoMove();
        }
    }

    private bool CheckGameOver()
    {
        //Vector3 tempNextPos = headNextPos + nextDirection;
        Vector3 headNextPos = snakeHead.currentPos + snakeHead.moveDirection + nextDirection;
        if (headNextPos.x < 0 || headNextPos.x > 9 || headNextPos.y < 0 || headNextPos.y > 9 || headNextPos.z < 0 || headNextPos.z > 9)
        {
            Debug.Log("Game Over, out of map");
            return true;
        }
        if (moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] == SNAKE)
        {
            Debug.Log("Game Over, hit self");
            return true;
        }
        return false;
    }

    private void ProcessUserInput()
    {
        if (Input.GetKey("space"))
        {
            printState();
        }

        //触控操作
        //tan75 = 3.732
        //tan70 = 2.75
        //tan65 = 2.1445
        //tan60 = 1.732
        //tan55 = 1.428
        //tan50 = 1.192
        //tan45 = 1
        //tan15 = 0.268
        //tan10 = 0.176
        //tan5 = 0.0875
        //tan0 = 0
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Canceled)
            {
                Debug.Log("Canceled");
            }
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                touchBeganPos = Input.GetTouch(0).position;
                //Debug.Log("began: " + touchBeganPos);
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                touchEndPos = Input.GetTouch(0).position;

                Vector3 tempDirection = new Vector3();
                float tan = (touchEndPos.y - touchBeganPos.y) / (touchEndPos.x - touchBeganPos.x);


                if (tan < 1.732 && tan > 0)
                {
                    tempDirection = negativeX;
                }
                if (tan < -0 && tan > -1.732)
                {
                    tempDirection = positiveZ;
                }
                if (tan < -1.732)
                {
                    tempDirection = negativeY;
                }
                if (tan > 1.732)
                {
                    tempDirection = positiveY;
                }
                if (touchEndPos.x - touchBeganPos.x < 0)
                {
                    tempDirection = -tempDirection;
                }
                //Debug.Log("began: " + touchBeganPos + " end: " + touchEndPos + " tan:" + tan + " d:" + tempDirection);
                TryChangeDirection(tempDirection);
            }
        }

        //鼠标滑动操作
        if (Input.GetMouseButtonDown(0))
        {
            touchBeganPos = Input.mousePosition;
            //Debug.Log("began: " + touchBeganPos);
        }
        if (Input.GetMouseButtonUp(0))
        {
            touchEndPos = Input.mousePosition;

            Vector3 tempDirection = new Vector3();
            float tan = (touchEndPos.y - touchBeganPos.y) / (touchEndPos.x - touchBeganPos.x);

            if (tan < 1.732 && tan > 0)
            {
                tempDirection = negativeX;
            }
            if (tan < -0 && tan > -1.732)
            {
                tempDirection = positiveZ;
            }
            if (tan < -1.732)
            {
                tempDirection = negativeY;
            }
            if (tan > 1.732)
            {
                tempDirection = positiveY;
            }
            if (touchEndPos.x - touchBeganPos.x < 0)
            {
                tempDirection = -tempDirection;
            }
            //Debug.Log("began: " + touchBeganPos + " end: " + touchEndPos + " tan:" + tan + " d:" + tempDirection);
            TryChangeDirection(tempDirection);
        }

        //键盘操作
        if (Input.GetKey("i") || Input.GetKey("w"))
        {
            TryChangeDirection(positiveY);
        }
        else if (Input.GetKey("k") || Input.GetKey("s"))
        {
            TryChangeDirection(negativeY);
        }
        else if (Input.GetKey("j") || Input.GetKey("a"))
        {
            TryChangeDirection(negativeZ);
        }
        else if (Input.GetKey("l") || Input.GetKey("d"))
        {
            TryChangeDirection(positiveZ);
        }
        else if (Input.GetKey("u") || Input.GetKey("q"))
        {
            TryChangeDirection(positiveX);
        }
        else if (Input.GetKey("o") || Input.GetKey("e"))
        {
            TryChangeDirection(negativeX);
        }
    }

    private void TryChangeDirection(Vector3 direction)
    {
        Debug.Log("TryChangeDirection " + direction);

        Vector3 headNextPos = snakeHead.currentPos + snakeHead.moveDirection;
        int x = (int)headNextPos.x;
        int y = (int)headNextPos.y;
        int z = (int)headNextPos.z;
        Debug.Log(headNextPos);

        bool changed = false;

        if (direction == positiveX)
        {
            if (x + 1 > 9)
            {
                Debug.Log("out of range");
            }
            else if (moveSpace[x + 1, y, z] > 0)
            {
                nextDirection = positiveX;
                changed = true;
            }
        }
        else if (direction == negativeX)
        {
            if (x - 1 < 0)
            {
                Debug.Log("out of range");
            }
            else if (moveSpace[x - 1, y, z] > 0)
            {
                nextDirection = negativeX;
                changed = true;
            }
        }
        else if (direction == positiveY)
        {
            if (y + 1 > 9)
            {
                Debug.Log("out of range");
            }
            else if (moveSpace[x, y + 1, z] > 0)
            {
                nextDirection = positiveY;
                changed = true;
            }
        }
        else if (direction == negativeY)
        {
            if (y - 1 < 0)
            {
                Debug.Log("out of range");
            }
            else if (moveSpace[x, y - 1, z] > 0)
            {
                nextDirection = negativeY;
                changed = true;
            }
        }
        else if (direction == positiveZ)
        {
            if (z + 1 > 9)
            {
                Debug.Log("out of range");
            }
            else if (moveSpace[x, y, z + 1] > 0)
            {
                nextDirection = positiveZ;
                changed = true;
            }
        }
        else if (direction == negativeZ)
        {
            if (z - 1 < 0)
            {
                Debug.Log("out of range");
            }
            else if (moveSpace[x, y, z - 1] > 0)
            {
                nextDirection = negativeZ;
                changed = true;
            }
        }
       
        if(changed)
        {
            currentWaitTime = waitTime + 0.1f;
            if(resuming)
            {
                resuming = false;
                ProcessDirectionChange();
            }
        }
    }

    private bool checkMove(int x, int y, int z)
    {
        if (x * y * z != 0)
            return false;
        if (moveSpace[x, y, z] <= 0)
            return false;
        return true;
    }

    private void ProcessGameOver()
    {
        StartCoroutine(ShowBlast());

        gameOver = true;
        //headNextPos -= nextDirection;
        atomDistance = 0;
        
        restartButton.gameObject.SetActive(true);

        //if (Advertisement.IsReady())
        {
            resumeButton.gameObject.SetActive(true);
            restartButton.GetComponent<RectTransform>().localPosition = new Vector3(0, -50, 0);
            restartButton.GetComponentInChildren<Text>().text = "我才不看广告呢╭(╯^╰)╮";
        }
        //else
        //{
        //    restartButton.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        //    restartButton.GetComponentInChildren<Text>().text = "重新开始/(ㄒoㄒ)/~~";
        //}
        //Time.timeScale = 0;
        
    }

    //判断是否吃到
    private void DoAtMoveBegin()
    {
        if(doAtMoveBegin)
        {
            return;
        }
        
        ProcessEat();
        doAtMoveBegin = true;
    }

    private void DoMove()
    {
        DoAtMoveBegin();

        float tempA = Acceleration;

        //通过方块的位移判断方块的状态
        if (displacement >= accLength)
        {
            tempA = 0;
        }
        if (displacement >= uniformLength + accLength)
        {
            tempA = -Acceleration;
        }
        if (displacement > 0.999)
        {
            doAtMoveBegin = false;
            currentWaitTime += Time.deltaTime;
            if (currentWaitTime < waitTime)
            {
                atomDistance = 0;
                return;
            }
            else
            {
                currentWaitTime = 0;
            }
            //处理物理，重新开始加速
            displacement = 0;
            startV = 0;
            tempA = Acceleration;

            if (CheckGameOver())
            {
                ProcessGameOver();
                return;
            }

            ProcessDirectionChange();
        }

        //计算物理
        float endV = startV + tempA * Time.fixedDeltaTime;
        atomDistance = (startV + endV) * Time.fixedDeltaTime / 2;
        displacement += atomDistance;
        startV = endV;
    }

    private void ProcessEat()
    {
        Vector3 headNextPos = snakeHead.currentPos + snakeHead.moveDirection;
        //如果吃到
        if (moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] == EATBODY)
        {
            Debug.Log("got one!");
            //printState();

            GenSnakeBody();
            SnakeBody bodyCube = null;
            snakeBodyDic.TryGetValue(headNextPos, out bodyCube);
            if (bodyCube != null)
            {
                bodyCube.Blast();
                SnakeBody lastBody = snakeBodyList[snakeBodyList.Count - 1];
                if (lastBody.moveDirection != lastBody.preDirection)
                {
                    bodyCube.setPos(lastBody.currentPos - lastBody.preDirection);
                    bodyCube.moveDirection = lastBody.preDirection;
                }
                else
                {
                    bodyCube.setPos(lastBody.currentPos - lastBody.moveDirection);
                    bodyCube.moveDirection = lastBody.moveDirection;
                }

                moveSpace[bodyCube.posX, bodyCube.posY, bodyCube.posZ] = SNAKE;
                bodyCube.eat = true;

                if (bodyCube.isDouble)
                {
                    GenSnakeBody();
                }
                if (bodyCube.isFlash)
                {
                    Time.timeScale *= 2;
                    currentSpeedUpTime = 0;
                    speedUp = true;
                }

                snakeBodyList.Add(bodyCube);
                snakeBodyDic.Remove(headNextPos);
            }
            moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] = BLANKSPACE;
        }
    }

    private void ProcessDirectionChange()
    {
        //处理尾部
        SnakeBody lastBody = snakeBodyList[snakeBodyList.Count - 1];
        moveSpace[lastBody.posX, lastBody.posY, lastBody.posZ] = BLANKSPACE;
        //处理头部
        snakeHead.moveOneStep();
        moveSpace[snakeHead.posX, snakeHead.posY, snakeHead.posZ] = SNAKE;
        //headNextPos += nextDirection;

        //更新方向数据
        for (int i = 0; i < snakeBodyList.Count; i++)
        {
            snakeBodyList[i].preDirection = snakeBodyList[i].moveDirection;
        }
        for (int i = snakeBodyList.Count - 1; i > 0; i--)
        {
            SnakeBody body = snakeBodyList[i];
            SnakeBody bodypre = snakeBodyList[i - 1];
            body.moveOneStep();
            body.moveDirection = bodypre.preDirection;
        }

        snakeHead.moveDirection = nextDirection;
    }

    //生成新的块
    private void GenSnakeBody()
    {
        int zeroIndex = Mathf.CeilToInt(Random.Range(-0.999f, 1.999f));
        int[] index = new int[3];
        do
        {
            for (int i = 0; i < 3; i++)
            {
                index[i] = (i == zeroIndex ? 0 : Mathf.CeilToInt(Random.Range(-0.999f, 8.999f)));
            }
        }
        while (moveSpace[index[0], index[1], index[2]] != BLANKSPACE);

        moveSpace[index[0], index[1], index[2]] = EATBODY;
        Vector3 position = new Vector3(index[0], index[1], index[2]);
        GameObject newBodyCube = (GameObject)Instantiate(snakeBodyCube, position, Quaternion.identity);
        SnakeBody body = newBodyCube.GetComponent<SnakeBody>();
        body.isAD = true;
        body.setPos(position);

        int indexSum = index[0] + index[1] + index[2];
        //Debug.Log(indexSum);

        if (snakeBodyList.Count > 2)
        {
            switch (indexSum)
            {
                case 3:
                    {
                        newBodyCube.GetComponent<Renderer>().material = flashMaterial;
                        body.isFlash = true;
                        break;
                    }
                case 4:
                    {
                        newBodyCube.GetComponent<Renderer>().material = doubleMaterial;
                        body.isDouble = true;
                        break;
                    }
                case 5:
                    {
                        newBodyCube.GetComponent<Renderer>().material = bodyMaterial2;
                        break;
                    }
                case 6:
                    {
                        newBodyCube.GetComponent<Renderer>().material = bodyMaterial3;
                        break;
                    }
                case 7:
                    {
                        newBodyCube.GetComponent<Renderer>().material = bodyMaterial4;
                        break;
                    }
                case 8:
                    {
                        newBodyCube.GetComponent<Renderer>().material = bodyMaterial5;
                        break;
                    }
                case 9:
                    {
                        newBodyCube.GetComponent<Renderer>().material = bodyMaterial;
                        break;
                    }
            }
        }

        snakeBodyDic.Add(position, body);

        //Debug.Log("xyz " + index[0] + " " + index[1] + " " + index[2]);
        //Debug.Log("position " + newBodyCube.transform.position);
    }


    private void printState()
    {
        Debug.Log("----------");
        int snakeCount = 0;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                for (int k = 0; k < 10; k++)
                {
                    if (i == 0 || j == 0 || k == 0)
                    {
                        if (moveSpace[i, j, k] == SNAKE)
                        {
                            snakeCount++;
                        }
                    }
                }
            }
        }
        Debug.Log("snakeCount " + snakeCount);
        Debug.Log("snakeBodyList.Count " + snakeBodyList.Count);
        //foreach (SnakeBody body in snakeBodyList)
        //{
        //    Debug.Log("body instancePos " + body.instantPos);
        //    Debug.Log("body moveD " + body.moveDirection);
        //    Debug.Log("body preD " + body.preDirection);
        //}
    }

    private void checkSnake()
    {
        //Debug.Log("-----");
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                for (int k = 0; k < 10; k++)
                {
                    if (moveSpace[i, j, k] == SNAKE)
                    {
                        //Debug.Log("snake: " + i + " " + j + " " + k);
                    }
                }
            }
        }
    }

}
