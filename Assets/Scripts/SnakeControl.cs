using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;

//程序控制逻辑不能写在 Coroutine 里；UI 动画 特效 这些没有问题。

public class SnakeControl : MonoBehaviour
{
    public GameObject floorCube;
    public GameObject snakeHeadCube;
    public GameObject snakeBodyCube;
    public GameObject barrierCube;

    //public Light light;
    public Camera myCamera;
    Light headLight;
    SwitchLight switchLight;

    public Material flashMaterial; //加速
    public Material doubleMaterial; //双倍
    public Material switchMaterial; //机关
    public Material muscleMaterial; //肌肉
    public Material penetrateMaterial; //穿透
    public Material lightOffMaterial; //关灯
    public Material lightOnMaterial; //开灯
    public Material trapMaterial; //陷阱
    public Material bodyMaterial;
    public Material bodyMaterial2;
    public Material bodyMaterial3;
    public Material bodyMaterial4;
    public Material bodyMaterial5;

    Dictionary<Vector3, SnakeBody> snakeFoodDic = new Dictionary<Vector3, SnakeBody>(); //保存所有可以吃的块
    List<SnakeBody> snakeBodyList = new List<SnakeBody>(); //保存小蛇身体的块
    Dictionary<Vector3, BarrierControl> barrierDic = new Dictionary<Vector3, BarrierControl>(); //保存障碍块
    SnakeBody snakeHead;

    // int[, ,] moveSpace = new int[12, 12, 12];//-1代表不可移动到，0代表小蛇占用，1代表空白，2代表可以吃的块
    Dictionary<Vector3, int> MapDic = new Dictionary<Vector3, int>();
    Dictionary<Vector3, GameObject> CubeObjectDic = new Dictionary<Vector3, GameObject>();
    const int BARRIER = -2;
    const int FLOOR = -1;
    const int SNAKE = 0;
    const int BLANKSPACE = 1;
    const int FOOD = 2;
    int mapSizeX = 10;
    int mapSizeZ = 10;
    static int borderLeft = 0;
    static int borderRight = 9;
    static int borderForward = 9;
    static int borderBack = 0;

    const int BARRIER_NUM = 6;
    Vector3 gameStartPos = new Vector3(7, 1, 7); //游戏开始时蛇头的位置
    public Vector3 nextDirection;

    //触控操作的开始和结束位置
    public Vector2 touchBeganPos;
    public Vector2 touchEndPos;

    //控制物理计算
    const float Acceleration = 25;
    float startV = 0.0f;
    const float accLength = 0.1f;
    const float uniformLength = 0.8f;
    float displacement = 0.0f;
    public static float atomDistance;

    const float blastTime = 0.15f; // Game over 时连续爆炸的间隔时间
    const float waitTime = 0.4f;
    float currentWaitTime = 0;
    private WaitForSeconds waitBlast = new WaitForSeconds(blastTime);

    // const float speedUpTime = 10;
    // float currentSpeedUpTime = 0;

    //游戏运行状态
    bool gameGoing = false;
    public static bool gameOver = false;
    bool gameResuming = false;
    bool gamePaused = false;
    int gameStatus = 0; // 0 -> UI, 1 -> Going, 2 -> resuming
    const int STATUS_UI = 0;
    const int STATUS_GOING = 1;
    const int STATUS_RESUMING = 2;

    // bool gameSpeedUp = false;

    float timeScale = 0;
    bool doAtMoveBegin = false;

    //界面上的按钮
    public Button startButton; // 开始游戏
    public Button resumeButton; // GameOver 时出现的原地复活按钮
    public Button restartButton; // GameOver 时出现的重新开始按钮
    public Button pauseButton; //右上角的暂停按钮
    public Button settingButton; // 左上角的设置按钮

    //迟到肌肉和穿透时的提示文字
    public Text muscleText;
    public Text penetrateText;
    int muscleCount = 0;
    int penetrateCount = 0;
    public TextMesh tipText;

    //public TextAsset stage;

    void Awake()
    {
        startButton.onClick.AddListener(StartButtonEvent);
        resumeButton.onClick.AddListener(ResumeButtonEvent);
        restartButton.onClick.AddListener(RestartButtonEvent);
        pauseButton.onClick.AddListener(PauseButtonEvent);
    }

    void Start()
    {
        //Advertisement.Initialize("1179800");
        //var r = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        var seed = DateTime.Now.DayOfYear * DateTime.Now.Second;
        Util.Log(seed);

        UnityEngine.Random.InitState(seed);
        gameStatus = STATUS_UI;
        StartCoroutine(InitGame());
        gameResuming = true;
        gameGoing = true;
    }

    IEnumerator InitGame()
    {
        yield return StartCoroutine(DrawMap());
        yield return StartCoroutine(InitUI());
        yield return StartCoroutine(InitSnakeHead());
        //yield return StartCoroutine(GenSnakeBody());
        GenSnakeBody();
    }

    //初始化UI
    IEnumerator InitUI()
    {
        //startButton.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
        yield return null;

        tipText.gameObject.SetActive(false);
        muscleCount = 0;
        penetrateCount = 0;
        muscleText.text = "：" + muscleCount;
        penetrateText.text = "：" + penetrateCount;

        RenderSettings.ambientLight = Color.gray;
    }

    //初始化地图数据
    void InitMap()
    {
        //初始化地图数组
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeZ; j++)
            {
                MapDic.Add(new Vector3(i, 0, j), FLOOR);
                MapDic.Add(new Vector3(i, 1, j), BLANKSPACE);
            }
        }

        //var lnes = File.ReadAllLines("Assets/Stages/Stage_1");
        //var all = stage.text;
        //var lines = all.Split('\n');
        //MapDic.Clear();

        //foreach (var line in lines)
        //{
        //    var data = line.Split(',');
        //    var v = new Vector3();
        //    v.x = Int32.Parse(data[0]);
        //    v.y = Int32.Parse(data[1]);
        //    v.z = Int32.Parse(data[2]);
        //    MapDic.Add(v, Int32.Parse(data[3]));
        //}
    }


    // 开始游戏
    void StartButtonEvent()
    {
        StartCoroutine(StartGame());
        startButton.gameObject.SetActive(false);
    }

    IEnumerator StartGame()
    {
        yield return StartCoroutine(InitSnakeHead());
        //yield return StartCoroutine(GenSnakeBody());
        GenSnakeBody();
        gameGoing = true;
        gameStatus = STATUS_GOING;
    }

    //原地复活按钮事件
    void ResumeButtonEvent()
    {
        StopAllCoroutines();
        //const string RewardedZoneId = "1179800";
        StartCoroutine(showAD());

        //if (Advertisement.IsReady(RewardedZoneId))
        //{
        //    var options = new ShowOptions { resultCallback = HandleShowResult };
        //    Advertisement.Show(RewardedZoneId, options);
        //}
        //else if (Advertisement.IsReady())
        //{
        //    //Advertisement.Show();
        //}
        //else
        //{
        //    Debug.Log("not ready");
        //}
        //ShowRewardedAd();
        resumeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
        ResumeGame();
    }

    //重新开始按钮事件
    void RestartButtonEvent()
    {
        StopAllCoroutines();
        StartCoroutine(RestartGame());
    }

    //暂停按钮事件
    void PauseButtonEvent()
    {
        //NativeInterface.TapTicImpact();
        TapticPlugin.TapticManager.Selection();
        if (gamePaused)
        {
            Time.timeScale = timeScale;
            gamePaused = false;
            pauseButton.GetComponentInChildren<Text>().text = "||";
            BGMControl.instance.Play();
        }
        else
        {
            timeScale = Time.timeScale;
            Time.timeScale = 0;
            gamePaused = true;
            pauseButton.GetComponentInChildren<Text>().text = ">";
            BGMControl.instance.Pause();
        }
    }

    IEnumerator showAD()
    {
        //while (!Advertisement.IsReady("1179800"))
        //{
        //    Debug.Log("wait");
        //    yield return new WaitForSeconds(1);
        //}

        //Advertisement.Show("1179800");
        yield return null;
    }

    public void ShowRewardedAd()
    {
        Util.Log("ShowRewardedAd");
        //const string RewardedZoneId = "1179800";

        //if (!Advertisement.IsReady(RewardedZoneId))
        //{
        //    Debug.Log(string.Format("Ads not ready for zone '{0}'", RewardedZoneId));
        //    return;
        //}

        //var options = new ShowOptions { resultCallback = HandleShowResult };
        //Advertisement.Show(RewardedZoneId, options);
    }

    // void HandleShowResult(ShowResult result)
    //{
    //    switch (result)
    //    {
    //        case ShowResult.Finished:
    //            Debug.Log("The ad was successfully shown.");
    //            //
    //            // YOUR CODE TO REWARD THE GAMER
    //            // Give coins etc.
    //            break;
    //        case ShowResult.Skipped:
    //            Debug.Log("The ad was skipped before reaching the end.");
    //            break;
    //        case ShowResult.Failed:
    //            Debug.LogError("The ad failed to be shown.");
    //            break;
    //    }
    //}


    //绘制地图
    IEnumerator DrawMap()
    {
        InitMap();
        foreach (var key in MapDic.Keys)
        {
            if (MapDic[key] == FLOOR)
            {
                GameObject cube = Instantiate(floorCube, key, Quaternion.identity);
                yield return cube;
                CubeObjectDic.Add(key, cube);
            }
        }
    }

    //初始化蛇头，蛇头的初始移动方向为 x 轴 负方向
    IEnumerator InitSnakeHead()
    {
        GameObject obj = Instantiate(snakeHeadCube, gameStartPos, Quaternion.identity);
        yield return obj;

        snakeHead = obj.GetComponent<SnakeBody>();
        snakeHead.setPos(gameStartPos);
        snakeHead.eat = true;
        snakeHead.preDirection = Vector3.left;
        snakeHead.moveDirection = Vector3.left;
        nextDirection = Vector3.left;

        yield return null;
        headLight = obj.GetComponent<Light>();
        switchLight = myCamera.GetComponent<SwitchLight>();
        switchLight.headLight = headLight;
        switchLight.headMaterial = obj.GetComponent<Renderer>().material;
        switchLight.Init();
        switchLight.enabled = false;

        snakeBodyList.Add(snakeHead);
        //moveSpace[snakeHead.posX, snakeHead.posY, snakeHead.posZ] = SNAKE;
        //MapDic[gameStartPos] = SNAKE;
    }

    //
    void ResumeGame()
    {
        gameStatus = STATUS_RESUMING;
        gameGoing = true;
        gameResuming = true;
        gameOver = false;
        atomDistance = 0;
        //snakeHead.currentPos -= snakeHead.moveDirection;
        foreach (var body in snakeBodyList)
        {
            //body.gameObject.SetActive(true);
            body.Appear();
        }
        foreach (var body in snakeFoodDic.Values)
        {
            //body.gameObject.SetActive(true);
            body.Appear();
        }
        var blastBodys = GameObject.FindGameObjectsWithTag("BlastBody");
        //Debug.Log(blastBodys.Length);
        foreach (var blastBody in blastBodys)
        {
            Destroy(blastBody);
        }
    }

    //重新开始游戏
    IEnumerator RestartGame()
    {
        //Debug.Log("RestartGame");
        foreach (var body in snakeBodyList)
        {
            Destroy(body.gameObject);
        }
        yield return null;

        foreach (var body in snakeFoodDic.Values)
        {
            Destroy(body.gameObject);
        }
        yield return null;

        foreach (var barrier in barrierDic.Values)
        {
            Destroy(barrier.gameObject);
        }

        var blastBodys = GameObject.FindGameObjectsWithTag("BlastBody");
        //Debug.Log(blastBodys.Length);
        foreach (var blastBody in blastBodys)
        {
            Destroy(blastBody);
        }
        yield return null;

        snakeFoodDic.Clear();
        snakeBodyList.Clear();
        barrierDic.Clear();
        MapDic.Clear();

        gameGoing = false;
        gameOver = false;
        gameResuming = false;
        doAtMoveBegin = false;
        currentWaitTime = 0;
        Time.timeScale = 1.0f;

        yield return StartCoroutine(InitUI());
        InitMap();
        yield return StartCoroutine(StartGame());
    }


    void Update()
    {
        if (!gameOver)
            ProcessUserInput();
    }

    void FixedUpdate()
    {
        if (gameGoing && !gameOver && !gameResuming)
        {
            DoMove();
        }
    }

    //
    bool CheckGameOver()
    {
        //Vector3 tempNextPos = headNextPos + nextDirection;
        Vector3 headNextPos = snakeHead.currentPos + snakeHead.moveDirection;

        if(headNextPos.x > borderRight || headNextPos.x < borderLeft || headNextPos.z > borderForward || headNextPos.z < borderBack)
        {
            Util.Log("out of map");
            return true;
        }

        int value = -233;
        if (!MapDic.TryGetValue(headNextPos, out value))
        {
            Util.Log("!MapDic.TryGetValue " + headNextPos);
            return true;
        }

        foreach(var body in snakeBodyList)
        {
            if(body.currentPos == headNextPos)
            {
                value = SNAKE;
                break;
            }
        }

        switch (value)
        {
            case SNAKE:
                {
                    if (penetrateCount > 0)
                    {
                        penetrateCount--;
                        penetrateText.text = "：" + penetrateCount;
                        StartCoroutine(ShowTip(headLight.transform.position, "-1", 1));
                        return false;
                    }
                    else
                    {
                        Util.Log("Game Over, hit self");
                        return true;
                    }
                }
            case FLOOR:
                {
                    Util.Log("Game Over");
                    return true;
                }
            case BARRIER:
                {
                    if (muscleCount > 0)
                    {
                        muscleCount--;
                        muscleText.text = "：" + muscleCount;
                        StartCoroutine(ShowTip(headLight.transform.position, "-1", 0));
                        BarrierControl barrier = null;
                        barrierDic.TryGetValue(headNextPos, out barrier);
                        if (barrier != null)
                        {
                            barrier.Blast(4);
                            Destroy(barrierDic[headNextPos].gameObject);
                            barrierDic.Remove(headNextPos);
                            MapDic[headNextPos] = BLANKSPACE;
                        }
                        return false;
                    }
                    else if (penetrateCount > 0)
                    {
                        penetrateCount--;
                        penetrateText.text = "：" + penetrateCount;
                        StartCoroutine(ShowTip(headLight.transform.position, "-1", 1));
                        return false;
                    }
                    else
                    {
                        Util.Log("Game Over, hit barrier");
                        return true;
                    }
                }
        }
        return false;
    }

    // 强制结束游戏
    void ForceGameOver()
    {

    }

    //处理用户输入
    void ProcessUserInput()
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

        //手指触控
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Canceled)
            {
                Util.Log("Canceled");
            }
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                touchBeganPos = Input.GetTouch(0).position;
                //Debug.Log("began: " + touchBeganPos);
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                touchEndPos = Input.GetTouch(0).position;

                Vector3 tempDirection = snakeHead.moveDirection;
                float tan = (touchEndPos.y - touchBeganPos.y) / (touchEndPos.x - touchBeganPos.x);

                if (tan < 1.732 && tan > 0)
                {
                    tempDirection = Vector3.left;
                }
                else if (tan < 0 && tan > -1.732)
                {
                    tempDirection = Vector3.forward;
                }
                /*
                else if (tan <= -1.732)
                {
                    tempDirection = Vector3.down;
                }
                else if (tan >= 1.732)
                {
                    tempDirection = Vector3.up;
                }
                */
                if (touchEndPos.x - touchBeganPos.x < 0)
                {
                    tempDirection = -tempDirection;
                }
                
                //Debug.Log("began: " + touchBeganPos + " end: " + touchEndPos + " tan:" + tan + " d:" + tempDirection);
                TryChangeDirection(tempDirection);
            }
        }

        //鼠标的滑动操作
        if (Input.GetMouseButtonDown(0))
        {
            touchBeganPos = Input.mousePosition;
            //Debug.Log("began: " + touchBeganPos);
        }
        if (Input.GetMouseButtonUp(0))
        {
            touchEndPos = Input.mousePosition;

            Vector3 tempDirection = snakeHead.moveDirection;
            float tan = (touchEndPos.y - touchBeganPos.y) / (touchEndPos.x - touchBeganPos.x);

            if (tan <= 1.732 && tan >= 0)
            {
                tempDirection = Vector3.left;
            }
            else if (tan <= 0 && tan >= -1.732)
            {
                tempDirection = Vector3.forward;
            }
            else if (tan <= -1.732)
            {
                tempDirection = Vector3.down;
            }
            else if (tan >= 1.732)
            {
                tempDirection = Vector3.up;
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
            TryChangeDirection(Vector3.up);
        }
        else if (Input.GetKey("k") || Input.GetKey("s"))
        {
            TryChangeDirection(Vector3.down);
        }
        else if (Input.GetKey("j") || Input.GetKey("a"))
        {
            TryChangeDirection(Vector3.back);
        }
        else if (Input.GetKey("l") || Input.GetKey("d"))
        {
            TryChangeDirection(Vector3.forward);
        }
        else if (Input.GetKey("u") || Input.GetKey("q"))
        {
            TryChangeDirection(Vector3.right);
        }
        else if (Input.GetKey("o") || Input.GetKey("e"))
        {
            TryChangeDirection(Vector3.left);
        }
    }


    void TryChangeDirection(Vector3 direction)
    {
        //Debug.Log("TryChangeDirection " + direction);

        Vector3 headNextPos = snakeHead.currentPos + snakeHead.moveDirection;
        if (gameResuming)
        {
            headNextPos = snakeHead.currentPos;
        }
        //int x = (int)headNextPos.x;
        //int y = (int)headNextPos.y;
        //int z = (int)headNextPos.z;
        //Debug.Log(headNextPos);

        bool changed = false;
        var nNext = headNextPos + direction;
        if (!MapDic.ContainsKey(nNext))
        {
            Util.Log("invalied direction");
            return;
        }

        int value = MapDic[nNext];
        foreach(var body in snakeBodyList)
            if(body.currentPos == nNext)
        {
                value = SNAKE;
                break;
        }
        if (checkMove(value))
        {
            nextDirection = direction;
            changed = true;
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

    bool checkMove(int spaceValue)
    {
        bool flag = false;
        switch (spaceValue)
        {
            case BARRIER:
                {
                    if (muscleCount > 0)
                    {
                        flag = true;
                    }
                    break;
                }
            case FLOOR:
                {
                    flag = false;
                    break;
                }
            case SNAKE:
                {
                    if (penetrateCount > 0)
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
            case FOOD:
                {
                    flag = true;
                    break;
                }
        }
        return flag;
    }

    //死亡爆炸
    IEnumerator BlastAllBodyFood()
    {
        foreach (var body in snakeBodyList)
        {
            body.Blast(3);
            body.gameObject.SetActive(false);
            yield return waitBlast;
        }
        foreach (var body in snakeFoodDic.Values)
        {
            body.Blast(3);
            body.gameObject.SetActive(false);
            yield return waitBlast;
        }
    }

    void ProcessGameOver()
    {
        StartCoroutine(BlastAllBodyFood());

        gameOver = true;
        //headNextPos -= nextDirection;
        atomDistance = 0;
        pauseButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(true);

        //if (Advertisement.IsReady())
        {
            resumeButton.gameObject.SetActive(true);
            restartButton.GetComponent<RectTransform>().localPosition = new Vector3(0, -100, 0);
            //restartButton.GetComponentInChildren<Text>().text = "我才不看广告呢╭(╯^╰)╮";
        }
        //else
        //{
        //    restartButton.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        //    restartButton.GetComponentInChildren<Text>().text = "重新开始/(ㄒoㄒ)/~~";
        //}
        //Time.timeScale = 0;

    }

    //在运动开始时判断是否吃到或者死掉
    void DoAtMoveBegin()
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
        AudioControl.instance.playMoveSound();
    }

    void DoMove()
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

    void ProcessEat()
    {
        Vector3 headNextPos = snakeHead.currentPos + snakeHead.moveDirection;
        //如果吃到
        //if (moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] == FOOD)
        if (!MapDic.ContainsKey(headNextPos))
        {
            Util.LogError("!MapDic.ContainsKey " + headNextPos);
            return;
        }

        if (MapDic[headNextPos] == FOOD)
        {
            SnakeBody bodyCube = null;
            snakeFoodDic.TryGetValue(headNextPos, out bodyCube);
            if (bodyCube != null)
            {
                bodyCube.Blast(4);
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

                //moveSpace[bodyCube.posX, bodyCube.posY, bodyCube.posZ] = SNAKE;
                //MapDic[bodyCube.currentPos] = SNAKE;
                bodyCube.eat = true;

                switch (bodyCube.cubeType)
                {
                    case CubeType.DoubleCube:
                        {
                            AudioControl.instance.playDoubleSound();
                            GenSnakeBody();
                            break;
                        }
                    case CubeType.FlashCube:
                        {
                            AudioControl.instance.playFlashSound();
                            Time.timeScale++;
                            break;
                        }
                    case CubeType.LightOffCube:
                        {
                            AudioControl.instance.playLightSound();
                            ProcessEatLight(false);
                            GenSnakeBody(CubeType.LightOnCube);
                            break;
                        }
                    case CubeType.LightOnCube:
                        {
                            AudioControl.instance.playLightSound();
                            ProcessEatLight(true);
                            break;
                        }
                    case CubeType.MuscleCube:
                        {
                            AudioControl.instance.playMuscleSound();
                            ProcessEatMuscle();
                            break;
                        }
                    case CubeType.PenetrateCube:
                        {
                            AudioControl.instance.playPenetrateSound();
                            ProcessEatPenetrate();
                            break;
                        }
                    case CubeType.SwitchCube:
                        {
                            ProcessEatSwitch();
                            break;
                        }
                    case CubeType.TrapCube:
                        {
                            AudioControl.instance.playTrapSound();
                            ProcessEatTrap();
                            GenSnakeBody(CubeType.SwitchCube);
                            break;
                        }
                    default:
                        AudioControl.instance.playNormalSound();
                        break;
                }

                GenSnakeBody();

                if (bodyCube.cubeType != CubeType.FlashCube && Time.timeScale > 1)
                {
                    Time.timeScale--;
                }

                snakeBodyList.Add(bodyCube);
                snakeFoodDic.Remove(headNextPos);
                MapDic[headNextPos] = BLANKSPACE;
            }
            //moveSpace[(int)headNextPos.x, (int)headNextPos.y, (int)headNextPos.z] = BLANKSPACE;

        }
    }

    void ProcessDirectionChange()
    {
        if (!gameResuming)
        {
            //处理尾部
            SnakeBody lastBody = snakeBodyList[snakeBodyList.Count - 1];
            //moveSpace[lastBody.posX, lastBody.posY, lastBody.posZ] = BLANKSPACE;
            //MapDic[lastBody.currentPos] = BLANKSPACE;
            //处理头部
            snakeHead.moveOneStep();
            //moveSpace[snakeHead.posX, snakeHead.posY, snakeHead.posZ] = SNAKE;
            //MapDic[snakeHead.currentPos] = SNAKE;

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

    Vector3[] getVariousBlankSpace(int num)
    {
        var results = new Vector3[num];
        for(int i = 0; i < num; i ++)
        {

        }
        return results;
    }

    // 在地图上找到一处空白
    Vector3 getOneBlankspace()
    {
        var newPos = Vector3.one;
        int posX = Mathf.CeilToInt(UnityEngine.Random.value * (mapSizeX - 1)) + borderLeft;
        int posZ = Mathf.CeilToInt(UnityEngine.Random.value * (mapSizeZ - 1)) + borderBack;

        int count = 0;
        int count1 = 0, count2 = 0;
        foreach (var v in MapDic.Values)
        {
            if (v == BLANKSPACE)
                count++;
            if (v == BARRIER)
                count1++;
            if (v == FOOD)
                count2++;
        }
            
        Util.Log("Blankspace: " + count + ", food: " + snakeFoodDic.Count + ", snake: " + snakeBodyList.Count + ", barrier: " + barrierDic.Count);
        Util.Log("Blankspace: " + count + ", food: " + count2 + ", snake: " + snakeBodyList.Count + ", barrier: " + count1);

        int c = 0;
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeZ; j++)
            {
                newPos.x = posX;
                newPos.z = posZ;
                //Debug.Log(newPos);
                if (!MapDic.Keys.Contains(newPos))
                {
                    Util.LogError("wtf:" + newPos);
                    continue;
                }
                bool isBody = false;
                foreach (var body in snakeBodyList)
                    if (body.currentPos == newPos)
                {
                        isBody = true;
                        break;
                }
                
                if (!isBody && MapDic[newPos] == BLANKSPACE)
                    return newPos;
                
                posZ = (posZ == borderForward) ? borderBack : posZ + 1;
                c++;
            }
            posX = (posX == borderRight) ? borderLeft : posX + 1;
        }

        Util.LogError("getOneBlankspace " + newPos + " count: " + c);
        return Vector3.zero;
    }

    //生成新的块
    void GenSnakeBody(CubeType type = CubeType.CommonCube)
    {
        Vector3 newPos = getOneBlankspace();
        MapDic[newPos] = FOOD;

        int randomType = Mathf.CeilToInt(UnityEngine.Random.value * 100);

        if (randomType <= 5) //障碍
            randomType = 10;
        else if (randomType <= 10) //关灯
            randomType = 8;
        else if (randomType <= 16) //加速
            randomType = 3;
        else if (randomType <= 22) //switch
            randomType = 5;
        else if (randomType <= 27) //肌肉
            randomType = 6;
        else if (randomType <= 34) //穿透
            randomType = 7;
        else if (randomType <= 41) //double
            randomType = 4;
        else if (randomType <= 60) //普通
            randomType = 1;
        else if (randomType <= 70) //普通
            randomType = 2;
        else if (randomType <= 80) //普通
            randomType = -1;
        else                                    //普通
            randomType = 0;

        if (type != CubeType.CommonCube)
            randomType = (int)type;
        
        if (type == CubeType.LightOnCube)//要求产生开灯块
        {
            randomType = 9;
        }
        else if (randomType == 9)//否则如果随机到开灯块则变为关灯块
        {
            randomType = 8;
        }
        //randomType = 5;
        
        StartCoroutine(GenSnakeBodyAnim(newPos, randomType));
    }

    IEnumerator GenSnakeBodyAnim(Vector3 newPos, int randomType)
    {
        var newBodyCube = Instantiate(snakeBodyCube, newPos, Quaternion.identity);
        yield return newBodyCube;
        //CubeObjectDic.Add(newPos, newBodyCube);

        SnakeBody body = newBodyCube.GetComponent<SnakeBody>();
        body.setPos(newPos);
        snakeFoodDic.Add(newPos, body);

        if (snakeBodyList.Count > 0)
        {
            switch (randomType)
            {
                case -1:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = bodyMaterial;
                        break;
                    }
                case 0:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = bodyMaterial2;
                        break;
                    }
                case 1:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = bodyMaterial3;
                        break;
                    }
                case 2:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = bodyMaterial4;
                        break;
                    }
                case 3:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = flashMaterial;
                        body.cubeType = CubeType.FlashCube;
                        break;
                    }
                case 4:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = doubleMaterial;
                        body.cubeType = CubeType.DoubleCube;
                        break;
                    }
                case 5:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = switchMaterial;
                        body.cubeType = CubeType.SwitchCube;
                        break;
                    }
                case 6:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = muscleMaterial;
                        body.cubeType = CubeType.MuscleCube;
                        break;
                    }
                case 7:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = penetrateMaterial;
                        body.cubeType = CubeType.PenetrateCube;
                        break;
                    }
                case 8:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = lightOffMaterial;
                        body.cubeType = CubeType.LightOffCube;
                        break;
                    }
                case 9:
                    {
                        yield return newBodyCube.GetComponent<Renderer>().material = lightOnMaterial;
                        yield return newBodyCube.AddComponent<LightFlash>();
                        body.cubeType = CubeType.LightOnCube;
                        break;
                    }
                case 10:
                    yield return newBodyCube.GetComponent<Renderer>().material = trapMaterial;
                    body.cubeType = CubeType.TrapCube;
                    break;
            }
        }
    }
    
    //吃到机关块
    void ProcessEatSwitch()
    {
        var removePos = Vector3.zero;
        var deltaPos = Vector3.zero;
        int deltaX = (int)snakeHead.moveDirection.x;
        int deltaZ = (int)snakeHead.moveDirection.z;

        if (snakeHead.moveDirection == Vector3.left)
        {
            removePos = new Vector3(borderRight, 0, borderBack);
            deltaPos = Vector3.forward;
        }
        else if (snakeHead.moveDirection == Vector3.right)
        {
            removePos = new Vector3(borderLeft, 0, borderForward);
            deltaPos = Vector3.back;
        }
        else if (snakeHead.moveDirection == Vector3.forward)
        {
            removePos = new Vector3(borderLeft, 0, borderBack);
            deltaPos = Vector3.right;
        }
        else if (snakeHead.moveDirection == Vector3.back)
        {
            removePos = new Vector3(borderRight, 0, borderForward);
            deltaPos = Vector3.left;
        }
        else
        {
            Util.LogError("wtf");
        }

        var newPos = removePos + snakeHead.moveDirection * mapSizeX;
        StartCoroutine(extendMapAnim(newPos, removePos, deltaPos));
        
        for (int i = 0; i < mapSizeZ; i++)
        {
            if (!MapDic.ContainsKey(newPos))
                MapDic.Add(newPos, FLOOR);
            if (!MapDic.ContainsKey(newPos + Vector3.up))
                MapDic.Add(newPos + Vector3.up, BLANKSPACE);
            if(MapDic.ContainsKey(removePos))
                MapDic.Remove(removePos);
            if (MapDic.ContainsKey(removePos + Vector3.up))
                MapDic.Remove(removePos + Vector3.up);

            newPos += deltaPos;
            removePos += deltaPos;
        }

        borderLeft += deltaX;
        borderRight += deltaX;
        borderForward += deltaZ;
        borderBack += deltaZ;
    }

    void CheckMapDic(int index)
    {
        if (MapDic.Count > 200)
        {
            Util.LogError("error " + index + ", " + MapDic.Count + " " + borderLeft + " " + borderRight + " " + borderForward + " " + borderBack);
        }
    }

    IEnumerator extendMapAnim(Vector3 newPos, Vector3 removePos, Vector3 deltaPos)
    {
        GameObject cube;
        WaitForSeconds wait = new WaitForSeconds(Time.timeScale / mapSizeZ / 2);
        for (int i = 0; i < mapSizeZ; i++)
        {
            var newCube = Instantiate(floorCube, newPos, Quaternion.identity);
            yield return newCube;
            CubeObjectDic.Add(newPos, newCube);

            cube = null;
            CubeObjectDic.TryGetValue(removePos, out cube);
            if (cube != null)
            {
                cube.GetComponent<SnakeBody>().Blast(3);
                //cube.SetActive(false);
                Destroy(cube);
            }
            else
                Util.Log("wtf: " + removePos);

            var removeUp = removePos + Vector3.up;
            if (barrierDic.ContainsKey(removeUp))
            {
                barrierDic[removeUp].Blast();
                Destroy(barrierDic[removeUp].gameObject);
                barrierDic.Remove(removeUp);
            }

            if (snakeFoodDic.ContainsKey(removeUp))
            {
                Util.Log("remove food at " + removeUp);
                snakeFoodDic[removeUp].Blast();
                Destroy(snakeFoodDic[removeUp].gameObject);
                snakeFoodDic.Remove(removeUp);
            }

            CubeObjectDic.Remove(removePos);
            newPos += deltaPos;
            removePos += deltaPos;
            yield return wait;
        }
    }

    //陷阱逻辑
    void ProcessEatTrap(int barrierNum = BARRIER_NUM)
    {
        Vector3[] newBarriers = new Vector3[barrierNum];
        for (int i = 0; i < barrierNum; i++)
        {
            var pos = getOneBlankspace();
            MapDic[pos] = BARRIER;
            newBarriers[i] = pos;
        }
        StartCoroutine(TrapAnim(newBarriers));
    }
    //陷阱动画
    IEnumerator TrapAnim(Vector3[] newBarriers)
    {
        for (int i = 0; i < newBarriers.Length; i++)
        {
            if (barrierDic.Keys.Contains(newBarriers[i]))
                continue;
            var barrier = Instantiate(barrierCube).GetComponent<BarrierControl>();
            yield return barrier;
            barrier.setPos(newBarriers[i]);
            barrierDic.Add(newBarriers[i], barrier);
        }
    }

    //吃到灯光开关
    void ProcessEatLight(bool offon)
    {
        if (!offon)
        {
            switchLight.enabled = true;
            switchLight.turnOff();
            muscleText.color = new Color(0.8f, 0.8f, 0.8f);
            penetrateText.color = new Color(0.8f, 0.8f, 0.8f);
        }
        else
        {
            switchLight.enabled = true;
            switchLight.turnOn();
            muscleText.color = new Color(0.2f, 0.2f, 0.2f);
            penetrateText.color = new Color(0.2f, 0.2f, 0.2f);
        }
    }

    //吃到肌肉块
    void ProcessEatMuscle()
    {
        muscleCount++;
        muscleText.text = "：" + muscleCount;
        //ShowTip(headLight.transform.position, "+1", 0);
        StartCoroutine(ShowTip(headLight.transform.position, "+1", 0));
    }

    //吃到穿透块
    void ProcessEatPenetrate()
    {
        penetrateCount++;
        penetrateText.text = "：" + penetrateCount;
        //ShowTip(headLight.transform.position, "+1", 1);
        StartCoroutine(ShowTip(headLight.transform.position, "+1", 1));
    }

    void ProcessEatChangeMap()
    {

    }

    IEnumerator ShowTip(Vector3 pos, string tip, int index)
    {
        //Debug.Log(pos);
        //tipText.gameObject.transform.position = pos;
        tipText.GetComponent<ShowTip>().Show(new Vector3(pos.x + 1, pos.y + 1, pos.z + 1), tip, index);
        yield return null;
    }

    void printState()
    {
        //Debug.Log("----------");
        //int snakeCount = 0;
        //for (int i = 0; i < 10; i++)
        //{
        //    for (int j = 0; j < 10; j++)
        //    {
        //        for (int k = 0; k < 10; k++)
        //        {
        //            if (i == 0 || j == 0 || k == 0)
        //            {
        //                if (moveSpace[i, j, k] == SNAKE)
        //                {
        //                    snakeCount++;
        //                }
        //            }
        //        }
        //    }
        //}
        //Debug.Log("snakeCount " + snakeCount);
        //Debug.Log("snakeBodyList.Count " + snakeBodyList.Count);
        //foreach (SnakeBody body in snakeBodyList)
        //{
        //    Debug.Log("body instancePos " + body.instantPos);
        //    Debug.Log("body moveD " + body.moveDirection);
        //    Debug.Log("body preD " + body.preDirection);
        //}
    }

    void checkSnake()
    {
        //Debug.Log("-----");
        //for (int i = 0; i < 10; i++)
        //{
        //    for (int j = 0; j < 10; j++)
        //    {
        //        for (int k = 0; k < 10; k++)
        //        {
        //            if (moveSpace[i, j, k] == SNAKE)
        //            {
        //                //Debug.Log("snake: " + i + " " + j + " " + k);
        //            }
        //        }
        //    }
        //}
    }

}
