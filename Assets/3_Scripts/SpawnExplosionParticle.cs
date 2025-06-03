using UnityEngine;

public class SpawnExplosionParticle : MonoBehaviour
{
    [SerializeField] private GameObject targetObj;
    [SerializeField] private int maxParticle;

    public void Initialize()
    {
        for (int i = 0; i < maxParticle; i++)
        {
            GameObject particle = Instantiate(targetObj, transform.position, Quaternion.identity);

            Vector3 direction = Vector3.zero;
            direction.x = Mathf.Cos(Mathf.Deg2Rad * i * (360f / maxParticle));
            direction.y = Mathf.Sin(Mathf.Deg2Rad * i * (360f / maxParticle));

            particle.GetComponent<ExplosionParticleManager>().Initialize(direction);
        }

        Destroy(gameObject);
    }
}
