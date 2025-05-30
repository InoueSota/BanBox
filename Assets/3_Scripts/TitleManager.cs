using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // My Component
    private InputManager inputManager;

    [Header("遷移先シーン名")]
    [SerializeField] private string nextStageName;

    void Start()
    {
        // Set Component
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        // 入力状況を取得する
        inputManager.GetAllInput();

        // 遷移先シーンに遷移する
        if (inputManager.IsTrgger(inputManager.jump)) { SceneManager.LoadScene(nextStageName); }

        // 完全リセット
        if (inputManager.IsTrgger(inputManager.reset)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }

    void LateUpdate()
    {
        // 入力状況をリセットする
        inputManager.SetIsGetInput();
    }
}
