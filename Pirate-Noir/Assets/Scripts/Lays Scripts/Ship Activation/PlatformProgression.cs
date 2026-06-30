using UnityEngine;

// public so other scripts can reference and call it
public class PlatformProgression : MonoBehaviour
{

#region Inspector

///
/// Empty space to organize region headers
///


#region Ship Progression




    [Header("Ship Progression Settings: Hover over each variable for more info")]
    [Header(" ")]

    [Tooltip("Number of kills needed per ship to activate the next ship.")]
    [SerializeField] private int killsNeeded = 2;
    //
    [Tooltip("The next ship that will come onto the scene after the required kills are met.")]
    [SerializeField] private GameObject nextObjectToActivate;

#endregion
#region Ship Sink

    [Header(" ")]
    [Header("Ship Sink Settings: Hover over each variable for more info")]
    [Header(" ")]

    [Tooltip("The current ship game object.")]
    [SerializeField] private Transform platformToRotate;
    //
    [Tooltip("Speed at which the ship rotates when activated.")]
    [SerializeField] private float rotationSpeed = 1f;
    //
    [Tooltip("The ending angle of the sinking ship, it will start sinking after reaching this point.")]
    [SerializeField] private float maxRotation = -60f;
    //
    [Tooltip("Speed at which the ship sinks.")]
    [SerializeField] private float sinkSpeed = 2f;
    //
    [Tooltip("The point on the ship that is checked to see if it has sunk below the y value of -1f. This should be the highest point of the ship.")]
    [SerializeField] private Transform sinkCheckPoint;
    //
    [Tooltip("Entire Platform Manager Game object parent. This will despawn once the ship has been fully sunk.")]
    [SerializeField] private GameObject platformManager;


#endregion
#region Game Win

    [Header(" ")]
    [Header("Game Win Settings: Hover over each variable for more info")]
    [Header(" ")]

    // Automatically false
    [Tooltip("Check this box if this is the final ship to destroy.")]
    [SerializeField] private bool finalPlatform;
    //
    [Tooltip("Victory ship parent game object.")]
    [SerializeField] private GameObject winCondition;

#endregion
#region Enemy

    [Header(" ")]
    [Header("Enemy Settings: Hover over each variable for more info")]
    [Header(" ")]

    // Stores references to every enemy with the PlatformCount script in an array
    // This will be used later to make the enemies fall when the ship starts sinking.
    [SerializeField] private PlatformCount[] enemies;

    // This will allow the EnemyGenerator script to see if this is the final ship, so it can spawn more enemies.
     public bool FinalPlatform => finalPlatform;



#endregion
#endregion
#region Variables

    // Keeps track of the number of kills the player has achieved
    private int currentKills = 0;
    //
    // Prevents the ship from being activated multiple times
    private bool activated = false;
    //
    // Controls whether the ship should rotate after activation
    private bool rotatePlatform = false;
    //
    // Platform will sink when this is true
    private bool sinkPlatform = false;
    //
    // Variable to store the starting position of the platform to calculate sinking distance
    private Vector3 startPosition;

#endregion
#region Start

    private void Start()
    {
        // Kills start at 0 at game start
        //Debug.Log("Kills start = " + currentKills);

        // Store the starting position of the platform to calculate sinking distance
        startPosition = platformToRotate.position;
        // Finds all gambojects with the PlatformCount script. This will just be the enemies. Stores them in an array (enemies)
        enemies = FindObjectsByType<PlatformCount>();
    }

#endregion
#region Update

    // Runs every frame
    private void Update()
    {

        // Run this if the ship is currently rotating
        if (rotatePlatform)
        {
            // Ships current rotation stored in currentRotation
            Vector3 currentRotation = platformToRotate.eulerAngles;

            float xRotation = currentRotation.x;


            // Unity Rotations are stored in 360 degrees. -60 degrees = 300 degrees.
            // Conversion
            if (xRotation > 180f)
                xRotation -= 360f;

            // Stop rotating at -60 degrees.
            if (xRotation > -60f)
            {
                // X axis
                platformToRotate.Rotate(-rotationSpeed * Time.deltaTime, 0f, 0f);
            }
            // Stop rotation
            else
            {
                // Stops update
                rotatePlatform = false;
                // Current rotation
                Vector3 finalRotation = platformToRotate.eulerAngles;
                // Snaps to angle
                finalRotation.x = maxRotation;

                platformToRotate.eulerAngles = finalRotation;

                // This ship is now sinking!
                sinkPlatform = true;
            }
        }

        if (sinkPlatform)
        {
            platformToRotate.position += Vector3.down * sinkSpeed * Time.deltaTime;

            float sunkDistance =
                startPosition.y - platformToRotate.position.y;

            // If the highest point of the ship is below -1f on the y axis, destroy the ship.
            if (sinkCheckPoint.position.y <= -1f)
            {
                // Destroys the game object assigned. This should be the platform manager parent game object.
                Destroy(platformManager);

            }
        }
    }
#endregion
#region Enemy Defeated

    // Called when an enemy dies. Another script can call this because it is public.
    public void EnemyDefeated()
    {
        // If the next ship has already been activated.
        if (activated)
            return;

        // Increase kill count by 1
        currentKills++;

        // Have enough enemies dies to activate the next ship?
        if (currentKills >= killsNeeded)
        {
            // Prevents this from being called twice.
            activated = true;

            // This will run once the required kills have been met on the last ship. It will activate the winning ship.
            if (finalPlatform)
            {
                winCondition.SetActive(true);
            }
            else
            {
                // Turns on the game object, the next ship
                nextObjectToActivate.SetActive(true);
            }

            //PlatformProgression nextPlatform = nextObjectToActivate.GetComponent<PlatformProgression>();

            //nextPlatform.SetDifficulty(killsNeeded + 1,rotationSpeed + 1f);


            // Starts rotating the ship
            rotatePlatform = true;

            // Loops through each enemy in the array.
            foreach (var enemy in enemies)
            {
                // Runs once for each enemy
                // Allows the enemies to fall
                enemy.ActivatePhysics();
            }
        }
    }
#endregion
#region Set Difficulty

    public void SetDifficulty(int newKillsNeeded,float newRotationSpeed)
    {
        killsNeeded = newKillsNeeded;
        rotationSpeed = newRotationSpeed;
    }

#endregion
}
