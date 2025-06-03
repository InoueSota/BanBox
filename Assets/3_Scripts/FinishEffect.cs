using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishEffect : MonoBehaviour
{
    [SerializeField] private string changeSceneName;

    public void ChangeScene()
    {
        SceneManager.LoadScene(changeSceneName);
    }

    public void SetActiveFalse()
    {
        gameObject.SetActive(false);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
