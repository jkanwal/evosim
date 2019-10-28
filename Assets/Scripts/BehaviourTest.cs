using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTest : MonoBehaviour
{

    //load in genome, materials, etc.
    public Genome genome;
    public Material grabberColour;
    public Material stingerColour;

    //Declare variables for motion
    private float timeVar = 0;
    private float step = Mathf.PI / 60;
    private float minSpeed;
    private float maxSpeed;
    private float rotationRange;

    //Declare variables for raycasting & grabbing
    private int layerMask = 1 << 8;
    private RaycastHit hit;
    private Vector3 target;
    private Vector3 offset = new Vector3(0f, 2f, 0f);



    void Start()
    {
        //Assign leg colour based on whether it is a stinger or grabber
        LegColour(1);
        LegColour(2);
        LegColour(3);
        LegColour(4);
        LegColour(5);
        LegColour(6);

        //Get min and max speed, and rotation range, from genome
        minSpeed = gameObject.GetComponent<Genome>().minSpeed;
        maxSpeed = gameObject.GetComponent<Genome>().maxSpeed;
        rotationRange = gameObject.GetComponent<Genome>().rotationRange;

    }

    void FixedUpdate()
    {
        MotionController(); //defines all the different conditions for different types of motion

    }

    //What happens when you collide with an object?  
    void OnCollisionEnter(Collision collision)
    {
        //If it's with the stinger, decommission it
        if ((gameObject.CompareTag("StingTargeting") || gameObject.CompareTag("StingTargeting_G")) && !collision.gameObject.CompareTag("Walls"))
        {
            if (collision.gameObject.CompareTag("Pick Up"))
            {
                collision.gameObject.GetComponent<Renderer>().material = stingerColour;
                Destroy(collision.gameObject.GetComponent<Rotator>());
            }
            else
            {
                Renderer[] children;
                children = collision.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in children)
                {
                    rend.material = stingerColour;
                }
            }
            Rigidbody rBody = collision.gameObject.GetComponent<Rigidbody>();
            rBody.isKinematic = true;
            rBody.detectCollisions = false;
            collision.gameObject.tag = "Inert";
            Debug.Log("Destroyed " + collision.gameObject.ToString());
            if (gameObject.CompareTag("StingTargeting_G")) {
                gameObject.tag = "Grabbing";
            }
            else
            {
                gameObject.tag = "Creature";
            }
            
        }
        //If it's with the grabber, grab it
        else if (gameObject.CompareTag("GrabTargeting") && !collision.gameObject.CompareTag("Walls"))
        {
            Rigidbody rBody = collision.gameObject.GetComponent<Rigidbody>();
            rBody.isKinematic = true;
            rBody.detectCollisions = false;
            collision.transform.SetParent(transform);
            collision.transform.localPosition = offset;
            collision.gameObject.tag = "Inert";
            gameObject.tag = "Grabbing";
        }
        else if (gameObject.CompareTag("StingTargeting_G") && collision.gameObject.CompareTag("Walls"))
        {
            gameObject.tag = "Grabbing";
        }
        else if ((gameObject.CompareTag("GrabTargeting") || gameObject.CompareTag("StingTargeting")) && collision.gameObject.CompareTag("Walls"))
        {
            gameObject.tag = "Creature";
        }
    }


    //Function to asign leg colour based on genes
    void LegColour(int LegID)
    {
        int[] LegGenes = GetComponent<Genome>().LegFunction;
        int LegGene = LegGenes[LegID-1];
        string LegName = "Leg" + LegID.ToString();
        GameObject Leg = transform.Find(LegName).gameObject;
        if (LegGene == 1)
        {
            Leg.GetComponent<Renderer>().material = grabberColour;
        }
        else if (LegGene == 2)
        {
            Leg.GetComponent<Renderer>().material = stingerColour;
        }
    }

    //Function that defines all the possible states of motion
    void MotionController()
    {
        float speed = Random.Range(minSpeed, maxSpeed); //set a random speed for each time step

        //if I'm being grabbed by someone else, or if I'm dead, do nothing
        if (gameObject.CompareTag("Inert"))
        {
            Rigidbody rBody = gameObject.GetComponent<Rigidbody>();
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }
        //if I'm not busy doing anything else, check whether anything in line of sight of grabber, and switch tag to GrabTargeting
        else if (gameObject.CompareTag("Creature") && Physics.Raycast(transform.position, transform.up, out hit, Mathf.Infinity, layerMask))
        {
            gameObject.tag = "GrabTargeting";
            target = hit.point;
        }
        //if nothing in line of sight of grabber, check if anything in line of sight of stinger
        else if ((gameObject.CompareTag("Creature") || gameObject.CompareTag("Grabbing")) && Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            if (gameObject.CompareTag("Creature"))
            {
                gameObject.tag = "StingTargeting";
            }
            else
            {
                gameObject.tag = "StingTargeting_G";
            }
            target = hit.point;
        }
        //if there's a detected target, move in the direction of target until you collide with it
        else if (gameObject.CompareTag("GrabTargeting") || gameObject.CompareTag("StingTargeting") || gameObject.CompareTag("StingTargeting_G"))
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.fixedDeltaTime * speed);
        }
        //else, move randomly
        else
        {
            Vector3 randomDirection = new Vector3(0, Mathf.Sin(timeVar) * (rotationRange / 2), 0); // Moving at random angles 
            timeVar += step;
            GetComponent<Rigidbody>().AddForce(transform.forward * speed);
            transform.Rotate(randomDirection * Time.fixedDeltaTime * 10.0f);
        }
    }

    //Function that defines grabbing behaviour
    void Grab(Collision collision)
    {

    }

    //Function that defines stinging behaviour
    void Sting(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pick Up"))
        {
            collision.gameObject.GetComponent<Renderer>().material = stingerColour;
            Destroy(collision.gameObject.GetComponent<Rotator>());
        }
        else
        {
            Renderer[] children;
            children = collision.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in children)
            {
                rend.material = stingerColour;
            }
        }
        Rigidbody rBody = collision.gameObject.GetComponent<Rigidbody>();
        rBody.isKinematic = true;
        rBody.detectCollisions = false;
        collision.gameObject.tag = "Inert";
        Debug.Log("Destroyed " + collision.gameObject.ToString());
        if (gameObject.CompareTag("StingTargeting_G"))
        {
            gameObject.tag = "Grabbing";
        }
        else
        {
            gameObject.tag = "Creature";
        }
    }


}
