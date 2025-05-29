using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    // 自コンポーネント取得
    private PlayerManager manager;
    private bool isPushLeft;
    private bool isPushRight;
    private bool isTriggerJump;

    // 基本情報
    private Vector2 halfSize;
    private Vector2 cameraHalfSize;

    // 座標系
    private Vector3 originPosition;
    private Vector3 nextPosition;

    [Header("横移動")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private float accelerationTime;
    private float moveSpeed;
    private float acceleration;
    private float accelerationTimer;
    private bool canAcceleration;
    private Vector3 moveDirection;

    [Header("押す")]
    [SerializeField] private float pushTime;
    private bool isPushing;

    [Header("ジャンプ")]
    [SerializeField] private float jumpDistance;
    [SerializeField] private float jumpSpeed;
    private bool isJumping;
    private float jumpTarget;

    [Header("滞空")]
    [SerializeField] private float hangTime;
    [SerializeField] private float hitHeadHangTime;
    private bool isHovering;
    private float hangTimer;

    [Header("重力")]
    [SerializeField] private float gravityMax;
    [SerializeField] private float addGravity;
    private bool isGravity;
    private float gravityPower;

    // 落下初期化
    private Vector3 dropInitialTarget;
    private bool isDropInitialize;

    [Header("落下")]
    [SerializeField] private float dropAmount;
    [SerializeField] private float breakDistance;
    private Vector3 dropTarget;
    private Vector3 dropDirection;
    private bool canBreakBox;
    private bool isDropping;

    // 爆発
    private bool isExplositionMove;
    private bool canBreakAll;

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
        // 移動方向の修正
        CheckDirection();

        // 加速
        Acceleration();

        // 移動
        float deltaMoveSpeed = moveSpeed * Time.deltaTime;
        nextPosition += deltaMoveSpeed * moveDirection;

        // ブロックとの衝突判定
        if (!isPushing && !isExplositionMove && (isPushLeft || isPushRight))
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X軸判定
                float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.5f;

                // Y軸判定
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
                        // 段ボールに横から触れたとき
                        CheckPush(obj);

                        // プレイヤーが右側
                        if (nextPosition.x > obj.transform.position.x)
                        {
                            nextPosition.x = obj.transform.position.x + 0.5f + halfSize.x;
                            break;
                        }
                        // プレイヤーが左側
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
    void CheckPush(GameObject _obj)
    {
        BoxManager _objBoxManager = _obj.GetComponent<BoxManager>();

        if (GetIsGround() && _obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && !_objBoxManager.GetIsGravity())
        {
            transform.position = nextPosition;

            transform.DOMove(_obj.transform.position, pushTime).SetEase(Ease.OutSine).OnComplete(FinishPush);
            _obj.transform.DOMove(_obj.transform.position + moveDirection, pushTime).SetEase(Ease.OutSine).OnComplete(_objBoxManager.FinishBeingPushed);
            _objBoxManager.SetIsBeingPushed(moveDirection, pushTime);
            isPushing = true;
        }
    }
    void FinishPush()
    {
        nextPosition = transform.position;
        isPushing = false;
    }
    void Jump()
    {
        // ジャンプ開始と初期化
        if (manager.GetCanJump() && !isPushing && !isDropping && !isDropInitialize && !isJumping && !isHovering && !isGravity && isTriggerJump)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                if (nextPosition.y > obj.transform.position.y)
                {
                    // X軸判定
                    float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.5f;

                    // Y軸判定
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

        // ジャンプ処理
        if (isJumping)
        {
            float deltaJumpSpeed = jumpSpeed * Time.deltaTime;
            nextPosition.y += (jumpTarget - nextPosition.y) * deltaJumpSpeed;

            // ジャンプ終了処理
            if (Mathf.Abs(jumpTarget - nextPosition.y) < 0.03f)
            {
                nextPosition.y = jumpTarget;

                hangTimer = hangTime;
                isHovering = true;
                isJumping = false;
            }

            // ブロックとの衝突判定
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
                {
                    // X軸判定
                    float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.26f;

                    // Y軸判定
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
                // ブロックとの衝突判定
                bool noBlock = true;

                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
                {
                    if (nextPosition.y > obj.transform.position.y)
                    {
                        // X軸判定
                        float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                        float xDoubleSize = halfSize.x + 0.25f;

                        // Y軸判定
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

            // ブロックとの衝突判定
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                if (nextPosition.y > obj.transform.position.y)
                {
                    // X軸判定
                    float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.25f;

                    // Y軸判定
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
                // ブロックがなかったら次に進める
                dropTarget += dropDirection;

                // 吹っ飛びの終着点等を取得する
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
                {
                    // X軸判定
                    float xBetween = Mathf.Abs(dropTarget.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.25f;

                    // Y軸判定
                    float yBetween = Mathf.Abs(dropTarget.y - obj.transform.position.y);
                    float yDoubleSize = halfSize.y + 0.25f;

                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        // ブロックがあったら１つ手前で止める
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
            }

            // １マス隙間の場合は吹っ飛ばないようにする
            if (Vector3.Distance(nextPosition, dropTarget) < 0.5f)
            {
                nextPosition = transform.position;
                isDropInitialize = false;
            }
            else
            {
                // 段ボールを破壊可能かどうか
                if (Vector3.Distance(dropInitialTarget, dropTarget) >= breakDistance)
                {
                    canBreakBox = true;
                }

                transform.position = nextPosition;

                // 吹っ飛び開始
                transform.DOMove(dropInitialTarget, 0.5f).SetEase(Ease.OutSine).OnComplete(FinishDropInitialize);
            }
        }
    }
    void FinishDropInitialize()
    {
        // フラグ初期化
        isDropInitialize = false;
        isDropping = true;

        // 距離によって移動速度が変わらないように調整
        float dropTime = Vector3.Distance(nextPosition, dropTarget) / dropAmount;

        // 吹っ飛び開始
        transform.DOMove(dropTarget, dropTime).SetEase(Ease.OutCirc).OnComplete(FinishDrop);
    }
    void Dropping()
    {
        // 落下中の処理
        if (isDropping)
        {
            // 付近オブジェクトを検索
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X軸判定
                float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.25f;

                // Y軸判定
                float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
                float yDoubleSize = halfSize.y + 0.25f;

                if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                {
                    // 破壊の高さに足りているか
                    if (canBreakBox)
                    {
                        // 段ボールがあったら破壊する
                        if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX)
                        {
                            Destroy(obj);
                            break;
                        }
                    }
                    else if (canBreakAll)
                    {
                        // 段ボールがあったら破壊する
                        if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() != AllObjectManager.ObjectType.GROUND)
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
        isDropInitialize = false;
        isDropping = false;
        isPushing = false;
        canBreakBox = false;
        canBreakAll = false;
        isExplositionMove = false;
    }
    void ClampInCamera()
    {
        // 左端を超えたか
        float thisLeftX = nextPosition.x - halfSize.x;
        if (thisLeftX < -cameraHalfSize.x)
        {
            nextPosition.x = -cameraHalfSize.x + halfSize.x;
        }

        // 右端を超えたか
        float thisRightX = nextPosition.x + halfSize.x;
        if (thisRightX > cameraHalfSize.x)
        {
            nextPosition.x = cameraHalfSize.x - halfSize.x;
        }

        // 上端を超えたか
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
        //向きを取得
        // 左-1 右 1 

        //右
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
        //移動速度を取得 
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
    public void SetExplosionMove(Vector3 _explosionMoveDirection)
    {
        if (!isExplositionMove)
        {
            dropTarget.x = Mathf.Round(transform.position.x);
            dropTarget.y = Mathf.Round(transform.position.y);

            while (!isDropping)
            {
                // ブロックがなかったら次に進める
                dropTarget += _explosionMoveDirection;

                // 吹っ飛びの終着点等を取得する
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
                {
                    // X軸判定
                    float xBetween = Mathf.Abs(dropTarget.x - obj.transform.position.x);
                    float xDoubleSize = halfSize.x + 0.25f;

                    // Y軸判定
                    float yBetween = Mathf.Abs(dropTarget.y - obj.transform.position.y);
                    float yDoubleSize = halfSize.y + 0.25f;

                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.GROUND)
                        {
                            dropTarget = obj.transform.position;
                            dropTarget -= _explosionMoveDirection;
                            isDropping = true;
                            break;
                        }
                    }
                }
            }

            nextPosition = transform.position;

            // 距離によって移動速度が変わらないように調整
            float dropTime = Vector3.Distance(transform.position, dropTarget) / dropAmount;

            // 吹っ飛び開始
            transform.DOKill();
            transform.DOMove(dropTarget, dropTime).SetEase(Ease.OutSine).OnComplete(FinishDrop);

            // 隣にアクティブな段ボールがあったらそれも吹き飛ばす
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X軸判定
                float xBetween = Mathf.Abs(Mathf.Round(transform.position.x) - _explosionMoveDirection.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.25f;

                // Y軸判定
                float yBetween = Mathf.Abs(Mathf.Round(transform.position.y) - obj.transform.position.y);
                float yDoubleSize = halfSize.y + 0.25f;

                if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                {
                    if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX)
                    {
                        obj.transform.DOKill();
                        obj.transform.DOMove(dropTarget - _explosionMoveDirection, dropTime).SetEase(Ease.OutSine).OnComplete(obj.GetComponent<BoxManager>().FinishExplosionMove);
                        obj.GetComponent<BoxManager>().SetIsExplosionMove(_explosionMoveDirection, dropTarget - _explosionMoveDirection, dropTime);
                        break;
                    }
                }
            }

            // フラグ類
            canBreakAll = true;
            isExplositionMove = true;
        }
    }
}
