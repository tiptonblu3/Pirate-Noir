using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // speed of the projectile, this will probably change in later versions
    public Vector3 moveDirection; // direction of the projectile, this will probably change in later versions

    public virtual void Launch(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }
}
