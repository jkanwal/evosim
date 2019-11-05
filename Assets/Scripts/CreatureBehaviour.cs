using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CreatureBehaviour : MonoBehaviour
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
    private Vector3[] LegDirections;
    private List<Vector3> StingDirections = new List<Vector3>();
    private List<Vector3> GrabDirections = new List<Vector3>();
    private int layerMask = 1 << 9;
    //private RaycastHit hit;
    private Vector3 target;
    private float grabPref;
    private Vector3 offset = new Vector3(0f, 2f, 0f);
    private float releaseRate = 15f;
    private float releaseTime;
    private GameObject grabbee;

    //keep track of my food points
    public int points = 0;


    void Start()
    {
        //Assign leg colour based on whether it is a stinger or grabber
        LegDirections = new Vector3[] {transform.up, -transform.up, transform.right, -transform.right, transform.forward, -transform.forward};
        for (int i = 1; i < 7; i++)
        {
            LegColourRaycast(i);
        }

        //Get min and max speed, rotation range, and grabPref from genome
        minSpeed = gameObject.GetComponent<Genome>().minSpeed;
        maxSpeed = gameObject.GetComponent<Genome>().maxSpeed;
        rotationRange = gameObject.GetComponent<Genome>().rotationRange;
        grabPref = gameObject.GetComponent<Genome>().GrabberPref;

    }

    void FixedUpdate()
    {
        //if I'm grabbing food, digest it and get a point
        if (grabbee != null && grabbee.CompareTag("Pick Up"))
        {
            if (Time.time > releaseTime)
            {
                Destroy(grabbee);
                grabbee = null; //empty my grabbee variable
                points += 1;   
                //if I'm not already dead, reset my Grabbing tag
                if (!gameObject.CompareTag("Inert"))
                {
                    if (gameObject.CompareTag("Grabbing"))
                    {
                        gameObject.tag = "Creature"; 
                    }
                    else if (gameObject.CompareTag("Grabbing_G"))
                    {
                        gameObject.tag = "Grabbed";
                    }
                }
                /* (Commented out, so keeping hold of other creatures for now)
                //if I'm grabbing a creature, release it
                else
                {
                    grabbee.transform.parent = null;
                    Rigidbody rBody = grabbee.GetComponent<Rigidbody>();
                    rBody.isKinematic = false;
                    rBody.detectCollisions = true;
                    grabbee.tag = "Creature";
                }
                gameObject.tag = "Creature"; //reset my Grabbing tag
                grabbee = null; //empty my grabbee variable
                */
            }
        }

        MotionController(); //defines all the different conditions for different types of motion
    }


    //Function to assign leg colour & Stinger/Grabber directions based on genes
    void LegColourRaycast(int LegID)
    {
        int[] LegGenes = GetComponent<Genome>().LegFunction;
        int LegGene = LegGenes[LegID - 1];
        Vector3 Direction = LegDirections[LegID - 1];
        string LegName = "Leg" + LegID.ToString();
        GameObject Leg = transform.Find(LegName).gameObject;
        if (LegGene == 1)
        {
            Leg.GetComponent<Renderer>().material = grabberColour;
            GrabDirections.Add(Direction); //add Direction to GrabDirections list

        }
        else if (LegGene == 2)
        {
            Leg.GetComponent<Renderer>().material = stingerColour;
            StingDirections.Add(Direction); //add Direction to StingDirections list
        }
    }


    //Function that defines all the possible states of motion
    void MotionController()
    {
        float speed = Random.Range(minSpeed, maxSpeed); //set a random speed for each time step

        //if I'm dead, do nothing
        if (gameObject.CompareTag("Inert"))
        {
            Rigidbody rBody = GetComponent<Rigidbody>();
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }
        //if I'm being grabbed by someone else, do nothing
        else if (gameObject.CompareTag("Grabbed") || gameObject.CompareTag("Grabbing_G"))
        {
            Rigidbody rBody = GetComponent<Rigidbody>();
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }
        //if there's a detected target, move in the direction of target until you collide with it (or a wall)
        else if (gameObject.CompareTag("GrabTargeting") || gameObject.CompareTag("StingTargeting") || gameObject.CompareTag("StingTargeting_G"))
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.fixedDeltaTime * speed);
        }
        //Else, check my raycasts and target if something found, otherwise move randomly
        else
        {
            List<Vector3> GrabHits = new List<Vector3>();
            List<Vector3> StingHits = new List<Vector3>();
            if (GrabDirections.Any() && !gameObject.CompareTag("Grabbing") && !gameObject.CompareTag("Grabbing_G"))
            {
                foreach (Vector3 direction in GrabDirections)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity, layerMask))
                    {
                        GrabHits.Add(hit.point);
                    }
                }
            }
            if (StingDirections.Any())
            {
                foreach (Vector3 direction in StingDirections)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity, layerMask))
                    {
                        StingHits.Add(hit.point);
                    }
                }
            }
            if (GrabHits.Any() && !StingHits.Any())
            {
                //Find closest point in Grab List & set as target
                target = GetClosestHitPoint(GrabHits);
                gameObject.tag = "GrabTargeting";
            }
            else if (!GrabHits.Any() && StingHits.Any())
            {
                //Find closest point in Sting List & set as target
                target = GetClosestHitPoint(StingHits);
                if (gameObject.CompareTag("Creature"))
                {
                    gameObject.tag = "StingTargeting";
                }
                else
                {
                    gameObject.tag = "StingTargeting_G";
                }
            }
            else if (GrabHits.Any() && StingHits.Any())
            {
                //depends on genome grabber preference
                if (grabPref >= 0.5)
                {
                    target = GetClosestHitPoint(GrabHits);
                    gameObject.tag = "GrabTargeting";
                }
                else
                {
                    target = GetClosestHitPoint(StingHits);
                    if (gameObject.CompareTag("Creature"))
                    {
                        gameObject.tag = "StingTargeting";
                    }
                    else
                    {
                        gameObject.tag = "StingTargeting_G";
                    }
                }
            }
            else
            {
                //Move randomly if both lists empty
                Vector3 randomDirection = new Vector3(0, Mathf.Sin(timeVar) * (rotationRange / 2), 0); // Moving at random angles 
                timeVar += step;
                GetComponent<Rigidbody>().AddForce(transform.forward * speed);
                transform.Rotate(randomDirection * Time.fixedDeltaTime * 10.0f);
            }
        }
    }


    //What happens when you collide with an object?  
    void OnCollisionEnter(Collision collision)
    {
        //If you collide with something other than a wall...
        if (!collision.gameObject.CompareTag("Walls"))
        {
            //if sting targeting, sting it
            if (gameObject.CompareTag("StingTargeting") || gameObject.CompareTag("StingTargeting_G"))
            {
                Sting(collision);
            }
            //if grab targeting, grab it
            else if (gameObject.CompareTag("GrabTargeting"))
            {
                Grab(collision);
            }

        }
        else //(i.e. if you collide with a wall...)
        {
            //if you were StingTargeting_G, go back to grabbing mode
            if (gameObject.CompareTag("StingTargeting_G"))
            {
                gameObject.tag = "Grabbing";
            }
            //if you were GrabTargeting or StingTargeting, go back to random motion
            else if (gameObject.CompareTag("GrabTargeting") || gameObject.CompareTag("StingTargeting"))
            {
                gameObject.tag = "Creature";
            }
        }
    }


    //Function that defines grabbing behaviour
    void Grab(Collision collision)
    {
        grabbee = collision.gameObject;
        if (!grabbee.CompareTag("Pick Up"))
        {
            grabbee.tag = "Grabbed";
        }
        Rigidbody rBody = grabbee.GetComponent<Rigidbody>();
        rBody.isKinematic = true;
        //rBody.detectCollisions = false;
        collision.transform.SetParent(transform);
        collision.transform.localPosition = offset;
        releaseTime = Time.time + releaseRate;
        if (gameObject.CompareTag("Grabbed"))
        {
            gameObject.tag = "Grabbing_G";
        }
        else
        {
            gameObject.tag = "Grabbing";
        }
    }

    //Function that defines stinging behaviour
    void Sting(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pick Up"))
        {
            Destroy(collision.gameObject.GetComponent<Rotator>());
        }
        collision.gameObject.tag = "Inert";
        collision.gameObject.layer = 8;
        Rigidbody rBody = collision.gameObject.GetComponent<Rigidbody>();
        rBody.isKinematic = true;
        rBody.detectCollisions = false;
        Renderer[] children;
        children = collision.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in children)
        {
            rend.material = stingerColour;
        }
        if (gameObject.CompareTag("StingTargeting_G"))
        {
            gameObject.tag = "Grabbing";
        }
        else
        {
            gameObject.tag = "Creature";
        }
        //Debug.Log("Destroyed " + collision.gameObject.ToString());
    }

    //Function to find closest hit point to me
    Vector3 GetClosestHitPoint(List<Vector3> hits)
    {
        Vector3 Closest = Vector3.zero;
        float minDist = Mathf.Infinity;
        foreach (Vector3 hitp in hits)
        {
            float dist = Vector3.Distance(hitp, transform.position);
            if (dist < minDist)
            {
                Closest = hitp;
                minDist = dist;
            }
        }
        return Closest;
    }

}
