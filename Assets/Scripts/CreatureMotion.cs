using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class CreatureMotion : MonoBehaviour
{

    //Declare variables for motion
    private float speedOrigin;
    private float angleOrigin;
    private float minSpeed;
    private float maxSpeed;
    private float rotationRange;
    private Rigidbody rBody;
    private Collider[] childrenColliders;

    public int Ticks;

    void Start()
    {
        //Get min and max speed, rotation range, and grabPref from genome
        minSpeed = gameObject.GetComponent<Genome>().minSpeed;
        maxSpeed = gameObject.GetComponent<Genome>().maxSpeed;
        rotationRange = gameObject.GetComponent<Genome>().rotationRange;
        
        rBody = GetComponent<Rigidbody>(); //Get Rigid body component just once at Start, as this is an expensive operation

        //Ignore collisions with its own legs
        childrenColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in childrenColliders)
        {
            Physics.IgnoreCollision(col, GetComponent<Collider>());
        }

        //Set speed and angle origins for perlin noise
        speedOrigin = Random.Range(0f, 10000f);
        angleOrigin = Random.Range(0f, 2*Mathf.PI);

        Ticks = 0;
    }

    void FixedUpdate() 
    {
        Ticks += 1; //count up a Tick at each physics update

        //if I'm dead, or bring grabbed by someone else, do nothing
        if (gameObject.CompareTag("Inert") || gameObject.CompareTag("Grabbed"))
        {   
            rBody.velocity = Vector3.zero;
            //rBody.angularVelocity = Vector3.zero;
        } 
        else
        {
            //random motion
            float speed = maxSpeed * Mathf.PerlinNoise(speedOrigin + Ticks, 0.0f); //set a speed for this time step using perlin noise
            Vector3 randomDirection = new Vector3(0, Mathf.Sin(angleOrigin + Ticks) * rotationRange, 0); //keep rotating smoothly
            transform.Rotate(randomDirection * Ticks);
            rBody.AddForce(transform.forward * speed * 5f); //can double the speed to make more interactions happen in the same number of physics ticks?   
        }

    }

}

