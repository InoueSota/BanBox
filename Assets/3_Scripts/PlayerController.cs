using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // My Component

    private PlayerManager manager;
    private bool isPushLeft;
    private bool isPushRight;
    private bool isPushUp;
    private bool isPushDown;
    private bool isTriggerJump;

    // Sprite
    private SpriteRenderer spriteRenderer;
    [Header("Sprite")]
    [SerializeField] private Sprite centerSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite explosionVerticalSprite;
    [SerializeField] private Sprite moveSprite;
    [SerializeField] private Sprite pushSprite;
    [SerializeField] private Sprite explosionHorizontalSprite;

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

    [Header("����")]
    [SerializeField] private float pushTime;
    private bool isPushing;

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

    [Header("����")]
    [SerializeField] private float explosionAmount;
    private Vector3 explosionTarget;
    private bool isExplositionMove;

    void Start()
    {
        manager = GetComponent<PlayerManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        halfSize.x = transform.localScale.x * 0.5f;
        halfSize.y = transform.localScale.y * 0.5f;
        cameraHalfSize.x = Camera.main.ScreenToWorldPoint(new(Screen.width, 0f, 0f)).x;
        cameraHalfSize.y = Camera.main.ScreenToWorldPoint(new(0f, Screen.height, 0f)).y;
        originPosition = transform.position;
    }
    public void Initialize()
    {
        transform.position = originPosition;
        isJumping = false;
        isHovering = false;
        isGravity = false;
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
            Jump();
            Hovering();
            Gravity();
            Thwart();
            Explosion();

            ClampInCamera();
            ChangeSprite();

            transform.position = nextPosition;
        }
    }

    void CheckDirection()
    {
        if (!isExplositionMove && (isPushLeft || isPushRight))
        {
            moveSpeed = maxSpeed + acceleration;

            if (isPushLeft)
            {
                moveDirection = Vector3.left;
                spriteRenderer.flipX = true;
            }
            else if (isPushRight)
            {
                moveDirection = Vector3.right;
                spriteRenderer.flipX = false;
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
        if (!isPushing && !isExplositionMove && (isPushLeft || isPushRight))
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
                    // �W�����v���Ă��Ȃ����͏c���������ׂ�����
                    if (!isJumping) { yDoubleSize = halfSize.y + 0.15f; }

                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        // ���u���b�N�̔j��t���O�𗧂Ă�
                        if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.WEAK) { obj.GetComponent<WeakBlockManager>().SetDisappear(); }

                        // �v���C���[���E��
                        if (nextPosition.x > obj.transform.position.x)
                        {
                            nextPosition.x = obj.transform.position.x + 0.5f + halfSize.x;

                            // �i�{�[���ɉE���獶�ɐG�ꂽ�Ƃ�
                            if (moveDirection == Vector3.left) { CheckPush(obj); }
                            break;
                        }
                        // �v���C���[������
                        else
                        {
                            nextPosition.x = obj.transform.position.x - 0.5f - halfSize.x;

                            // �i�{�[���ɉE���獶�ɐG�ꂽ�Ƃ�
                            if (moveDirection == Vector3.right) { CheckPush(obj); }
                            break;
                        }
                    }
                }
            }
        }
    }
    void CheckPush(GameObject _obj)
    {
        bool finishCheck = false;
        bool canPush = true;
        Vector3 checkPosition = transform.position + moveDirection;
        Vector3 checkPosition2 = transform.position + moveDirection;

        while (!finishCheck)
        {
            bool empty = true;

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X������
                float xBetween = Mathf.Abs(checkPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.25f;

                // Y������
                float yBetween = Mathf.Abs(checkPosition.y - obj.transform.position.y);
                float yDoubleSize = halfSize.y + 0.25f;

                if (yBetween < yDoubleSize && xBetween < xDoubleSize && obj.GetComponent<AllObjectManager>().GetIsActive())
                {
                    if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.GROUND ||
                        obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BLOCK ||
                        obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.WEAK)
                    {
                        empty = false;

                        foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("Object"))
                        {
                            // X������
                            float xBetween2 = Mathf.Abs(checkPosition2.x - obj2.transform.position.x);

                            // Y������
                            float yBetween2 = Mathf.Abs(checkPosition2.y - obj2.transform.position.y);

                            if (yBetween2 < yDoubleSize && xBetween2 < xDoubleSize && obj2.GetComponent<AllObjectManager>().GetIsActive())
                            {
                                if (obj2.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.GROUND ||
                                    obj2.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BLOCK ||
                                    obj2.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.WEAK)
                                {
                                    canPush = false;
                                    finishCheck = true;
                                    break;
                                }
                                else if (obj2.GetComponent<BoxManager>().GetBoxType() == BoxManager.BoxType.HORIZONTAL)
                                {
                                    finishCheck = true;
                                    break;
                                }
                            }
                        }

                        // ������W��i�߂�
                        checkPosition2 += moveDirection;
                    }
                    else if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && !obj.GetComponent<BoxManager>().GetIsDropping()) { empty = false; }
                }
            }

            // �󔒂̂P�}�X
            if (empty) { finishCheck = true; break; }

            // ������W��i�߂�
            checkPosition += moveDirection;
        }

        if (canPush)
        {
            BoxManager _objBoxManager = null;
            if (_obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX) { _objBoxManager = _obj.GetComponent<BoxManager>(); }

            if (_objBoxManager && GetIsGround() && !_objBoxManager.GetIsDropping())
            {
                transform.position = nextPosition;

                // �v���C���[�𓮂���
                transform.DOMove(_obj.transform.position, pushTime).SetEase(Ease.OutSine).OnComplete(FinishPush);
                // �Ώۂ̒i�{�[���𓮂���
                _obj.transform.DOMove(_obj.transform.position + moveDirection, pushTime).SetEase(Ease.OutSine).OnComplete(_objBoxManager.FinishBeingPushed);
                // �Ώۂ̒i�{�[���̑O���ɑ��i�{�[�������邩���肵�A����Γ���������
                _objBoxManager.SetIsBeingPushed(moveDirection, pushTime);
                isPushing = true;
            }
        }
    }
    void FinishPush()
    {
        nextPosition = transform.position;
        isPushing = false;
    }
    void Jump()
    {
        // �W�����v�J�n�Ə�����
        if (manager.GetCanJump() && !isPushing && !isExplositionMove && !isJumping && !isHovering && !isGravity && isTriggerJump)
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
                        // ���u���b�N�̔j��t���O�𗧂Ă�
                        if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.WEAK) { obj.GetComponent<WeakBlockManager>().SetDisappear(); }

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
            if (!isExplositionMove && !isJumping && !isHovering)
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
                                // ���u���b�N�̔j��t���O�𗧂Ă�
                                if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.WEAK) { obj.GetComponent<WeakBlockManager>().SetDisappear(); }

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
                            // ���u���b�N�̔j��t���O�𗧂Ă�
                            if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.WEAK) { obj.GetComponent<WeakBlockManager>().SetDisappear(); }

                            nextPosition.y = obj.transform.position.y + 0.5f + halfSize.y;
                            isGravity = false;
                            break;
                        }
                    }
                }
            }
        }
    }
    void Thwart()
    {
        if (!isPushing && !isExplositionMove && !isJumping && !isHovering && !isGravity && isPushDown)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X������
                float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x - 0.01f;

                // Y������
                float yBetween = Mathf.Abs(nextPosition.y - 1f - obj.transform.position.y);
                float yDoubleSize = halfSize.y - 0.01f;

                if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX)
                {
                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        if (obj.GetComponent<BoxManager>().GetBoxType() == BoxManager.BoxType.VERTICAL) { obj.GetComponent<BoxManager>().DestroySelf(Vector3.down); break; }
                    }
                }
            }
        }
        else if (!isPushing && !isExplositionMove && !isJumping && !isHovering && !isGravity && isPushUp)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X������
                float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x - 0.01f;

                // Y������
                float yBetween = Mathf.Abs(nextPosition.y + 1f - obj.transform.position.y);
                float yDoubleSize = halfSize.y - 0.01f;

                if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX)
                {
                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        if (obj.GetComponent<BoxManager>().GetBoxType() == BoxManager.BoxType.VERTICAL) { obj.GetComponent<BoxManager>().DestroySelf(Vector3.up); break; }
                    }
                }
            }
        }
    }
    void Explosion()
    {
        // �������̏���
        if (isExplositionMove)
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
                    // �i�{�[������������j�󂷂�
                    if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() != AllObjectManager.ObjectType.GROUND)
                    {
                        Destroy(obj);
                        break;
                    }
                }
            }
        }
    }
    void FinishExplosion()
    {
        nextPosition = transform.position;
        gravityPower = 0f;
        isJumping = false;
        isHovering = false;
        isGravity = true;
        isPushing = false;
        isExplositionMove = false;
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
    void ChangeSprite()
    {
        if (!isExplositionMove)
        {
            if (isPushing) { spriteRenderer.sprite = pushSprite; }
            else if (isPushLeft || isPushRight) { spriteRenderer.sprite = moveSprite; }
            else if (isPushUp) { spriteRenderer.sprite = upSprite; }
            else if (isPushDown) { spriteRenderer.sprite = downSprite; }
            else { spriteRenderer.sprite = centerSprite; }
        }
    }

    // Getter
    void GetInput()
    {
        isPushLeft = false;
        isPushRight = false;
        isPushUp = false;
        isPushDown = false;
        isTriggerJump = false;

        // ������
        if (manager.GetInputManager().IsPush(manager.GetInputManager().horizontal))
        {
            if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().horizontal) < -0.1f) { isPushLeft = true; }
            else if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().horizontal) > 0.1f) { isPushRight = true; }
        }
        // �c����
        if (manager.GetInputManager().IsPush(manager.GetInputManager().vertical))
        {
            if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().vertical) < -0.1f) { isPushDown = true; }
            else if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().vertical) > 0.1f) { isPushUp = true; }
        }
        // �W�����v
        if (manager.GetInputManager().IsTrgger(manager.GetInputManager().jump)) { isTriggerJump = true; }
    }
    public bool GetIsGround()
    {
        if (isJumping || isHovering || isGravity || isExplositionMove)
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

    // Setter
    public void SetExplosionMove(GameObject _explosionObj, Vector3 _explosionMoveDirection)
    {
        explosionTarget.x = Mathf.Round(transform.position.x);
        explosionTarget.y = Mathf.Round(transform.position.y);
        transform.position = explosionTarget;

        while (!isExplositionMove)
        {
            // �u���b�N���Ȃ������玟�ɐi�߂�
            explosionTarget += _explosionMoveDirection;

            // ������т̏I���_�����擾����
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X������
                float xBetween = Mathf.Abs(explosionTarget.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.25f;

                // Y������
                float yBetween = Mathf.Abs(explosionTarget.y - obj.transform.position.y);
                float yDoubleSize = halfSize.y + 0.25f;

                if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                {
                    if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.GROUND)
                    {
                        explosionTarget = obj.transform.position;
                        explosionTarget -= _explosionMoveDirection;

                        // Sprite
                        if (Mathf.Abs(_explosionMoveDirection.x) > 0f) { spriteRenderer.sprite = explosionHorizontalSprite; }
                        else { spriteRenderer.sprite = explosionVerticalSprite; }

                        isExplositionMove = true;
                        break;
                    }
                }
            }
        }

        nextPosition = transform.position;

        // �����ɂ���Ĉړ����x���ς��Ȃ��悤�ɒ���
        float dropTime = Vector3.Distance(transform.position, explosionTarget) / explosionAmount;

        // ������ъJ�n
        transform.DOKill();
        transform.DOMove(explosionTarget, dropTime).SetEase(Ease.OutSine).OnComplete(FinishExplosion);

        // �ׂɃA�N�e�B�u�Ȓi�{�[�����������炻���������΂�
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
        {
            // X������
            float xBetween = Mathf.Abs(Mathf.Round(transform.position.x) - _explosionMoveDirection.x - obj.transform.position.x);
            float xDoubleSize = halfSize.x + 0.25f;

            // Y������
            float yBetween = Mathf.Abs(Mathf.Round(transform.position.y) - obj.transform.position.y);
            float yDoubleSize = halfSize.y + 0.25f;

            if (yBetween < yDoubleSize && xBetween < xDoubleSize)
            {
                if (obj != _explosionObj && obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX)
                {
                    obj.transform.DOKill();
                    obj.transform.DOMove(explosionTarget - _explosionMoveDirection, dropTime).SetEase(Ease.OutSine).OnComplete(obj.GetComponent<BoxManager>().FinishExplosionMove);
                    obj.GetComponent<BoxManager>().SetIsExplosionMove(_explosionObj, _explosionMoveDirection, explosionTarget - _explosionMoveDirection, dropTime);
                    break;
                }
            }
        }
    }
}
