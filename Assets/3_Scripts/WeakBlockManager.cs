using UnityEngine;

public class WeakBlockManager : MonoBehaviour
{
    // My Component
    private Animator animator;

    // Other Component
    private Transform playerTransform;

    void Start()
    {
        // Get Component
        animator = GetComponent<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        //CheckPlayer();
        CheckBox();
    }

    void CheckPlayer()
    {
        float doubleSize = 1.00001f;

        // XŽ²”»’è
        float xBetween = Mathf.Abs(transform.position.x - playerTransform.position.x);
        // YŽ²”»’è
        float yBetween = Mathf.Abs(transform.position.y - playerTransform.position.y);

        if (yBetween < doubleSize && xBetween < doubleSize) { animator.SetTrigger("Start"); }
    }
    void CheckBox()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Object"))
        {
            // XŽ²”»’è
            float xBetween = Mathf.Abs(transform.position.x - obj.transform.position.x);
            float xDoubleSize = 0.8f;
            // YŽ²”»’è
            float yBetween = Mathf.Abs(transform.position.y - obj.transform.position.y);
            float yDoubleSize = 1.01f;

            if (obj.GetComponent<AllObjectManager>().GetIsActive() && obj.GetComponent<AllObjectManager>().GetObjectType() == AllObjectManager.ObjectType.BOX)
            {
                if (yBetween < yDoubleSize && xBetween < xDoubleSize) { animator.SetTrigger("Start"); }
            }
        }
    }

    // Setter
    public void SetDisappear() { animator.SetTrigger("Start"); }
}
