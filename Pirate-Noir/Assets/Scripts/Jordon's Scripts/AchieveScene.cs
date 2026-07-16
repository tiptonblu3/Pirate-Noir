using UnityEngine;
using System.Collections;

public class AchieveScene : MonoBehaviour //script to help the achievement to rotate around pivot point
{
    public GameObject pivotPoint; // Reference to the pivot point around which the object will rotate
    public float duration = 1.0f; // Duration of the rotation in seconds
    public bool Rotating = false; // Flag to indicate if the rotation is in progress
    public float RotateLeft = -90f; // Angle to rotate left
    public float RotateRight = 90f; // Angle to rotate right
    public void LeftTurn() 
    {
        if (!Rotating && pivotPoint != null)
        {
            StartRotation(RotateLeft);      
        }  
    } 

    public void RightTurn()
    {
        if (!Rotating && pivotPoint != null)
        {
            StartRotation(RotateRight);
        }
    } 

    public void StartRotation(float angle)
    {
        StartCoroutine(AnimateRotation(angle));
    }

    private IEnumerator AnimateRotation(float totalAngle)
    {
        Rotating = true;
        
        float elapsedTime = 0f;
        float DegreesRotated = 0f;

        // Keep track of the starting position and rotation to calculate the exact endpoint
        Vector3 pivotPos = pivotPoint.transform.position;
        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        // Calculate exactly where we MUST end up
        Vector3 targetPos = pivotPos + (Quaternion.AngleAxis(totalAngle, Vector3.up) * (originalPos - pivotPos));
        Quaternion targetRot = Quaternion.AngleAxis(totalAngle, Vector3.up) * originalRot;


        // Keep looping as long as we haven't reached our rotation point
        while (elapsedTime < duration)
        {
            // Add the time passed since the last frame to see if its passed the cooldown
            elapsedTime += Time.deltaTime;

            // Determine how much to rotate based on the time
            float targetrotationatthisframe = (elapsedTime / duration) * totalAngle;
            float rotationStep = targetrotationatthisframe - DegreesRotated;
            
            
            transform.RotateAround(pivotPos, Vector3.up, rotationStep); //rotates it
            DegreesRotated += rotationStep; //tracks total progress
            

            // Wait for the very next frame before continuing the loop
            yield return null;
        }
        transform.position = targetPos;
        transform.rotation = targetRot;
        

        Rotating = false;
    }




}
