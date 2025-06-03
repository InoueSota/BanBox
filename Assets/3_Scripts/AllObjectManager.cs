using UnityEngine;

public class AllObjectManager : MonoBehaviour
{
    [SerializeField] private bool isActive = true;

    public enum ObjectType
    {
        GROUND,
        BLOCK,
        BOX,
        WEAK,
    }
    [SerializeField] private ObjectType objectType;

    // Setter
    public void SetIsActive (bool _isActive)
    {
        isActive = _isActive;
    }
    
    // Getter
    public bool GetIsActive()
    {
        return isActive;
    }
    public ObjectType GetObjectType()
    {
        return objectType;
    }
    public bool GetIsHitObject()
    {
        return true;
    }
}
