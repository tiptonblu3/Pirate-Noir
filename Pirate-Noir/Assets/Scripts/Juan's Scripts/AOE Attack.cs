using UnityEngine;

public class AOEAttack : Projectile
{
    private Vector3 startPosition; // starting position of the AOE, this will probably change in later versions
    public float maxDistance = 4f; // maximum distance the AOE can travel before deactivating, this will probably change in later versions
    public float maxDistanceSquared; // maximum distance squared for optimization, this will probably change in later versions
    
    public override void Launch(Vector3 direction)
    {
        moveDirection = direction.normalized;
        startPosition = transform.position; // set the starting position of the AOE to the position of the enemy, this will probably change in later versions
    }

    public void Start()
    {
        maxDistanceSquared = maxDistance * maxDistance; // calculate the maximum distance squared for optimization, this will probably change in later versions
    }
    

    public override void Update()
    {
        Vector3 currentPosition = transform.position;

        transform.position += moveDirection * speed * Time.deltaTime;

        if((currentPosition - startPosition).magnitude >= maxDistanceSquared) // if the AOE has traveled a certain distance, deactivate it, this will probably change in later versions
        {
            gameObject.SetActive(false);
        }
    }
}
