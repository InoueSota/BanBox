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
        // ���͏󋵂��擾����
        inputManager.GetAllInput();

        // ���S���Z�b�g
        if (inputManager.IsTrgger(inputManager.reset)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }

    void LateUpdate()
    {
        // ���͏󋵂����Z�b�g����
        inputManager.SetIsGetInput();
    }
}
