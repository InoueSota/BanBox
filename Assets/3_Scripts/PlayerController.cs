using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ���R���|�[�l���g�擾
    private PlayerManager manager;
    private bool isPushLeft;
    private bool isPushRight;
    private bool isTriggerJump;

    // ��{���
    private Vector2 halfSize;
    private Vector2 cameraHalfSize;

    // ���W�n
    private Vector3 originPosition;
    private Vector3 nextPosition;

    [Header("���ړ�")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private float accelerationTime;
    private float moveSpeed;
    private float acceleration;
    private float accelerationTimer;
    private bool canAcceleration;
    private Vector3 moveDirection;

    [Header("�W�����v")]
    [SerializeField] private float jumpDistance;
    [SerializeField] private float jumpSpeed;
    private bool isJumping;
    private float jumpTarget;

    [Header("�؋�")]
    [SerializeField] private float hangTime;
    [SerializeField] private float hitHeadHangTime;
    private bool isHovering;
    private float hangTimer;

    [Header("�d��")]
    [SerializeField] private float gravityMax;
    [SerializeField] private float addGravity;
    private bool isGravity;
    private float gravityPower;

    // ����������
    private Vector3 dropInitialTarget;
    private bool isDropInitialize;

    [Header("����")]
    [SerializeField] private float dropAmount;
    [SerializeField] private float breakDistance;
    private Vector3 dropTarget;
    private Vector3 dropDirection;
    private bool canBreakBox;
    private bool isDropping;

    void Start()
    {
        manager = GetComponent<PlayerManager>();

        halfSize.x = transform.localScale.x * 0.5f;
        halfSize.y = transform.localScale.y * 0.5f;
        cameraHalfSize.x = Camera.main.ScreenToWorldPoint(new(Screen.width, 0f, 0f)).x;
        cameraHalfSize.y = Camera.main.ScreenToWorldPoint(new(0f, Screen.height, 0f)).y;
        originPosition = transform.position;

        isDropping = false;
    }
    public void Initialize()
    {
        transform.position = originPosition;
        isJumping = false;
        isHovering = false;
        isGravity = false;
        isDropping = false;
        transform.DOKill();
    }

    public void ManualUpdate()
    {
        if (manager.GetIsActive())
        {
            isPushLeft = false;
            isPushRight = false;
            isTriggerJump = false;

            if (manager.GetCanGetInput())
            {
                GetInput();
            }

            nextPosition = transform.position;

            Move();
            DropInitialize();
            Dropping();
            Jump();
            Hovering();
            Gravity();

            ClampInCamera();

            transform.position = nextPosition;
        }
    }

    void CheckDirection()
    {
        if (!isDropping && !isDropInitialize && (isPushLeft || isPushRight))
        {
            moveSpeed = maxSpeed + acceleration;

            if (isPushLeft)
            {
                moveDirection = Vector3.left;
            }
            else if (isPushRight)
            {
                moveDirection = Vector3.right;
            }
        }
        else
        {
            acceleration = 0f;
            canAcceleration = false;
            moveSpeed = 0f;
        }
    }
    void Acceleration()
    {
        if (!canAcceleration && GetIsGround())
        {
            accelerationTimer = accelerationTime;
            canAcceleration = true;
        }
        else if (canAcceleration && !GetIsGround())
        {
            acceleration = 0f;
            canAcceleration = false;
        }

        if (canAcceleration)
        {
            accelerationTimer -= Time.deltaTime;
            accelerationTimer = Mathf.Clamp(accelerationTimer, 0f, accelerationTime);
            acceleration = Mathf.Lerp(maxAcceleration, 0f, accelerationTimer / accelerationTime);
        }
    }
    void Move()
    {
        // �ړ������̏C��
        CheckDirection();

        // ����
        Acceleration();

        // �ړ�
        float deltaMoveSpeed = moveSpeed * Time.deltaTime;
        nextPosition += deltaMoveSpeed * moveDirection;

        // �u���b�N�Ƃ̏Փ˔���
        if (isPushLeft || isPushRight)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X������
                float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.5f;

                // Y������
                float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
                float yDoubleSize = halfSize.y + 0.25f;

                if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
                {
                    if (!isJumping)
                    {
                        yDoubleSize = halfSize.y + 0.15f;
                    }

                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        if (nextPosition.x > obj.transform.position.x)
                        {
                            nextPosition.x = obj.transform.position.x + 0.5f + halfSize.x;
                            break;
                        }
                        else
                        {
                            nextPosition.x = obj.transform.position.x - 0.5f - halfSize.x;
                            break;
                        }
                    }
                }
            }
        }
    }
    void Jump()
    {
        // �W�����v�J�n�Ə�����
        if (manager.GetCanJump() && !isDropping && !isDropInitialize && !isJumping && !isHovering && !isGravity && isTriggerJump)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                if (nextPosition.y > obj.transform.position.y)
                {
                    // X������
                    float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.5f;

                    // Y������
                    float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
                    float yDoubleSize = halfSize.y + 0.51f;

                    if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
                    {
                        jumpTarget = nextPosition.y + jumpDistance;
                        isJumping = true;
                        break;
                    }
                }
            }
        }

        // �W�����v����
        if (isJumping)
        {
            float deltaJumpSpeed = jumpSpeed * Time.deltaTime;
            nextPosition.y += (jumpTarget - nextPosition.y) * deltaJumpSpeed;

            // �W�����v�I������
            if (Mathf.Abs(jumpTarget - nextPosition.y) < 0.03f)
            {
                nextPosition.y = jumpTarget;

                hangTimer = hangTime;
                isHovering = true;
                isJumping = false;
            }

            // �u���b�N�Ƃ̏Փ˔���
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
                {
                    // X������
                    float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.26f;

                    // Y������
                    float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
                    float yDoubleSize = halfSize.y + 0.5f;

                    if (xBetween < xDoubleSize && yBetween < yDoubleSize)
                    {
                        if (nextPosition.y < obj.transform.position.y)
                        {
                            nextPosition.y = obj.transform.position.y - 0.5f - halfSize.y;
                            hangTimer = hitHeadHangTime;
                            isHovering = true;
                            isJumping = false;
                            break;
                        }
                    }
                }
            }
        }
    }
    void Hovering()
    {
        if (isHovering)
        {
            hangTimer -= Time.deltaTime;
            if (hangTimer <= 0f) { isHovering = false; }
        }
    }
    void Gravity()
    {
        if (!isGravity)
        {
            if (!isDropping && !isDropInitialize && !isJumping && !isHovering)
            {
                // �u���b�N�Ƃ̏Փ˔���
                bool noBlock = true;

                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
                {
                    if (nextPosition.y > obj.transform.position.y)
                    {
                        // X������
                        float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                        float xDoubleSize = halfSize.x + 0.25f;

                        // Y������
                        float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
                        float yDoubleSize = halfSize.y + 0.51f;

                        if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                        {
                            if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
                            {
                                noBlock = false;
                                break;
                            }
                        }
                    }
                }

                if (noBlock)
                {
                    gravityPower = 0f;
                    isGravity = true;
                }
            }
        }
        else
        {
            gravityPower += addGravity * Time.deltaTime;

            float deltaGravityPower = gravityPower * Time.deltaTime;
            nextPosition.y -= deltaGravityPower;

            // �u���b�N�Ƃ̏Փ˔���
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                if (nextPosition.y > obj.transform.position.y)
                {
                    // X������
                    float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.25f;

                    // Y������
                    float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
                    float yDoubleSize = halfSize.y + 0.51f;

                    if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
                    {
                        xDoubleSize = halfSize.x + 0.25f;

                        if (yBetween <= yDoubleSize && xBetween < xDoubleSize)
                        {
                            nextPosition.y = obj.transform.position.y + 0.5f + halfSize.y;
                            isGravity = false;
                            break;
                        }
                    }
                }
            }
        }
    }
    void DropInitialize()
    {
        if (!GetIsGround() && isTriggerJump && !isDropInitialize && !isDropping)
        {
            dropInitialTarget.x = Mathf.Round(nextPosition.x);
            dropInitialTarget.y = Mathf.Round(nextPosition.y);

            dropDirection = Vector3.down;
            dropTarget = dropInitialTarget;

            while (!isDropInitialize)
            {
                // �u���b�N���Ȃ������玟�ɐi�߂�
                dropTarget += dropDirection;

                // ������т̏I���_�����擾����
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
                {
                    // X������
                    float xBetween = Mathf.Abs(dropTarget.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.25f;

                    // Y������
                    float yBetween = Mathf.Abs(dropTarget.y - obj.transform.position.y);
                    float yDoubleSize = halfSize.y + 0.25f;

                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        // �u���b�N����������P��O�Ŏ~�߂�
                        if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX)
                        {
                            dropTarget = obj.transform.position;

                            if (Vector3.Distance(dropInitialTarget, dropTarget) < breakDistance)
                            {
                                dropTarget -= dropDirection;
                                isDropInitialize = true;
                                break;
                            }
                        }
                        if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() != AllObjectManager.ObjectType.BOX)
                        {
                            dropTarget = obj.transform.position;
                            dropTarget -= dropDirection;
                            isDropInitialize = true;
                            break;
                        }

                    }
                }

                // ������ʓ��ɃI�u�W�F�N�g���Ȃ��̂Ȃ��ʒ[�Ŏ~�߂�

                // ���[�𒴂�����
                float thisLeftX = dropTarget.x - halfSize.x;
                if (thisLeftX < -cameraHalfSize.x)
                {
                    dropTarget.x = -cameraHalfSize.x + halfSize.x;
                    isDropInitialize = true;
                    break;
                }

                // �E�[�𒴂�����
                float thisRightX = dropTarget.x + halfSize.x;
                if (thisRightX > cameraHalfSize.x)
                {
                    dropTarget.x = cameraHalfSize.x - halfSize.x;
                    isDropInitialize = true;
                    break;
                }

                // ��[�𒴂�����
                float thisTopY = dropTarget.y - halfSize.y;
                if (thisTopY > cameraHalfSize.y)
                {
                    dropTarget.y = cameraHalfSize.y - halfSize.y;
                    isDropInitialize = true;
                    break;
                }

                // ���[�𒴂�����
                float thisBottomY = dropTarget.y + halfSize.y;
                if (thisBottomY < -cameraHalfSize.y)
                {
                    dropTarget.y = -cameraHalfSize.y + halfSize.y;
                    isDropInitialize = true;
                    break;
                }
            }

            // �P�}�X���Ԃ̏ꍇ�͐�����΂Ȃ��悤�ɂ���
            if (Vector3.Distance(nextPosition, dropTarget) < 0.5f)
            {
                nextPosition = transform.position;
                isDropInitialize = false;
            }
            else
            {
                // �i�{�[����j��\���ǂ���
                if (Vector3.Distance(dropInitialTarget, dropTarget) >= breakDistance)
                {
                    canBreakBox = true;
                }

                transform.position = nextPosition;

                // ������ъJ�n
                transform.DOMove(dropInitialTarget, 0.5f).SetEase(Ease.OutSine).OnComplete(FinishDropInitialize);
            }
        }
    }
    void FinishDropInitialize()
    {
        // �t���O������
        isDropInitialize = false;
        isDropping = true;

        // �����ɂ���Ĉړ����x���ς��Ȃ��悤�ɒ���
        float dropTime = Vector3.Distance(nextPosition, dropTarget) / dropAmount;

        // ������ъJ�n
        transform.DOMove(dropTarget, dropTime).SetEase(Ease.OutCirc).OnComplete(FinishDrop);
    }
    void Dropping()
    {
        // �������̏���
        if (isDropping)
        {
            // �t�߃I�u�W�F�N�g������
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X������
                float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.25f;

                // Y������
                float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
                float yDoubleSize = halfSize.y + 0.25f;

                if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                {
                    // �j��̍����ɑ���Ă��邩
                    if (canBreakBox)
                    {
                        // �i�{�[������������j�󂷂�
                        if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX)
                        {
                            Destroy(obj);
                            break;
                        }
                    }

                }
            }
        }
    }
    void FinishDrop()
    {
        nextPosition = transform.position;
        gravityPower = 0f;
        isJumping = false;
        isHovering = false;
        isGravity = true;
        isDropping = false;
        canBreakBox = false;
    }
    void ClampInCamera()
    {
        // ���[�𒴂�����
        float thisLeftX = nextPosition.x - halfSize.x;
        if (thisLeftX < -cameraHalfSize.x)
        {
            nextPosition.x = -cameraHalfSize.x + halfSize.x;
        }

        // �E�[�𒴂�����
        float thisRightX = nextPosition.x + halfSize.x;
        if (thisRightX > cameraHalfSize.x)
        {
            nextPosition.x = cameraHalfSize.x - halfSize.x;
        }

        // ��[�𒴂�����
        if (nextPosition.y > cameraHalfSize.y)
        {
            nextPosition.y = cameraHalfSize.y;
            hangTimer = hangTime;
            isHovering = true;
            isJumping = false;
        }
    }

    // Getter
    void GetInput()
    {
        isPushLeft = false;
        isPushRight = false;
        isTriggerJump = false;

        if (manager.GetInputManager().IsPush(manager.GetInputManager().horizontal))
        {
            if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().horizontal) < -0.1f)
            {
                isPushLeft = true;
            }
            else if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().horizontal) > 0.1f)
            {
                isPushRight = true;
            }
        }
        if (manager.GetInputManager().IsTrgger(manager.GetInputManager().jump))
        {
            isTriggerJump = true;
        }
    }
    public bool GetIsGround()
    {
        if (isJumping || isHovering || isGravity || isDropInitialize || isDropping)
        {
            return false;
        }
        return true;
    }
    public int GetDirection()
    {
        //�������擾
        // ��-1 �E 1 

        //�E
        if (moveDirection.x > 0)
        {
            return 1;
        }
        else if (moveDirection.x < 0)
        {
            return -1;
        }

        return 0;
    }
    public float GetSpeed()
    {
        //�ړ����x���擾 
        return moveSpeed;
    }
    public bool GetIsMoving()
    {
        if (isPushLeft || isPushRight)
        {
            return true;
        }
        return false;
    }
    public bool GetIsJump()
    {
        return isJumping;
    }
    public bool GetIsHovering()
    {
        return isHovering;
    }
    public bool GetIsGravity()
    {
        return isGravity;
    }
}
