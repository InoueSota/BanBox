using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // My Component
    private InputManager inputManager;

    [Header("�J�ڐ�V�[����")]
    [SerializeField] private string nextStageName;

    void Start()
    {
        // Set Component
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        // ���͏󋵂��擾����
        inputManager.GetAllInput();

        // �J�ڐ�V�[���ɑJ�ڂ���
        if (inputManager.IsTrgger(inputManager.jump)) { SceneManager.LoadScene(nextStageName); }

        // ���S���Z�b�g
        if (inputManager.IsTrgger(inputManager.reset)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }

    void LateUpdate()
    {
        // ���͏󋵂����Z�b�g����
        inputManager.SetIsGetInput();
    }
}
