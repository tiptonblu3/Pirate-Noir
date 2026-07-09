using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class RangedEnemy : Enemy
{
    public List<GameObject> ProjectileList = new List<GameObject>();
    public GameObject Projectile; // object to pool, will be using the sword for now.
    public int poolSize = 10;
    private int nextProjectileIndex = 0;

    private float timer; //will be used for the attack code

    

    public override void Start()
    {
        base.Start(); // call the base start method to initialize the enemy

        // Object pooling for projectiles
        for(int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(Projectile);
            obj.SetActive(false);
            ProjectileList.Add(obj);
        }
    }
    

    public override void EnemyDied()
    {
        Debug.Log("Enemy died.");
        
        foreach (GameObject Projectile in ProjectileList)
        {
            if (Projectile != null)
            {
                Debug.Log("Disabling projectile");
                Destroy(Projectile);
            }
        }

        ProjectileList.Clear(); // make the pool separate later on
        base.EnemyDied();
    }
    
    
    #region Attack Code
    public GameObject GetPooledProjectile()
    {
        for (int i = 0; i < ProjectileList.Count; i++)
        {
            int index = (nextProjectileIndex + i) % ProjectileList.Count;
            if (!ProjectileList[index].activeInHierarchy)
            {
                nextProjectileIndex = (index + 1) % ProjectileList.Count;
                return ProjectileList[index];
            }
        }
        return null; // if no pooled object is available, return null, this will probably change in later versions
    }

    public override IEnumerator AttackPlayer()
    {
        IsDoingAction = true;
        //Sword.SetActive(true);
        agent.speed = 0;

        agent.SetDestination(Player.position); // make sure the enemy is facing the player when attacking, this will probably change in later versions
        Vector3 enPosition = transform.position;
        Vector3 playerPosition = Player.position;

        // making enemy face the player when attacking
        Vector3 AttackDirection = (playerPosition - enPosition).normalized; // get the direction from the enemy to the player
        AttackDirection.y = 0; // we aren't trying to make the enemy go up
        Vector3 LookDirection = playerPosition - enPosition; // get the direction from the enemy to the player for looking at the player
        LookDirection.y = 0; // we aren't trying to make the enemy go up
        Quaternion quaternion = Quaternion.LookRotation(LookDirection);

        int projectileChance = Random.Range(0, 100); // determine how many projectiles to shoot, this will probably change in later versions
        int projectileCount;

        if(projectileChance < 33) // determines how many projectiles will be launched
        {
            projectileCount = 1;
        }
        else if(projectileChance < 66)
        {
            projectileCount = 2;
        }
        else
        {
            projectileCount = 3;
        }

        GameObject projectile = GetPooledProjectile(); 

        for(int i = 0; i < projectileCount; i++)
        {
            transform.rotation = quaternion;
            //The projectile
            //GameObject projectile = GetPooledProjectile(); 
            if(projectile != null)
            {
                projectile.transform.position = transform.position + AttackDirection; // spawn the projectile in front of the enemy, this will probably change in later versions
                projectile.transform.rotation = quaternion; // make the projectile face the player, this will probably change in later versions
                projectile.SetActive(true); // activate the projectile, this will probably change in later versions

                projectile.GetComponent<Projectile>().Launch(AttackDirection); // launch the projectile in the direction of the player, this will probably change in later versions
            }
        }
        

        //Shoot();

        yield return new WaitForSeconds(AttackDuration);

        projectile.SetActive(false);
        IsDoingAction = false;

        if(AttackPhase)
        {
            ImmediateAction();
        }
        else
        {
            currentState = EnemyState.Chase; // if player is out of attack phase range, chase him again, this will probably change in later versions
        }
    }



    #endregion

    #region Strafe Code
    public override IEnumerator StrafeLeft()
    {
        // in this version of the code, the enemy does two fast dashes in the direction of the strafe while shooting a projectile.
        agent.speed = speed2;

        for (int i = 0; i < 2; i++)
        {
            agent.ResetPath(); // stop the enemy from moving towards the player, this will probably change in later versions
            Vector3 enPosition = transform.position;
            Vector3 playerPosition = Player.position;
            
            var OffsetPlayer = enPosition - playerPosition; // get the direction from the enemy to the player
            var StrafeDirection = Vector3.Cross(OffsetPlayer, Vector3.up);

            float strafeDashTime = 0.3f; // determines how long the dash is.
            

            while(strafeDashTime > 0)
            {
                agent.velocity = StrafeDirection.normalized * speed2; // set the velocity to the left of the player, this will probably change in later versions

                //Debug.Log(agent.velocity); // for testing purposes, to make sure the velocity is correct
            
                //look at player code
                lookPos = playerPosition - enPosition; // keeps the enemy looking at the player
                lookPos.y = 0; // we aren't trying to make the enemy go up
                rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                strafeDashTime -= Time.deltaTime;
                yield return null;
            }

            agent.velocity = Vector3.zero; // stop the enemy after the dash is done
            yield return new WaitForSeconds(0.2f); 

        }


        IsDoingAction = false;
        ImmediateAction();
    }

    public override IEnumerator StrafeRight()
    {   

    
        agent.speed = speed2;
        for (int i = 0; i < 2; i++)
        {
            agent.ResetPath();
            Vector3 enPosition = transform.position;
            Vector3 playerPosition = Player.position;
            
            var OffsetPlayer = playerPosition - enPosition; // get the direction from the enemy to the player
            var StrafeDirection = Vector3.Cross(OffsetPlayer, Vector3.up);

            float strafeDashTime = 0.3f; // determines how long the dash is.
            

            while(strafeDashTime > 0)
            {
                agent.velocity = StrafeDirection.normalized * speed2; // set the velocity to the left of the player, this will probably change in later versions

                //Debug.Log(agent.velocity); // for testing purposes, to make sure the velocity is correct
            
                //look at player code
                lookPos = playerPosition - enPosition; // keeps the enemy looking at the player
                lookPos.y = 0; // we aren't trying to make the enemy go up
                rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                strafeDashTime -= Time.deltaTime;
                yield return null;
            }
            
            
            agent.velocity = Vector3.zero; // stop the enemy after the dash is done
            yield return new WaitForSeconds(0.2f); // wait for the next frame before continuing the loop

        }

        IsDoingAction = false;
        ImmediateAction();
    }
    #endregion
}
