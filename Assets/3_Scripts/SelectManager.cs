using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    // Input
    private InputManager inputManager;
    private bool isTriggerJump;
    private bool isTriggerCancel;
    private bool isPushLeft;
    private bool isPushRight;

    [Header("ステージ最大数")]
    [SerializeField] private int stageMax;

    [Header("生成するステージ番号プレハブ")]
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private GameObject stageNumberPrefab;

    [Header("ステージ番号の間隔")]
    [SerializeField] private float stageNumberDistance;

    // Variables - Stage Number
    private int stageNumber;
    private string[] stageName;
    private Transform[] stageNumberObjTransform;

    [Header("ステージ選択間隔の時間")]
    [SerializeField] private float selectIntervalTime;
    private float selectIntervalTimer;

    [Header("カメラ")]
    [SerializeField] private SelectCameraManager selectCameraManager;

    void Start()
    {
        // Set Component
        inputManager = GetComponent<InputManager>();

        // Variables - Initialize
        stageName = new string[stageMax];
        stageNumberObjTransform = new Transform[stageMax];

        for (int i = 1; i < stageMax + 1; i++)
        {
            // ステージ番号を生成
            GameObject stageNumberObj = Instantiate(stageNumberPrefab, new Vector3((i - 1) * stageNumberDistance, 0f, 0f), Quaternion.identity);
            stageNumberObj.transform.SetParent(worldCanvas.transform);
            stageNumberObjTransform[i - 1] = stageNumberObj.transform;

            // ステージ名取得
            stageName[i - 1] = "Stage" + i.ToString();
            stageNumberObj.transform.GetChild(1).GetComponent<Text>().text = i.ToString();
        }
    }

    void Update()
    {
        // 入力状況を取得する
        inputManager.GetAllInput();
        GetInput();

        ChangeSelectStage();
        ChangeScene();

        // 完全リセット
        if (inputManager.IsTrgger(inputManager.reset)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }
    void LateUpdate()
    {
        // 入力状況をリセットする
        inputManager.SetIsGetInput();
    }

    void ChangeSelectStage()
    {
        selectIntervalTimer -= Time.deltaTime;

        if (selectIntervalTimer <= 0f && (isPushLeft || isPushRight))
        {
            // ステージ番号を減算する
            if (isPushLeft)
            {
                // すでに最小番号を選択していたら、最大番号にする
                if (stageNumber == 0) { stageNumber = stageMax - 1; }
                else { stageNumber--; }
            }
            // ステージ番号を加算する
            else if (isPushRight)
            {
                // すでに最大番号を選択していたら、最小番号にする
                if (stageNumber == stageMax - 1) { stageNumber = 0; }
                else { stageNumber++; }
            }
            selectCameraManager.SetTargetPosition(stageNumberObjTransform[stageNumber].position.x);
            selectIntervalTimer = selectIntervalTime;
        }
    }
    void ChangeScene()
    {
        if (isTriggerJump) { SceneManager.LoadScene(stageName[stageNumber]); }
        if (isTriggerCancel) { SceneManager.LoadScene("TitleScene"); }
    }

    // Getter
    void GetInput()
    {
        isTriggerJump = false;
        isTriggerCancel = false;
        isPushLeft = false;
        isPushRight = false;

        if (inputManager.IsTrgger(inputManager.jump)) { isTriggerJump = true; }
        if (inputManager.IsTrgger(inputManager.cancel)) { isTriggerCancel = true; }
        if (inputManager.IsPush(inputManager.horizontal))
        {
            if (inputManager.ReturnInputValue(inputManager.horizontal) < 0f) { isPushLeft = true; }
            else { isPushRight = true; }
        }
    }
}
