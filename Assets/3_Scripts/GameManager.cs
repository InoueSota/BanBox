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

        // Rキーを推すことで完全リセットを行う
        if (Input.GetKey(KeyCode.R)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }

    void LateUpdate()
    {
        // 入力状況をリセットする
        inputManager.SetIsGetInput();
    }
}
