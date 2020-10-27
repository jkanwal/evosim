using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class HostMotion : MonoBehaviour
{
    //Set mutation rate
    public float mutationRate = 0.5f;

    //Declare variables for motion
    public float minSpeed = 10f;
    public float maxSpeed = 50f;
    public float rotationRange = 45f;
    private float speedOrigin;
    private float angleOrigin;
    private Rigidbody rBody;
    private int Ticks;

    void Start()
    {
        rBody = GetComponent<Rigidbody>(); //Get Rigid body component just once at Start, as this is an expensive operation

        //Set speed and angle origins for perlin noise
        speedOrigin = Random.Range(0f, 10000f);
        angleOrigin = Random.Range(0f, 2*Mathf.PI);

        Ticks = 0;

        //if host is infected
        //{ChangeColour}
    }

    void FixedUpdate() 
    {
        Ticks += 1;

        //random motion (using perlin noise -- a type of natural-looking pattern of randomness)
        float speed = maxSpeed * Mathf.PerlinNoise(speedOrigin + Ticks, 0.0f); //set a speed for this time step using perlin noise
        Vector3 randomDirection = new Vector3(0, Mathf.Sin(angleOrigin + Ticks) * rotationRange, 0); //keep rotating smoothly
        transform.Rotate(randomDirection * Ticks);
        rBody.AddForce(transform.forward * speed * 5f);   
    }

    //If I get close enough to another host, and I have the virus but they don't, then I can transmit it to them with some chance x (equal to the virulence of my infection)
    void OnCollisionEnter(Collision other) 
    {
        if (GetComponent<Virus>().infected == true && other.gameObject.CompareTag("Host")) 
        {
            if (other.gameObject.GetComponent<Virus>().infected == false)
            {
                float myVirulence = GetComponent<Virus>().virulence; //get the virulence value of my infection
                float rand0 = Random.value; //returns a random number between 0 and 1
                //if this random number is less than or equal to the transmission rate (i.e. the virulence + 0.5) of my infection, then transmit the virus!
                if (rand0 <= myVirulence + 0.5)
                {
                    other.gameObject.GetComponent<Virus>().infected = true; //infect other host
                    //Debug.Log("infection event!");
                    float otherVirulence; //this will hold the value of virulence assigned to other host
                    //set other host's virulence value to the same as mine, with some chance of mutation
                    float rand1 = Random.value;
                    if (rand1 <= mutationRate)
                    {
                        //for this version of incremental mutation, we just add one small number to the parent's virulance
                        otherVirulence = myVirulence + 0.001f; //

                        //for random mutation, we just set the new virulance to some random number
                        //otherVirulence = Random.value; 

                        other.gameObject.GetComponent<Virus>().virulence = otherVirulence;
                        //Debug.Log("Mutation!");
                    }
                    else
                    {
                        otherVirulence = myVirulence; //set it to my virulence value
                        other.gameObject.GetComponent<Virus>().virulence = otherVirulence;
                    }
                    //Debug.Log("otherVirulence: " + otherVirulence.ToString());
                    //adjust other host's death rate
                    other.gameObject.GetComponent<LifeCycle>().myDeathRate += otherVirulence; // (the += symbol means add this value to the original value)
                    //change other host's colour according to their virulence              
                    other.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.5f + otherVirulence*50f, 0.5f - otherVirulence*50f, 0f);
                }
            } 
        }   
    }

}

