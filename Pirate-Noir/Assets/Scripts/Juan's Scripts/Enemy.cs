using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;



public class Enemy : MonoBehaviour
{
    // every enemy script will inherit from this big boy

    #region Variables
    public float health;
    public float maxHealth;
    public int damage;
    public float speed; // When enemy gets close, speed will slow it down.
    // will put more settings later to customize speed in different phases, for now just the attack will change the speed.
    public float speed2;

    public NavMeshAgent agent;
    public Transform Player;
    public PlayerStats player; // to use the health values from that code

    
    [Header("Attack Settings")]
    public Transform Attack; // enemies themselves won't hurt you but they will have attacks that do
    public GameObject Sword; // Model that will get disabled after attack and enabled when attacking.
    public float attackCooldown = 2f;

    //TEMPORARY ADITION
    public float AttackDuration = 1f;
    //In future versions the animator will play a role, for now this will do.


    /*public Animator animator; // to play attack animations, will be used in later versions
    public bool isAttacking = false; // to prevent the enemy from attacking multiple times in a row without cooldown, will be used in later versions*/
    
    
    [Header("Distance Check")]
    private float Distance; // to optimize distance checking
    public float AttackPhaseSqrRange; // squared range for attack phase
    public float FarSqrRange; // squared range for far range
    public float DetectRange = 8f; // the range from which the enemy will detect and attack the player.
    public float FarRange = 15f;
    public float AttackPhaseRange = 5f;
    //public float distance;

    public float stopAttackRange = 20f;
    public float stopAttackSqrRange;


    [Header("Behavior Settings")]
    public bool AttackPhase = false;
    public float BehaviorTimer = 5f;
    public bool ChoosingBehavior = false;
    public bool IsDoingAction = false;
    
    private Coroutine behaviorCoroutine;

    [Header("Roam Settings")]
    public float roamRadius = 10f; 
    public float roamTimer = 5f; // time in seconds between each roam action
    private float roamTime;
    public Vector3 roamPosition;

    [Header("Strafe Settings")]
    public float rotationSpeed = 5f;
    public Vector3 lookPos;
    public Quaternion rotation;
    public float strafeSpeed = 4f;

    public EnemyWaves enemyWaves;
    public EnemyState currentState; // to determine the current state of the enemy, will be used in later versions to make the enemy do different things based on the state.
    #endregion


    #region Start and Update
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        maxHealth = health;
        Sword.SetActive(false); // sword is disabled at the start, will be enabled when attacking, this will probably change in later versions
        
        AttackPhaseSqrRange = AttackPhaseRange * AttackPhaseRange;
        FarSqrRange = FarRange * FarRange;
        stopAttackSqrRange = stopAttackRange * stopAttackRange;

        currentState = EnemyState.Idle; // enemy starts in idle state, this will probably change in later versions

    }

    // Enemy will have two "phases", the roam and the attack phase
    /*
    Behaviors I still need to do implement
        - Approach (might make this one connect TO the attack)
        - Step back

    I also want to find a way in which enemies have a smaller chance of repeating an action the more they do it.
    */

    // Update is called once per frame
    void Update()
    {
        Distance = (this.transform.position - Player.position).sqrMagnitude;
        
        //distance = Vector3.Distance(this.transform.position, Player.position); // distance which will be used to measure and trigger enemy attacks

        //agent.SetDestination(Player.position);
        
        
        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Roam:
                Roam();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            
            
            case EnemyState.StrafeLeft:
                if (!IsDoingAction)
                {
                    IsDoingAction = true;
                    StartCoroutine(StrafeLeft());
                }
                //StrafeLeft();
                break;
            case EnemyState.StrafeRight:
                if (!IsDoingAction)
                {
                    IsDoingAction = true;
                    StartCoroutine(StrafeRight());
                }
                //StrafeRight();
                break;
            case EnemyState.Attack:
                //StartCoroutine(AttackPlayer());
                if(!IsDoingAction)
                {
                    IsDoingAction = true;
                    StartCoroutine(AttackPlayer());
                    //ImmediateAction();
                    // To circumvent the coroutine and prevent chase from never ending.
                    // TEMPORARY SOLUTION.
                }
                

            break;
            
        }

        if(Distance <= AttackPhaseSqrRange)
        {
            AttackPhase = true;
            //IsDoingAction = false; // this will allow the enemy to choose an attack when entering attack phase, this will probably change in later versions
        }

        else if(Distance > stopAttackSqrRange)
        {
            AttackPhase = false;
            
        }
        
        if (health <= 0f)
        {
            EnemyDied();
        }

    }
    #endregion


    #region Behaviors
    public void Idle()
    {
        // idle animation probably goes here.

        if(Distance <= FarSqrRange)
        {
            currentState = EnemyState.Chase; // if player is within far range, enemy will start chasing him, this will be used in later versions to make the enemy do different things based on the state.
        }

        if (!ChoosingBehavior)
        {
            behaviorCoroutine = StartCoroutine(BehaviorChoice()); // will choose a behavior after a certain amount of time, this will probably change in later versions
        }
    }
    
    public void Roam()
    {
        // code to make the enemy walk around in a few selected areas at random from a specific distance from the enemy, will be used in later versions
        roamTime += Time.deltaTime;
        agent.speed = speed2;

        // If enough time passed OR reached destination
        if (roamTime >= roamTimer || (this.transform.position - roamPosition).sqrMagnitude < 4f) 
        {
            roamPosition = RandomDirection();

            agent.SetDestination(roamPosition);

            roamTime = 0f;
        }
        
        if(!ChoosingBehavior)
        {
            behaviorCoroutine = StartCoroutine(BehaviorChoice()); // will choose a behavior after a certain amount of time, this will probably change in later versions
            /* 
            setting the coroutine like this makes it possible to stop it as well, which is why I went with this
            instead of just StartCoroutine(BehaviorChoice());
            */
        }

        if(Distance <= FarSqrRange)
        {
            currentState = EnemyState.Chase; // Same thing as the idle .
        }
    }

    Vector3 RandomDirection() // Method that chooses the direction the enemy goes to.
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius; // direction is random, but limited within the radius.

        randomDirection += transform.position; 

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }
    
    public void ChasePlayer()
    {
        if(behaviorCoroutine != null)
        {
            StopCoroutine(behaviorCoroutine); // stop choosing behavior when chasing player, this MIGHT change later.
            behaviorCoroutine = null;
        }

        // once player is detected, follow him, duh
        agent.SetDestination(Player.position);
        agent.speed = speed2;
        if(AttackPhase && !IsDoingAction)
        {
            ImmediateAction();
            
            /*if (!ChoosingBehavior)
            {
                behaviorCoroutine = StartCoroutine(BehaviorChoice());
            }

            return; // so that the enemy stops chasing
            */
        }

        /*if (distance <= DetectRange) // player is closer, enemy is faster
        {
            agent.speed = speed;
        }*/

        // No behavior choice here, I want the player to either lose the enemy or have him get close and reaching attack phase.
    }

    public virtual void EnemyDied()
    {
        enemyWaves.ActiveEnemies--;
        StopAllCoroutines();
        if(enemyWaves.ActiveEnemies < 0)
        {
            enemyWaves.ActiveEnemies = 0;
        }
        
        //Should I set it active or destroy the gameObject?
        health = maxHealth;

        this.gameObject.SetActive(false);
        
    }
    #endregion

    #region AttackCode
    public virtual IEnumerator AttackPlayer() //maybe change to coroutine later.
    {
        //IsDoingAction = true;
        // code to make enemy attack player, due to a lack of animation:
        // enemy will, for now, rush towards the player
        IsDoingAction = true;
        Sword.SetActive(true);
        agent.speed = speed;
        agent.SetDestination(Player.position); // if strafing, there's a chance enemy might stay circling the player, so I'm putting this here.    
        
        yield return new WaitForSeconds(AttackDuration);

        Sword.SetActive(false);
        IsDoingAction = false;

        if(AttackPhase)
        {
            ImmediateAction();
        }
        else
        {
            currentState = EnemyState.Chase; // if player is out of attack phase range, chase him again, this will probably change in later versions
        }

        /*if(!ChoosingBehavior)
        {
            behaviorCoroutine = StartCoroutine(BehaviorChoice());
        }*/

        //IsDoingAction = false;
    }
    
    public void DamagePlayer()
    {
        StartCoroutine(AttackCooldown());
        // Play attack animation or sound here if needed
        player.CurrentHealth -= damage;

        if (behaviorCoroutine != null)
        {
            //StopCoroutine(behaviorCoroutine); //Only Stop Coroutine if player is damaged.
            behaviorCoroutine = null;
        }
    }

    public IEnumerator AttackCooldown()
    {
        Sword.SetActive(false); // sword disappears when in cooldown, this will probably change in later versions
        yield return new WaitForSeconds(attackCooldown);

        if (!ChoosingBehavior)
        {
            behaviorCoroutine = StartCoroutine(BehaviorChoice());
        }

        // Sword.SetActive(true); // will make a function later to make the player be detected after entering a certain range, making the sword spawn in.
    }


    #endregion

    #region StrafeCode
    public virtual IEnumerator StrafeLeft()
    {
        float timer = 0f;
        agent.speed = speed2;

        while (timer < 2f)
        {
            // old version of the loop calls player position and transform position repeatedly, this is a more optimized version
            // Unity does not have to call for the transform and player position multiple times in a single frame anymore as it calls for this instead.
            Vector3 enPosition = transform.position;
            Vector3 playerPosition = Player.position;
            
            var OffsetPlayer = enPosition - playerPosition; // get the direction from the enemy to the player
            var StrafeDirection = Vector3.Cross(OffsetPlayer, Vector3.up);
            
            agent.SetDestination(enPosition + StrafeDirection); // set the destination to the left of the player, this will probably change in later versions
            lookPos = playerPosition - enPosition; // keeps the enemy looking at the player
            lookPos.y = 0; // we aren't trying to make the enemy go up
            rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            //timer += Time.deltaTime;
            
            // timer changed so it doesn't set a destination every frame.
            yield return new WaitForSeconds(0.2f); 
            timer += 0.2f;

            /*
            OLD Version:
            var OffsetPlayer = transform.position - Player.position; // get the direction from the enemy to the player
            var StrafeDirection = Vector3.Cross(OffsetPlayer, Vector3.up);
            agent.SetDestination(transform.position + StrafeDirection); // set the destination to the left of the player, this will probably change in later versions
            lookPos = Player.position - transform.position; // keeps the enemy looking at the player
            lookPos.y = 0; // we aren't trying to make the enemy go up
            rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
            */
        }


        IsDoingAction = false;
        ImmediateAction();

        /*if(!ChoosingBehavior)
        {
            behaviorCoroutine = StartCoroutine(BehaviorChoice());
        }*/
    }

    public virtual IEnumerator StrafeRight()
    {   
        float timer = 0f;
        agent.speed = speed2;

        while (timer < 2f)
        {
            Vector3 enPosition = transform.position;
            Vector3 playerPosition = Player.position;
            
            var OffsetPlayer = playerPosition - enPosition; // get the direction from the enemy to the player
            var StrafeDirection = Vector3.Cross(OffsetPlayer, Vector3.up);
            
            agent.SetDestination(enPosition + StrafeDirection); // set the destination to the left of the player, this will probably change in later versions
            lookPos = playerPosition - enPosition; // keeps the enemy looking at the player
            lookPos.y = 0; // we aren't trying to make the enemy go up
            rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            //timer += Time.deltaTime;
            yield return new WaitForSeconds(0.2f); // wait for the next frame before continuing the loop
            timer += 0.2f;

            /*
            OLD Version:
            var OffsetPlayer = Player.position - transform.position; // get the direction from the enemy to the player
            var StrafeDirection = Vector3.Cross(OffsetPlayer, Vector3.up);
            agent.SetDestination(transform.position + StrafeDirection); // set the destination to the left of the player, this will probably change in later versions
            lookPos = Player.position - transform.position; // keeps the enemy looking at the player
            lookPos.y = 0; // we aren't trying to make the enemy go up
            rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
            */
        }


        IsDoingAction = false;
        ImmediateAction();

        /*if(!ChoosingBehavior)
        {
            behaviorCoroutine = StartCoroutine(BehaviorChoice());
        }*/
    }
    #endregion

    #region BehaviorChoiceCode
    public enum EnemyState
    {
        Idle, // literally does nothing, just stands in idle.
        Roam, // walks around in a few selected areas at random from a specific distance from the enemy.
        Chase,
        Attack,
        Attack2,
        Attack3, // Enemies might possess more attacks in later versions, this is just a placeholder for now.
        StrafeLeft, // circles around the enemy
        StrafeRight,
        Approach,
        StepBack

    }

    public IEnumerator BehaviorChoice()
    {
        ChoosingBehavior = true;
        
        // If the player goes far enough, make it so that after a few seconds, behavior resets to roam and/or idle.
        // This is temporary, I will change this over the course of development.

        yield return new WaitForSeconds(BehaviorTimer);
        
        int choice = Random.Range(0, 100); // Percentage chance to choose a behavior.

        if(!AttackPhase)
        {
            if (choice < 20)
            {
                currentState = EnemyState.Idle;
            }
            else // make the enemy roam more often than standing idly.
            {
                currentState = EnemyState.Roam;
            }
            
            
            /*else
            {
                currentState = EnemyState.Chase;
            }
            
            Enemy will only start chasing when player is in range, it will not trigger automatically.
            */


        }

        /*
        else if(AttackPhase)
        {
            if(choice < 25)
            {
                currentState = EnemyState.StrafeRight;
            }
            else if (choice < 50)
            {
                currentState = EnemyState.StrafeLeft;
            }
            else
            {
                currentState = EnemyState.Attack;
            }
             don't have the other things implemented yet.
            else if (choice < 75)
            {
                currentState = EnemyState.Approach;
            }
            else
            {
                currentState = EnemyState.StepBack;
            }
            
        }
        */

        

        
        ChoosingBehavior = false;
        
    }

    public virtual void ImmediateAction()
    {
        int choice = Random.Range(0, 100);
        
        if(AttackPhase)
        {
            if(choice < 25)
            {
                currentState = EnemyState.StrafeRight;
            }
            else if (choice < 50)
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

    void OnEnable()
    {
        currentState = EnemyState.Idle;
        IsDoingAction = false;
        ChoosingBehavior = false;
        AttackPhase = false;

        agent.ResetPath();
        agent.isStopped = false;
    }

    #endregion

    
}
