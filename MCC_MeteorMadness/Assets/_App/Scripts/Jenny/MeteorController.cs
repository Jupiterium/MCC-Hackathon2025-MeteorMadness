using UnityEngine;

public class MeteorController : MonoBehaviour
{
    public Transform target;
    public float speed;
    public int rarity;

    void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target.position) < 1f)
            {
                // meteor hits Earth
                Destroy(gameObject);
            }
        }
    }
}
