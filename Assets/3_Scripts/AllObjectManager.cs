using UnityEngine;

public class AllObjectManager : MonoBehaviour
{
    [SerializeField] private bool isActive = true;

    public enum ObjectType
    {
        GROUND,
        BLOCK,
        BOX
    }
    [SerializeField] private ObjectType objectType;

    [Header("HP")]
    [SerializeField] private int hp;
    private int maxHp;

    void Start()
    {
        maxHp = hp;
    }

    // Setter
    public void Initialize()
    {
        hp = maxHp;
    }
    public void Damage()
    {
        hp--;
    }
    public void SetIsActive (bool _isActive)
    {
        isActive = _isActive;
    }
    
    // Getter
    public int GetHp()
    {
        return hp;
    }
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
