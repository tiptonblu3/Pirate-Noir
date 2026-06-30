using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BigEnemy : Enemy
{
    public int poolSize = 10;
    public GameObject AOECube;
    public List<GameObject> AOElist = new List<GameObject>(); // list of the AOE objects to pool

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start(); // call the base start method to initialize the enemy

        // Object pooling for projectiles
        for(int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(AOECube);
            obj.SetActive(false);
            AOElist.Add(obj);
        }
    }

    // Update is called once per frame

    #region AOE Attack Code
    public GameObject GetPooledAOE()
    {
        for (int i = 0; i < AOElist.Count; i++)
        {
            if (!AOElist[i].activeInHierarchy)
            {
                return AOElist[i];
            }
        }
        return null; // if no pooled object is available, return null, this will probably change in later versions
    }

    public void SpawnAOE(Vector3 position, Vector3 direction) //helps with spawning the object for the AOE
    {
        GameObject obj = GetPooledAOE();
        if(obj != null)
        {
            obj.transform.position = position;
            
            obj.SetActive(true);

            obj.GetComponent<AOEAttack>().Launch(direction); // launch the AOE in the direction the enemy is facing, this will probably change in later versions
            
        }
    }

    public override IEnumerator AttackPlayer()
    {
        IsDoingAction = true;
        
        agent.speed = speed;
        agent.ResetPath(); // stop the enemy from moving, this will probably change in later versions  

        Vector3 center = transform.position; // get the center of the enemy for the AOE
        Vector3 front = transform.forward; // get the front of the enemy for the AOE, this will probably change in later versions
        Vector3 right = transform.right; // get the right of the enemy for the AOE, this will probably change in later versions

        SpawnAOE(center + front * 2, front);
        SpawnAOE(center - front * 2, -front); // spawn object behind enemy
        SpawnAOE(center + right * 2, right); // spawn object to the right of the enemy
        SpawnAOE(center - right * 2, -right); // spawn object to the left
        

        yield return new WaitForSeconds(AttackDuration);

        
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

    public override void ImmediateAction()
    {
        int choice = Random.Range(0, 100);
        
        if(AttackPhase)
        {
            if(choice < 40)
            {
                currentState = EnemyState.StrafeRight;
            }
            else if (choice < 80)
            {
                currentState = EnemyState.StrafeLeft;
            }
            else 
            {
                currentState = EnemyState.Attack;
            }

            Debug.Log("Picked: " + currentState);
            /* don't have the other things implemented yet.
            else if (choice < 75)
            {
                currentState = EnemyState.Approach;
            }
            else
            {
                currentState = EnemyState.StepBack;
            }
            */
        }
    }


}
