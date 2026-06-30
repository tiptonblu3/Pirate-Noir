using UnityEngine;

public class PlatformCount : MonoBehaviour
{
    public PlatformProgression platformManager;
    private Rigidbody rb;
    // Future implementation, enemies will slip down the ship when it rotates and sinks.
    //private bool activePhysics = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true; // IMPORTANT: fully freeze them
    }

    public void ActivatePhysics()
    {

        rb.isKinematic = false;
        rb.useGravity = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Die();
        }
    }

    public void Die()
    {
        platformManager.EnemyDefeated();
        Destroy(gameObject);
    }
}