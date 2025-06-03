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

    [Header("爆発")]
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
                    // ジャンプしていない時は縦幅を少し細くする
                    if (!isJumping) { yDoubleSize = halfSize.y + 0.15f; }

                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        // 軟弱ブロックの破壊フラグを立てる
                        if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.WEAK) { obj.GetComponent<WeakBlockManager>().SetDisappear(); }

                        // プレイヤーが右側
                        if (nextPosition.x > obj.transform.position.x)
                        {
                            nextPosition.x = obj.transform.position.x + 0.5f + halfSize.x;

                            // 段ボールに右から左に触れたとき
                            if (moveDirection == Vector3.left) { CheckPush(obj); }
                            break;
                        }
                        // プレイヤーが左側
                        else
                        {
                            nextPosition.x = obj.transform.position.x - 0.5f - halfSize.x;

                            // 段ボールに右から左に触れたとき
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
                // X軸判定
                float xBetween = Mathf.Abs(checkPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.25f;

                // Y軸判定
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
                            // X軸判定
                            float xBetween2 = Mathf.Abs(checkPosition2.x - obj2.transform.position.x);

                            // Y軸判定
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

                        // 判定座標を進める
                        checkPosition2 += moveDirection;
                    }
                    else if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && !obj.GetComponent<BoxManager>().GetIsDropping()) { empty = false; }
                }
            }

            // 空白の１マス
            if (empty) { finishCheck = true; break; }

            // 判定座標を進める
            checkPosition += moveDirection;
        }

        if (canPush)
        {
            BoxManager _objBoxManager = null;
            if (_obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX) { _objBoxManager = _obj.GetComponent<BoxManager>(); }

            if (_objBoxManager && GetIsGround() && !_objBoxManager.GetIsDropping())
            {
                transform.position = nextPosition;

                // プレイヤーを動かす
                transform.DOMove(_obj.transform.position, pushTime).SetEase(Ease.OutSine).OnComplete(FinishPush);
                // 対象の段ボールを動かす
                _obj.transform.DOMove(_obj.transform.position + moveDirection, pushTime).SetEase(Ease.OutSine).OnComplete(_objBoxManager.FinishBeingPushed);
                // 対象の段ボールの前方に他段ボールがあるか判定し、あれば動かす処理
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
        // ジャンプ開始と初期化
        if (manager.GetCanJump() && !isPushing && !isExplositionMove && !isJumping && !isHovering && !isGravity && isTriggerJump)
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
                        // 軟弱ブロックの破壊フラグを立てる
                        if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.WEAK) { obj.GetComponent<WeakBlockManager>().SetDisappear(); }

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
            if (!isExplositionMove && !isJumping && !isHovering)
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
                                // 軟弱ブロックの破壊フラグを立てる
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
                            // 軟弱ブロックの破壊フラグを立てる
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
                // X軸判定
                float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x - 0.01f;

                // Y軸判定
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
                // X軸判定
                float xBetween = Mathf.Abs(nextPosition.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x - 0.01f;

                // Y軸判定
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
        // 落下中の処理
        if (isExplositionMove)
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

        // 横入力
        if (manager.GetInputManager().IsPush(manager.GetInputManager().horizontal))
        {
            if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().horizontal) < -0.1f) { isPushLeft = true; }
            else if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().horizontal) > 0.1f) { isPushRight = true; }
        }
        // 縦入力
        if (manager.GetInputManager().IsPush(manager.GetInputManager().vertical))
        {
            if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().vertical) < -0.1f) { isPushDown = true; }
            else if (manager.GetInputManager().ReturnInputValue(manager.GetInputManager().vertical) > 0.1f) { isPushUp = true; }
        }
        // ジャンプ
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
    public void SetExplosionMove(GameObject _explosionObj, Vector3 _explosionMoveDirection)
    {
        explosionTarget.x = Mathf.Round(transform.position.x);
        explosionTarget.y = Mathf.Round(transform.position.y);
        transform.position = explosionTarget;

        while (!isExplositionMove)
        {
            // ブロックがなかったら次に進める
            explosionTarget += _explosionMoveDirection;

            // 吹っ飛びの終着点等を取得する
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
            {
                // X軸判定
                float xBetween = Mathf.Abs(explosionTarget.x - obj.transform.position.x);
                float xDoubleSize = halfSize.x + 0.25f;

                // Y軸判定
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

        // 距離によって移動速度が変わらないように調整
        float dropTime = Vector3.Distance(transform.position, explosionTarget) / explosionAmount;

        // 吹っ飛び開始
        transform.DOKill();
        transform.DOMove(explosionTarget, dropTime).SetEase(Ease.OutSine).OnComplete(FinishExplosion);

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
