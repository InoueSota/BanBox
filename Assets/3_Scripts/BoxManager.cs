using DG.Tweening;
using UnityEngine;

public class BoxManager : MonoBehaviour
{
    // ���R���|�[�l���g
    private GameObject playerObj;
    private PlayerController playerController;
    private Transform playerTransform;

    // ��{���
    private Vector2 halfSize;

    // ���W�n
    private Vector3 originPosition;
    private Vector3 nextPosition;

    public enum BoxType
    {
        HORIZONTAL,
        VERTICAL
    }
    [Header("���")]
    [SerializeField] private BoxType boxType;

    // ������Ă��邩�t���O
    private bool isBeingPushed;

    // �������ł��邩�t���O
    private bool isExplosionMove;

    [Header("�d��")]
    [SerializeField] private float gravityMax;
    [SerializeField] private float addGravity;
    private bool isGravity;
    private float gravityPower;

    void Start()
    {
        originPosition = transform.position;
        halfSize.x = transform.localScale.x * 0.5f;
        halfSize.y = transform.localScale.y * 0.5f;

        // Set Component - Other
        playerObj = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObj.GetComponent<PlayerController>();
        playerTransform = playerObj.transform;

        isGravity = false;
    }

    void Update()
    {
        nextPosition = transform.position;

        Gravity();

        transform.position = nextPosition;
    }

    void Gravity()
    {
        if (!isGravity)
        {
            if (!isBeingPushed && !isExplosionMove)
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
                            if (obj != this && obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
                            {
                                noBlock = false;
                                break;
                            }
                        }
                    }
                }

                if (nextPosition.y > playerTransform.position.y)
                {
                    // X������
                    float xBetween = Mathf.Abs(nextPosition.x - playerTransform.position.x);
                    float xDoubleSize = halfSize.x + 0.25f;

                    // Y������
                    float yBetween = Mathf.Abs(nextPosition.y - playerTransform.position.y);
                    float yDoubleSize = halfSize.y + 0.51f;

                    if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                    {
                        noBlock = false;
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

                    if (obj != this && obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
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

    // Getter
    public bool GetIsGravity()
    {
        return isGravity;
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
            // X������
            float xBetween = Mathf.Abs(nextPosition.x + _moveDirection.x - obj.transform.position.x);
            float xDoubleSize = halfSize.x + 0.5f;

            // Y������
            float yBetween = Mathf.Abs(nextPosition.y - obj.transform.position.y);
            float yDoubleSize = halfSize.y + 0.25f;

            if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
            {
                if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                {
                    // �i�{�[���ɉ�����G�ꂽ�Ƃ�
                    BoxManager _objBoxManager = obj.GetComponent<BoxManager>();

                    if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && !_objBoxManager.GetIsGravity())
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
        isGravity = true;
        gravityPower = 0f;
        nextPosition = transform.position;
    }
    public void SetIsExplosionMove(Vector3 _explosionMoveDirection, Vector3 _position, float _pushTime)
    {
        isExplosionMove = true;

        // �ׂɃA�N�e�B�u�Ȓi�{�[�����������炻���������΂�
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
        {
            // X������
            float xBetween = Mathf.Abs(Mathf.Round(transform.position.x) - _explosionMoveDirection.x - obj.transform.position.x);
            float xDoubleSize = halfSize.x + 0.25f;

            // Y������
            float yBetween = Mathf.Abs(Mathf.Round(transform.position.y) - obj.transform.position.y);
            float yDoubleSize = halfSize.y + 0.25f;

            if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetIsHitObject())
            {
                if (yBetween < yDoubleSize && xBetween < xDoubleSize)
                {
                    // �i�{�[���ɉ�����G�ꂽ�Ƃ�
                    BoxManager _objBoxManager = obj.GetComponent<BoxManager>();

                    if (obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX && !_objBoxManager.GetIsGravity())
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
        isGravity = true;
        gravityPower = 0f;
        nextPosition = transform.position;
    }

    public void DestroySelf(Vector3 _moveDirection)
    {
        // �����ړ�
        playerController.SetExplosionMove(-_moveDirection);

        // ��������
        Destroy(gameObject);
    }
}
