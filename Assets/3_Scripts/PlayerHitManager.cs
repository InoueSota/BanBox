using UnityEngine;

public class PlayerHitManager : MonoBehaviour
{
    // äÓñ{èÓïÒ
    private Vector2 halfSize;

    void Start()
    {
        halfSize.x = transform.localScale.x * 0.5f;
        halfSize.y = transform.localScale.y * 0.5f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        OnTrigger2D(collision);
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        OnTrigger2D(collision);
    }
    void OnTrigger2D(Collider2D collision)
    {
        /*if (collision.CompareTag("Object"))
        {
            if (collision.GetComponent<AllObjectManager>().GetIsActive()&&collision.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.ITEM)
            {

            }
        }*/
    }
}
