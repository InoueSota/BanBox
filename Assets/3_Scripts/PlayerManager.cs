using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // 自コンポーネント取得
    private PlayerMoveManager moveManager;
    private PlayerHitManager hitManager;

    // Other Objects
    [Header("Other Objects")]
    [SerializeField] private GameObject gameManagerObj;
    private InputManager inputManager;

    // 移動フラグ
    private bool isActive;
    private bool canGetInput;
    private bool canJump = true;

    void Start()
    {
        // Set Component
        moveManager = GetComponent<PlayerMoveManager>();
        hitManager = GetComponent<PlayerHitManager>();

        // Set Component - Other
        inputManager = gameManagerObj.GetComponent<InputManager>();
        
        canGetInput = true;
        isActive = true;
        moveManager.Initialize();
    }

    void Update()
    {
        inputManager.GetAllInput();

        moveManager.ManualUpdate();
    }

    // Setter
    public void SetIsActive(bool _isActive)
    {
        isActive = _isActive;
    }
    public void SetCanGetInput(bool _canGetInput)
    {
        canGetInput = _canGetInput;
    }
    public void SetCanJump(bool _canJump)
    {
        canJump = _canJump;
    }
    public void Initialize()
    {
        moveManager.Initialize();
    }

    // Getter
    public InputManager GetInputManager()
    {
        return inputManager;
    }
    public bool GetIsActive()
    {
        return isActive;
    }
    public bool GetCanGetInput()
    {
        return canGetInput;
    }
    public bool GetCanJump()
    {
        return canJump;
    }
}
