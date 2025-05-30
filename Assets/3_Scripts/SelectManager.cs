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

    [Header("�X�e�[�W�ő吔")]
    [SerializeField] private int stageMax;

    [Header("��������X�e�[�W�ԍ��v���n�u")]
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private GameObject stageNumberPrefab;

    [Header("�X�e�[�W�ԍ��̊Ԋu")]
    [SerializeField] private float stageNumberDistance;

    // Variables - Stage Number
    private int stageNumber;
    private string[] stageName;
    private Transform[] stageNumberObjTransform;

    [Header("�X�e�[�W�I���Ԋu�̎���")]
    [SerializeField] private float selectIntervalTime;
    private float selectIntervalTimer;

    [Header("�J����")]
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
            // �X�e�[�W�ԍ��𐶐�
            GameObject stageNumberObj = Instantiate(stageNumberPrefab, new Vector3((i - 1) * stageNumberDistance, 0f, 0f), Quaternion.identity);
            stageNumberObj.transform.SetParent(worldCanvas.transform);
            stageNumberObjTransform[i - 1] = stageNumberObj.transform;

            // �X�e�[�W���擾
            stageName[i - 1] = "Stage" + i.ToString();
            stageNumberObj.transform.GetChild(1).GetComponent<Text>().text = i.ToString();
        }
    }

    void Update()
    {
        // ���͏󋵂��擾����
        inputManager.GetAllInput();
        GetInput();

        ChangeSelectStage();
        ChangeScene();

        // ���S���Z�b�g
        if (inputManager.IsTrgger(inputManager.reset)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }
    void LateUpdate()
    {
        // ���͏󋵂����Z�b�g����
        inputManager.SetIsGetInput();
    }

    void ChangeSelectStage()
    {
        selectIntervalTimer -= Time.deltaTime;

        if (selectIntervalTimer <= 0f && (isPushLeft || isPushRight))
        {
            // �X�e�[�W�ԍ������Z����
            if (isPushLeft)
            {
                // ���łɍŏ��ԍ���I�����Ă�����A�ő�ԍ��ɂ���
                if (stageNumber == 0) { stageNumber = stageMax - 1; }
                else { stageNumber--; }
            }
            // �X�e�[�W�ԍ������Z����
            else if (isPushRight)
            {
                // ���łɍő�ԍ���I�����Ă�����A�ŏ��ԍ��ɂ���
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
