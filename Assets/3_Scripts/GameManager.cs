using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // My Component
    private InputManager inputManager;

    [Header("Clear")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float changeSceneIntervalTime;
    private float changeSceneIntervalTimer;
    private bool isClear;

    [Header("UI")]
    [SerializeField] private GameObject clearTextObj;

    void Start()
    {
        // Set Component
        inputManager = GetComponent<InputManager>();

        // Variables - Initialize
        changeSceneIntervalTimer = changeSceneIntervalTime;
    }

    void Update()
    {
        // 入力状況を取得する
        inputManager.GetAllInput();

        Clear();

        // 完全リセット
        if (inputManager.IsTrgger(inputManager.reset)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }
    void LateUpdate()
    {
        // 入力状況をリセットする
        inputManager.SetIsGetInput();
    }

    void Clear()
    {
        if (!isClear)
        {
            bool noBox = true;

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX) { noBox = false; }
            }

            if (noBox) { clearTextObj.SetActive(true); isClear = true; }
        }
        else
        {
            changeSceneIntervalTimer -= Time.deltaTime;
            if (changeSceneIntervalTimer <= 0f) { SceneManager.LoadScene(nextSceneName); }
        }
    }
}
