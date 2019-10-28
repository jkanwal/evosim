using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMotionParams : MonoBehaviour
{
    //Genetic parameters
    public float minSpeed; 
    public float maxSpeed;
    public float rotationRange = 120;  // How far should the object rotate to find a new direction?


    //General parameters
    private float timeVar = 0;
    private float step = Mathf.PI / 60;
    private Vector3 randomDirection;    // Random, constantly changing direction from a narrow range for natural motion
    private float speed;    // speed is a constantly changing value from the random range of minSpeed and maxSpeed 

    void FixedUpdate()
    {
        randomDirection = new Vector3(0, Mathf.Sin(timeVar) * (rotationRange / 2), 0); // Moving at random angles 
        timeVar += step;
        speed = Random.Range(minSpeed, maxSpeed);
        GetComponent<Rigidbody>().AddForce(transform.forward * speed);
        transform.Rotate(randomDirection * Time.deltaTime * 10.0f);
    }
}

