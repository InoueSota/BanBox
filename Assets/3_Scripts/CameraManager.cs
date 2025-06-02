using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Position
    private Vector3 originPosition;

    [Header("ÉJÉÅÉâà⁄ìÆë¨ìx")]
    [SerializeField] private float floatRange;
    [SerializeField] private float addRotateValue;
    private float rotateValue;

    void Start()
    {
        originPosition = transform.position;
    }

    void Update()
    {
        FloatCamera();
    }

    void FloatCamera()
    {
        rotateValue += addRotateValue * Time.deltaTime;

        Vector3 floatPosition = originPosition;
        floatPosition.x += Mathf.Cos(rotateValue) * floatRange;
        floatPosition.y += Mathf.Sin(rotateValue * 2f) * floatRange;
        transform.position = floatPosition;
    }
}
