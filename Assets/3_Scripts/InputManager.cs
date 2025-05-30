using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Šî–{ƒNƒ‰ƒX
    public class InputPattern
    {
        public float input = 0f;
        public float preInput = 0f;

        private bool isGetInput = false;

        public void GetInput(string _inputName)
        {
            if (!isGetInput)
            {
                preInput = input;
                input = Input.GetAxisRaw(_inputName);

                isGetInput = true;
            }
        }
        public void SetIsGetInput(bool _isGetInput)
        {
            isGetInput = _isGetInput;
        }
    }

    // “ü—Í‚ÌŽí—Þ
    public InputPattern horizontal;
    public InputPattern vertical;
    public InputPattern jump;
    public InputPattern reset;
    public InputPattern cancel;

    void Start()
    {
        horizontal = new InputPattern();
        vertical = new InputPattern();
        jump = new InputPattern();
        reset = new InputPattern();
        cancel = new InputPattern();
    }

    public void SetIsGetInput()
    {
        horizontal.SetIsGetInput(false);
        vertical.SetIsGetInput(false);
        jump.SetIsGetInput(false);
        reset.SetIsGetInput(false);
        cancel.SetIsGetInput(false);
    }

    public void GetAllInput()
    {
        horizontal.GetInput("Horizontal");
        vertical.GetInput("Vertical");
        jump.GetInput("Jump");
        reset.GetInput("Reset");
        cancel.GetInput("Cancel");
    }

    public bool IsTrgger(InputPattern _inputPattern)
    {
        if (_inputPattern.input != 0f && _inputPattern.preInput == 0f)
        {
            return true;
        }
        return false;
    }

    public bool IsPush(InputPattern _inputPattern)
    {
        if (_inputPattern.input != 0f && _inputPattern.preInput != 0f)
        {
            return true;
        }
        return false;
    }

    public bool IsRelease(InputPattern _inputPattern)
    {
        if (_inputPattern.input == 0f && _inputPattern.preInput != 0f)
        {
            return true;
        }
        return false;
    }

    public float ReturnInputValue(InputPattern _inputPattern)
    {
        return _inputPattern.input;
    }
}
