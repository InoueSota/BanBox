using DG.Tweening;
using UnityEngine;

public class BoxManager : MonoBehaviour
{
    // My Component
    [Header("横向き画像")]
    [SerializeField] private Sprite horizontalBoxSprite;
    private SpriteRenderer spriteRenderer;

    // 他コンポーネント
    private GameObject playerObj;
    private PlayerController playerController;
    private Transform playerTransform;

    // 基本情報
    private Vector2 halfSize;

    // 座標系
    private Vector3 originPosition;
    private Vector3 nextPosition;

    public enum BoxType
    {
        HORIZONTAL,
        VERTICAL
    }
    [Header("状態")]
    [SerializeField] private BoxType boxType;

    // 押されているかフラグ
    private bool isBeingPushed;

    // 吹っ飛んでいるかフラグ
    private bool isExplosionMove;

    [Header("落下")]
    [SerializeField] private float dropAmount;
    private Vector3 dropTarget;
    private bool isDropping;

    void Start()
    {
        // Sprite
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = horizontalBoxSprite;

        originPosition = transform.position;
        halfSize.x = transform.localScale.x * 0.5f;
        halfSize.y = transform.localScale.y * 0.5f;

        // Rotation
        switch (boxType)
        {
            case BoxType.VERTICAL:
                transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                break;
        }

        // Set Component - Other
        playerObj = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObj.GetComponent<PlayerController>();
        playerTransform = playerObj.transform;

        CheckGround();
    }

    void Update()
    {
        nextPosition = transform.position;

        Gravity();

        transform.position = nextPosition;
    }

    void Gravity()
    {
        if (!isDropping && !isBeingPushed && !isExplosionMove)
        {
            CheckGround();
        }
    }
    void CheckGround()
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
                    if (obj != this && obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && obj.GetComponent<BoxManager>().GetIsDropping())
                    {
                        break;
                    }
                    else if (obj != this && obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
                    {
                        noBlock = false;
                        break;
                    }
                }
            }
        }

        if (nextPosition.y > playerTransform.position.y)
        {
            // X軸判定
            float xBetween = Mathf.Abs(nextPosition.x - playerTransform.position.x);
            float xDoubleSize = halfSize.x + 0.25f;

            // Y軸判定
            float yBetween = Mathf.Abs(nextPosition.y - playerTransform.position.y);
            float yDoubleSize = halfSize.y + 0.51f;

            if (yBetween < yDoubleSize && xBetween < xDoubleSize)
            {
                noBlock = false;
            }
        }

        if (noBlock)
        {
            dropTarget.x = Mathf.Round(transform.position.x);
            dropTarget.y = Mathf.Round(transform.position.y);

            int droppingCount = 1;

            while (!isDropping)
            {
                // ブロックがなかったら次に進める
                dropTarget += Vector3.down;

                // 落下の終着点等を取得する
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
                        if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && obj.GetComponent<BoxManager>().GetIsDropping())
                        {
                            droppingCount++;
                            break;
                        }
                        else if (obj.GetComponent<AllObjectManager>().GetIsActive())
                        {
                            dropTarget = obj.transform.position;
                            dropTarget -= Vector3.down * droppingCount;
                            isDropping = true;
                            break;
                        }
                    }
                }
            }

            // 落下距離を取得
            float dropDistance = Vector3.Distance(transform.position, dropTarget);

            // 距離によって移動速度が変わらないように調整
            float dropTime = dropDistance / dropAmount;

            // 落下距離に応じてBoxTypeを変化させる
            switch (boxType)
            {
                case BoxType.HORIZONTAL:
                    if (Mathf.Round(dropDistance) % 2 == 1) { boxType = BoxType.VERTICAL; }
                    break;
                case BoxType.VERTICAL:
                    if (Mathf.Round(dropDistance) % 2 == 1) { boxType = BoxType.HORIZONTAL; }
                    break;
            }

            // 落下開始
            transform.DOKill();
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(dropTarget, dropTime).SetEase(Ease.InSine));
            sequence.Join(transform.DORotate(Vector3.forward * (90f * Mathf.Round(dropDistance)), dropTime, RotateMode.WorldAxisAdd).SetEase(Ease.InSine));
            sequence.Play().OnComplete(FinishDrop);

            isDropping = true;
        }
    }
    void FinishDrop()
    {
        nextPosition = transform.position;
        isDropping = false;
    }

    // Getter
    public bool GetIsDropping()
    {
        return isDropping;
    }
    public BoxType GetBoxType()
    {
        return boxType;
    }

    // Setter
    public void SetIsBeingPushed(Vector3 _moveDirection, float _pushTime)
    {
        isBeingPushed = true;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
        {
            // X軸判定
            float xBetween = Mathf.Abs(nextPosition.x + _moveDirection.x - obj.transform.position.x);
            float xDoubleSize = halfSize.x + 0.5f;

            // Y軸判定
            float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
            float yDoubleSize = halfSize.y + 0.25f;

            if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
            {
                if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                {
                    // 段ボールに横から触れたとき
                    BoxManager _objBoxManager = obj.GetComponent<BoxManager>();

                    if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && !_objBoxManager.GetIsDropping())
                    {
                        transform.position = nextPosition;

                        obj.transform.DOMove(obj.transform.position + _moveDirection, _pushTime).SetEase(Ease.OutSine).OnComplete(_objBoxManager.FinishBeingPushed);
                        _objBoxManager.SetIsBeingPushed(_moveDirection, _pushTime);
                        break;
                    }
                    else if (obj.GetComponent<AllObjectManager>().GetObjectType() != AllObjectManager.ObjectType.BOX)
                    {
                        DestroySelf(_moveDirection);
                    }
                }
            }
        }
    }
    public void FinishBeingPushed()
    {
        isBeingPushed = false;
        CheckGround();
        nextPosition = transform.position;
    }
    public void SetIsExplosionMove(Vector3 _explosionMoveDirection, Vector3 _position, float _pushTime)
    {
        isExplosionMove = true;

        // 隣にアクティブな段ボールがあったらそれも吹き飛ばす
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
        {
            // X軸判定
            float xBetween = Mathf.Abs(Mathf.Round(transform.position.x) - _explosionMoveDirection.x - obj.transform.position.x);
            float xDoubleSize = halfSize.x + 0.25f;

            // Y軸判定
            float yBetween = Mathf.Abs(Mathf.Round(transform.position.y) - obj.transform.position.y);
            float yDoubleSize = halfSize.y + 0.25f;

            if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
            {
                if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                {
                    // 段ボールに横から触れたとき
                    BoxManager _objBoxManager = obj.GetComponent<BoxManager>();

                    if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && !_objBoxManager.GetIsDropping())
                    {
                        transform.position = nextPosition;

                        obj.transform.DOKill();
                        obj.transform.DOMove(_position - _explosionMoveDirection, _pushTime).SetEase(Ease.OutSine).OnComplete(_objBoxManager.FinishExplosionMove);
                        _objBoxManager.SetIsExplosionMove(_explosionMoveDirection, _position - _explosionMoveDirection, _pushTime);
                        break;
                    }
                }
            }
        }
    }
    public void FinishExplosionMove()
    {
        isExplosionMove = false;
        CheckGround();
        nextPosition = transform.position;
    }

    public void DestroySelf(Vector3 _moveDirection)
    {
        // 爆発移動
        playerController.SetExplosionMove(-_moveDirection);

        // 消去する
        Destroy(gameObject);
    }
}
