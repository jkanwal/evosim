using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : MonoBehaviour
{

    //load in genome & prefabs
    public Genome genome;
    public GameObject CreaturePrefab;
    public GameObject FoodPrefab;
    

    //General parameters needed for motion
    private float timeVar = 0;
    private float step = Mathf.PI / 60;
    private Vector3 randomDirection; 
    private float speed;
    private int layerMask = 1 << 8;
    private RaycastHit hit;
    private Vector3 target;

    //General simulation parameters
    public AntennaSteering antennaSteering;
    public GrabFood GrabFood;
    public int foodAmount = 20;
    public float foodProb = 0.001f;
    public float minimumHeight = 2f;
    public float maximumHeight = 7f;
    public float minSpeed = 0f;
    public float maxSpeed = 35f;
    public float rotationRange = 80f;
    public int creatureNum = 4;
    public float resetRate = 30f;
    private float resetTime;
    //public float mutationRate = 0.1;

    void Start()
    {
        //Build creature from genome


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
            randomDirection = new Vector3(0, Mathf.Sin(timeVar) * (rotationRange / 2), 0); // Moving at random angles 
            timeVar += step;
            GetComponent<Rigidbody>().AddForce(transform.forward * speed);
            transform.Rotate(randomDirection * Time.deltaTime * 10.0f);
        }

    }
}
