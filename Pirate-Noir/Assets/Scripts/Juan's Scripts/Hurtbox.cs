using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public Enemy enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemy.DamagePlayer();
        }
    }
    

}
