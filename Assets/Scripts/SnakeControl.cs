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
    public GameObject barrierCube;

    public Material flashMaterial;
    public Material doubleMaterial;
    public Material switchMaterial;
    public Material muscleMaterial;
    public Material penetrateMaterial;
    public Material bodyMaterial;
    public Material bodyMaterial2;
    public Material bodyMaterial3;
    public Material bodyMaterial4;
    public Material bodyMaterial5;

    //
    private Dictionary<Vector3, SnakeBody> snakeBodyDic = new Dictionary<Vector3, SnakeBody>();
    private List<SnakeBody> snakeBodyList = new List<SnakeBody>();
    private List<GameObject> floorCubes = new List<GameObject>();
    private Dictionary<Vector3, GameObject> barrierDic = new Dictionary<Vector3, GameObject>();
    private SnakeBody snakeHead;

    private int[, ,] moveSpace = new int[12, 12, 12];//-1代表不可移动到，0代表小蛇占用，1代表空白，2代表可以吃的块
    private const int BARRIER = -2;
    private const int UNREACHABLE = -1;
    private const int SNAKE = 0;
    private const int BLANKSPACE = 1;
    private const int EATBODY = 2;

    //方向
    private Vector3 positiveX = new Vector3(1, 0, 0);
    private Vector3 negativeX = new Vector3(-1, 0, 0);
    private Vector3 positiveY = new Vector3(0, 1, 0);
    private Vector3 negativeY = new Vector3(0, -1, 0);
    private Vector3 positiveZ = new Vector3(0, 0, 1);
    private Vector3 negativeZ = new Vector3(0, 0, -1);

    private Vector3 gameStartPos = new Vector3(3, 1, 7);
    public Vector3 nextDirection;

    //触控操作的开始和结束位置
    public Vector2 touchBeganPos;
    public Vector2 touchEndPos;

    //控制物理计算
    private const float Acceleration = 12;
    private float startV = 0.0f;
    private const float accLength = 0.1f;
    private const float uniformLength = 0.8f;
    private float displacement = 0.0f;
    public static float atomDistance;

    private const float blastTime = 0.15f;
    private const float waitTime = 0.4f;
    private float currentWaitTime = 0;

    //private const float speedUpTime = 10;
    //private float currentSpeedUpTime = 0;

    //游戏运行状态
    public static bool gameOver = false;
    private bool gameResuming = false;
    private bool gamePaused = false;
    //private bool gameSpeedUp = false;

    private float timeScale = 0;
    private bool doAtMoveBegin = false;

    //界面上的按钮
    private Button resumeButton;
    private Button restartButton;
    private Button pauseButton;

    private int muscleCount = 0;
    private int penetrateCount = 0;

    void Start()
    {
        Advertisement.Initialize("1179800");
        InitUI();
        InitMap();
        DrawMap();
        InitSnakeHead();
        GenSnakeBody();
    }

    private void InitUI()
    {
        resumeButton = GameObject.Find("ResumeButton").GetComponent<Button>();
        restartButton = GameObject.Find("RestartButton").GetComponent<Button>();
        pauseButton = GameObject.Find("PauseButton").GetComponent<Button>();

        resumeButton.onClick.AddListener(ResumeButtonEvent);
        restartButton.onClick.AddListener(RestartButtonEvent);
        pauseButton.onClick.AddListener(PauseButtonEvent);

        resumeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
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
        pauseButton.gameObject.SetActive(true);
        ResumeGame();
    }

    void RestartButtonEvent()
    {
        StopAllCoroutines();
        StartCoroutine(RestartGame());
        resumeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    void PauseButtonEvent()
    {
        if (gamePaused)
        {
            Time.timeScale = timeScale;
            gamePaused = false;
            pauseButton.GetComponentInChildren<Text>().text = "暂停";
        }
        else
        {
            timeScale = Time.timeScale;
            Time.timeScale = 0;
            gamePaused = true;
            pauseButton.GetComponentInChildren<Text>().text = "继续";
        }
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


    //初始化地图数组
    private void InitMap()
    {
        //初始化地图数组
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                for (int k = 0; k < 12; k++)
                {
                    moveSpace[i, j, k] = UNREACHABLE;
                    if (i == 1 || j == 1 || k == 1)
                    {
                        moveSpace[i, j, k] = BLANKSPACE;
                        if (i == 11 || j == 11 || k == 11 || i == 0 || j == 0 || k == 0)
                        {
                            moveSpace[i, j, k] = UNREACHABLE;
                        }
                    }

                }
            }
        }

        //for(int i = 0; i < 5; i ++)
        //{
        //    for(int j = 0; j < 5; j ++)
        //    {
        //        for(int k = 0; k < 5; k ++)
        //        {
        //            moveSpace[i, j, k] = UNREACHABLE;
        //            if(i == 4 || j == 4 || k == 4)
        //            {
        //                moveSpace[i, j, k] = BLANKSPACE;
        //            }
        //        }
        //    }
        //}

    }

    //绘制地图
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
                        Instantiate(floorCube, new Vector3(i, j, k), Quaternion.identity);
                    }
                }
            }
        }

        //for (int i = 0; i < 5; i++)
        //{
        //    for (int j = 0; j < 5; j++)
        //    {
        //        for (int k = 0; k < 5; k++)
        //        {
        //            if (i == 4 || j == 4 || k == 4)
        //            {
        //                Instantiate(floorCube, new Vector3(i - 1, j - 1, k - 1), Quaternion.identity);
        //            }
        //        }
        //    }
        //}
    }

    //初始化蛇头
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

    //
    private void ResumeGame()
    {
        gameResuming = true;
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
        var blastBodys = GameObject.FindGameObjectsWithTag("BlastBody");
        //Debug.Log(blastBodys.Length);
        foreach (var blastBody in blastBodys)
        {
            GameObject.Destroy(blastBody);
        }
    }

    //死亡爆炸
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

    //重新开始游戏
    IEnumerator RestartGame()
    {
        Debug.Log("RestartGame");
        foreach (var body in snakeBodyList)
        {
            Destroy(body.gameObject);
        }
        foreach (var body in snakeBodyDic.Values)
        {
            Destroy(body.gameObject);
        }

        var blastBodys = GameObject.FindGameObjectsWithTag("BlastBody");
        //Debug.Log(blastBodys.Length);
        foreach (var blastBody in blastBodys)
        {
            Destroy(blastBody);
        }
        foreach(var barrier in barrierDic.Values)
        {
            Destroy(barrier);
        }

        snakeBodyDic.Clear();
        snakeBodyList.Clear();

        InitMap();
        InitSnakeHead();
        GenSnakeBody();
        gameOver = false;
        gameResuming = false;
        //gameSpeedUp = false;
        //currentSpeedUpTime = 0;
        currentWaitTime = 0;
        Time.timeScale = 1.0f;
        muscleCount = 0;
        penetrateCount = 0;
        yield return null;
    }



    void Update()
    {
        ProcessUserInput();
    }

    void FixedUpdate()
    {
        if (!gameOver && !gameResuming)
        {
            //if (speedUp)
            //{
            //    currentSpeedUpTime += Time.deltaTime;
            //    if (currentSpeedUpTime > speedUpTime)
            //    {
            //        speedUp = false;
            //        currentSpeedUpTime = 0;
            //        Time.timeScale /= 2;
            //    }
            //}
            DoMove();
        }
    }

    //
    private bool CheckGameOver()
    {
        //Vector3 tempNextPos = headNextPos + nextDirection;
        Vector3 headNextPos = snakeHead.currentPos + snakeHead.moveDirection;// +nextDirection;
        //if (headNextPos.x < 0 || headNextPos.x > 9 || headNextPos.y < 0 || headNextPos.y > 9 || headNextPos.z < 0 || headNextPos.z > 9)
        //{
        //    Debug.Log("Game Over, out of map");
        //    return true;
        //}
        if (moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] == SNAKE)
        {
            if (penetrateCount > 0)
            {
                penetrateCount--;
                return false;
            }
            else
            {
                Debug.Log("Game Over, hit self");
                return true;
            }
        }
        if (moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] == UNREACHABLE)
        {
            Debug.Log("Game Over");
            return true;
        }
        if (moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] == BARRIER)
        {
            if (muscleCount > 0)
            {
                muscleCount--;
                GameObject barrier = null;
                barrierDic.TryGetValue(headNextPos, out barrier);
                if (barrier != null)
                {
                    barrier.GetComponent<BarrierControl>().Blast();
                    barrierDic.Remove(headNextPos);
                }
                return false;
            }
            else
            {
                Debug.Log("Game Over, hit barrier");
                return true;
            }
        }
        return false;
    }

    //处理用户输入
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
        if (gameResuming)
        {
            headNextPos = snakeHead.currentPos;
        }
        int x = (int)headNextPos.x;
        int y = (int)headNextPos.y;
        int z = (int)headNextPos.z;
        Debug.Log(headNextPos);

        bool changed = false;

        if (direction == positiveX)
        {
            if (checkMove(moveSpace[x + 1, y, z]))
            {
                nextDirection = positiveX;
                changed = true;
            }
        }
        else if (direction == negativeX)
        {
            if (checkMove(moveSpace[x - 1, y, z]))
            {
                nextDirection = negativeX;
                changed = true;
            }
        }
        else if (direction == positiveY)
        {
            if (checkMove(moveSpace[x, y + 1, z]))
            {
                nextDirection = positiveY;
                changed = true;
            }
        }
        else if (direction == negativeY)
        {
            if (checkMove(moveSpace[x, y - 1, z]))
            {
                nextDirection = negativeY;
                changed = true;
            }
        }
        else if (direction == positiveZ)
        {
            if (checkMove(moveSpace[x, y, z + 1]))
            {
                nextDirection = positiveZ;
                changed = true;
            }
        }
        else if (direction == negativeZ)
        {
            if (checkMove(moveSpace[x, y, z - 1]))
            {
                nextDirection = negativeZ;
                changed = true;
            }
        }

        if (changed)
        {
            currentWaitTime = waitTime + 0.1f;
            if (gameResuming)
            {
                //gameResuming = false;
                ProcessDirectionChange();
            }
        }
    }

    private bool checkMove(int spaceValue)
    {
        bool flag = false;
        switch (spaceValue)
        {
            case BARRIER:
                {
                    if(muscleCount > 0)
                    {
                        flag = true;
                    }
                    break;
                }
            case UNREACHABLE:
                {
                    flag = false;
                    break;
                }
            case SNAKE:
                {
                    if(penetrateCount > 0)
                    {
                        flag = true;
                    }
                    break;
                }
            case BLANKSPACE:
                {
                    flag = true;
                    break;
                }
            case EATBODY:
                {
                    flag = true;
                    break;
                }
        }
        return flag;
    }


    private void ProcessGameOver()
    {
        StartCoroutine(ShowBlast());

        gameOver = true;
        //headNextPos -= nextDirection;
        atomDistance = 0;
        pauseButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(true);

        //if (Advertisement.IsReady())
        {
            resumeButton.gameObject.SetActive(true);
            restartButton.GetComponent<RectTransform>().localPosition = new Vector3(0, -100, 0);
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
        if (doAtMoveBegin)
        {
            return;
        }
        if (CheckGameOver())
        {
            ProcessGameOver();
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
            doAtMoveBegin = false;
            //处理物理，重新开始加速
            displacement = 0;
            startV = 0;
            tempA = Acceleration;

            //if (CheckGameOver())
            //{
            //    ProcessGameOver();
            //    return;
            //}

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
                GenSnakeBody();

                if (bodyCube.isDouble)
                {
                    GenSnakeBody();
                }
                if (bodyCube.isFlash)
                {
                    Time.timeScale++;
                    //currentSpeedUpTime = 0;
                    //speedUp = true;
                }
                else if (Time.timeScale > 1)
                {
                    //speedUp = false;
                    Time.timeScale--;
                }
                if (bodyCube.isSwitch)
                {
                    ProcessSwitch();
                }
                if (bodyCube.isMuscle)
                {
                    muscleCount++;
                }
                if (bodyCube.isPenetrate)
                {
                    penetrateCount++;
                }

                snakeBodyList.Add(bodyCube);
                snakeBodyDic.Remove(headNextPos);
            }
            moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] = BLANKSPACE;
        }
    }

    private void ProcessDirectionChange()
    {
        if (!gameResuming)
        {
            //处理尾部
            SnakeBody lastBody = snakeBodyList[snakeBodyList.Count - 1];
            moveSpace[lastBody.posX, lastBody.posY, lastBody.posZ] = BLANKSPACE;
            //处理头部
            snakeHead.moveOneStep();
            moveSpace[snakeHead.posX, snakeHead.posY, snakeHead.posZ] = SNAKE;

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
        }
        else
        {
            gameResuming = false;
        }

        snakeHead.moveDirection = nextDirection;
    }

    //生成新的块
    private void GenSnakeBody()
    {
        //int zeroIndex = Mathf.CeilToInt(Random.Range(-0.999f, 1.999f));
        int[] index = new int[3];
        int count = 0;
        do
        {
            count = 0;
            for (int i = 0; i < 3; i++)
            {
                //index[i] = (i == zeroIndex ? 0 : Mathf.CeilToInt(Random.Range(-0.999f, 8.999f)));
                index[i] = Mathf.CeilToInt(Random.Range(-0.999f, 8.999f));
                if (index[i] == 4)
                {
                    count++;
                }
            }
            if (count >= 2)
            {
                continue;
            }
        }
        while (moveSpace[index[0], index[1], index[2]] != BLANKSPACE);

        moveSpace[index[0], index[1], index[2]] = EATBODY;
        Vector3 position = new Vector3(index[0], index[1], index[2]);
        GameObject newBodyCube = (GameObject)Instantiate(snakeBodyCube, position, Quaternion.identity);
        SnakeBody body = newBodyCube.GetComponent<SnakeBody>();
        body.setPos(position);

        int randomType = Mathf.CeilToInt(Random.value * 10);
        //if (snakeBodyList.Count > 2)
        {
            switch (randomType)
            {
                case 1:
                    {
                        newBodyCube.GetComponent<Renderer>().material = bodyMaterial2;
                        break;
                    }
                case 2:
                    {
                        newBodyCube.GetComponent<Renderer>().material = bodyMaterial3;
                        break;
                    }
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
                        newBodyCube.GetComponent<Renderer>().material = switchMaterial;
                        body.isSwitch = true;
                        break;
                    }
                case 6:
                    {
                        newBodyCube.GetComponent<Renderer>().material = muscleMaterial;
                        body.isMuscle = true;
                        break;
                    }
                case 7:
                    {
                        newBodyCube.GetComponent<Renderer>().material = penetrateMaterial;
                        body.isPenetrate = true;
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

    }

    private void ProcessSwitch()
    {
        List<int[]> indexes = new List<int[]>();

        int[] index = getBlankSpace();
        indexes.Add(index);
        bool flag = false;
        for (int i = 0; i < 9; i++)
        {
            flag = false;
            var random = Random.value;
            if (random < 0.1)
            {
                if (moveSpace[index[0] + 1, index[1], index[2]] == BLANKSPACE)
                {
                    index[0]++;
                    indexes.Add(index);
                    flag = true;
                }
            }
            else if(random < 0.3)
            {
                if (moveSpace[index[0] - 1, index[1], index[2]] == BLANKSPACE)
                {
                    index[0]--;
                    indexes.Add(index);
                    flag = true;
                }
            }
            else if (random < 0.5)
            {
                if (moveSpace[index[0], index[1]+1, index[2]] == BLANKSPACE)
                {
                    index[1]++;
                    indexes.Add(index);
                    flag = true;
                }
            }
            else if (random < 0.7)
            {
                if (moveSpace[index[0], index[1]-1, index[2]] == BLANKSPACE)
                {
                    index[1]--;
                    indexes.Add(index);
                    flag = true;
                }
            }
            else if (random < 0.8)
            {
                if (moveSpace[index[0], index[1], index[2]+1] == BLANKSPACE)
                {
                    index[2]++;
                    indexes.Add(index);
                    flag = true;
                }
            }
            else if (random < 1)
            {
                if (moveSpace[index[0], index[1], index[2]-1] == BLANKSPACE)
                {
                    index[2]--;
                    indexes.Add(index);
                    flag = true;
                }
            }

            if(!flag)
            {
                i--;
                index = getBlankSpace();
                continue;
            }
            moveSpace[index[0], index[1], index[2]] = BARRIER;
            var barrier = Instantiate(barrierCube);
            barrier.GetComponent<BarrierControl>().setPos(index);

            barrierDic.Add(new Vector3(index[0], index[1], index[2]), barrier.gameObject);
        }

    }

    private int[] getBlankSpace()
    {
        int[] index = new int[3];
        do
        {
            for (int i = 0; i < 3; i++)
            {
                //index[i] = (i == zeroIndex ? 0 : Mathf.CeilToInt(Random.Range(-0.999f, 8.999f)));
                index[i] = Mathf.CeilToInt(Random.Range(0.001f, 9.999f));
            }
        }
        while (moveSpace[index[0], index[1], index[2]] != BLANKSPACE);

        return index;
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
