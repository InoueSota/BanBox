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

        // R�L�[�𐄂����ƂŊ��S���Z�b�g���s��
        if (Input.GetKey(KeyCode.R)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }

    void LateUpdate()
    {
        // ���͏󋵂����Z�b�g����
        inputManager.SetIsGetInput();
    }
}
