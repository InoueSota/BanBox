using UnityEngine;

public class SelectCameraManager : MonoBehaviour
{
    // ˆÚ“®Œn
    [SerializeField] private float chasePower;
    private float targetX;
    private Vector3 nextPosition;

    void Update()
    {
        nextPosition = transform.position;

        CameraMove();

        transform.position = nextPosition;
    }

    void CameraMove()
    {
        nextPosition.x += (targetX - nextPosition.x) * (chasePower * Time.deltaTime);
    }

    // Setter
    public void SetPosition(float _targetX)
    {
        targetX = _targetX;
        transform.position = new(targetX, transform.position.y, transform.position.z);
    }
    public void SetTargetPosition(float _targetX)
    {
        targetX = _targetX;
    }

    // Getter
    public float GetTargetX()
    {
        return targetX;
    }
}
