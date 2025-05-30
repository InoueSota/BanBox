using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // My Component
    private InputManager inputManager;

    void Start()
    {
        // Set Component
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        // 入力状況を取得する
        inputManager.GetAllInput();

        // 完全リセット
        if (inputManager.IsTrgger(inputManager.reset)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }

    void LateUpdate()
    {
        // 入力状況をリセットする
        inputManager.SetIsGetInput();
    }
}
