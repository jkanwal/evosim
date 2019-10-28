using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntennaSteering : MonoBehaviour
{
    //Genetic parameters
    public float minSpeed;
    public float maxSpeed;
    public float rotationRange = 120;  // How far should the object rotate to find a new direction?
    //public float reach = 10f; //how far does the raycast reach


    //General parameters
    private float timeVar = 0;
    private float step = Mathf.PI / 60;
    private Vector3 randomDirection;    // Random, constantly changing direction from a narrow range for natural motion
    private float speed;    // speed is a constantly changing value from the random range of minSpeed and maxSpeed 
    private int layerMask = 1 << 8;
    private RaycastHit hit;
    private Vector3 target;


    void Start()
    {
   
    }

    void FixedUpdate()
    {
        speed = Random.Range(minSpeed, maxSpeed); //set a random speed for each time step
        //check whether any food in line of sight of antenna, and switch tag to targeting
        if (!gameObject.CompareTag("Grabbing") && Physics.Raycast(transform.position, transform.up, out hit, Mathf.Infinity, layerMask))
        {
            /*
            Vector3 force = Vector3.MoveTowards(transform.position, hit.point, Time.deltaTime * speed);
            GetComponent<Rigidbody>().AddForce(force);
            */

            gameObject.tag = "Targeting";
            target = hit.point;

            //transform.position = Vector3.MoveTowards(transform.position, hit.point, Time.deltaTime * speed);
        }
        //move in the direction of that food until you grab it
        else if (gameObject.CompareTag("Targeting"))
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        }
        //else move randomly
        else
        {
            randomDirection = new Vector3(0, Mathf.Sin(timeVar) * (rotationRange/2), 0); // Moving at random angles 
            timeVar += step;
            GetComponent<Rigidbody>().AddForce(transform.forward * speed);
            transform.Rotate(randomDirection * Time.deltaTime * 10.0f);
        }

    }
}
