using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

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
    private Rigidbody rBody;

    //Declare variables for raycasting & grabbing
    private Vector3[] LegDirections;
    private List<Vector3> StingDirections = new List<Vector3>();
    private List<Vector3> GrabDirections = new List<Vector3>();
    public int layerMask = 1 << 9;
    public float raycastLimit = 5f;
    private Vector3 target;
    private Vector3 targetDirection;
    private float grabPref;
    //private Vector3 offset = new Vector3(0f, 2f, 0f);
    public int releaseRate = 100;
    private List<GameObject> Grabbees = new List<GameObject>();
    private List<Vector3> TargetDirections = new List<Vector3>();
    private List<float> ReleaseTimes = new List<float>();

    //keep track of my food points
    public int points = 0;

    //Keep track of Ticks
    private int Ticks;

    void Start()
    {
        LegDirections = new Vector3[] { transform.up, -transform.up, transform.right, -transform.right, transform.forward, -transform.forward };
        //Assign leg colour based on whether it is a stinger or grabber
        int[] LegGenes = GetComponent<Genome>().LegFunction;
        for (int i = 1; i < 7; i++)
        {
            LegColourRaycast(i, LegGenes);
        }

        //Get min and max speed, rotation range, and grabPref from genome
        minSpeed = gameObject.GetComponent<Genome>().minSpeed;
        maxSpeed = gameObject.GetComponent<Genome>().maxSpeed;
        rotationRange = gameObject.GetComponent<Genome>().rotationRange;
        grabPref = gameObject.GetComponent<Genome>().GrabberPref;

        //Get Rigid body component just once at Start, as this is an expensive operation
        rBody = GetComponent<Rigidbody>();

        Ticks = 0;

    }

    void FixedUpdate()
    {
        Ticks += 1; //count up a Tick at each physics update

        //if I'm grabbing food, digest it and get a point after releaseRate amount of time
        //iterate backwards as we may be deleting elements from the list
        if (Grabbees.Count > 0)
        {
            for (int i = Grabbees.Count - 1; i >= 0; i--)
            {
                if (Grabbees[i] != null)
                {
                    if (Grabbees[i].CompareTag("Pick Up"))
                    {
                        if (Ticks > ReleaseTimes[i])
                        {
                            points += 1; //get a point for digesting food!
                            //Grabbees.Remove(grabbee);
                            Destroy(Grabbees[i]); //destroy grabbee
                            Grabbees.RemoveAt(i); //remove from Grabbee list
                            GrabDirections.Add(TargetDirections[i]); //add the target direction back to grab directions
                            TargetDirections.RemoveAt(i); //and remove it from the target directions list
                            ReleaseTimes.RemoveAt(i); //also remove the corresponding release time
                        }
                    }
                }
                else
                { 
                    Grabbees.RemoveAt(i);
                } 
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

        MotionController(); //defines all the different conditions for different types of motion

    }


    //Function to assign leg colour & Stinger/Grabber directions based on genes
    void LegColourRaycast(int LegID, int[] LegGenes)
    {
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
        //if I'm dead, or bring grabbed by someone else, do nothing
        if (gameObject.CompareTag("Inert") || gameObject.CompareTag("Grabbed"))
        {   
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }
        //if there's a detected target, move in the direction of target until you collide with it (or a wall)
        else if (gameObject.CompareTag("GrabTargeting") || gameObject.CompareTag("StingTargeting"))
        {
            float speed = Random.Range(minSpeed, maxSpeed); //set a random speed between the min and max
            transform.position = Vector3.MoveTowards(transform.position, target, Time.fixedDeltaTime * speed);
        }
        //Else, check my raycasts and target if something found, otherwise move randomly
        else
        {
            List<Vector3> GrabHits = new List<Vector3>();
            List<Vector3> GrabTargetDirections = new List<Vector3>();
            List<Vector3> StingHits = new List<Vector3>();
            if (GrabDirections.Any())
            {
                foreach (Vector3 direction in GrabDirections)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, direction, out hit, raycastLimit, layerMask))
                    {
                        GrabHits.Add(hit.point);
                        GrabTargetDirections.Add(direction);
                    }
                }
            }
            if (StingDirections.Any())
            {
                foreach (Vector3 direction in StingDirections)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, direction, out hit, raycastLimit, layerMask))
                    {
                        StingHits.Add(hit.point);
                    }
                }
            }
            if (GrabHits.Any() && !StingHits.Any())
            {
                //Find closest point in Grab List & set as target
                target = GetClosestHitPoint(GrabHits);
                targetDirection = GrabTargetDirections[GrabHits.IndexOf(target)];
                gameObject.tag = "GrabTargeting";
            }
            else if (!GrabHits.Any() && StingHits.Any())
            {
                //Find closest point in Sting List & set as target
                target = GetClosestHitPoint(StingHits);
                gameObject.tag = "StingTargeting";
            }
            else if (GrabHits.Any() && StingHits.Any())
            {
                //depends on genome grabber preference
                if (grabPref >= 0.5)
                {
                    target = GetClosestHitPoint(GrabHits);
                    targetDirection = GrabTargetDirections[GrabHits.IndexOf(target)];
                    gameObject.tag = "GrabTargeting";
                }
                else
                {
                    target = GetClosestHitPoint(StingHits);
                    gameObject.tag = "StingTargeting";
                }
            }
            else
            {
                //Move randomly if both lists empty
                float speed = Random.Range(minSpeed, maxSpeed); //set a random speed for this time step
                Vector3 randomDirection = new Vector3(0, Mathf.Sin(timeVar) * (rotationRange / 2), 0); //set a random angle to turn
                timeVar += step;
                transform.Rotate(randomDirection * Time.fixedDeltaTime * 10.0f);
                rBody.AddForce(transform.forward * speed);
            }
        }
    }


    //What happens when you collide with an object?  
    void OnCollisionEnter(Collision collision)
    {
        //If you collide with something other than a wall or dead thing...
        if (!collision.gameObject.CompareTag("Walls") && !collision.gameObject.CompareTag("Inert"))
        {
            //if sting targeting, sting it
            if (gameObject.CompareTag("StingTargeting"))
            {
                Sting(collision);
            }
            //if grab targeting, grab it
            else if (gameObject.CompareTag("GrabTargeting"))
            {
                Grab(collision);
            }

        }
        else //(i.e. if you collide with a wall or dead thing...)
        {
            //if you were GrabTargeting or StingTargeting, go back to random motion
            if (gameObject.CompareTag("GrabTargeting") || gameObject.CompareTag("StingTargeting"))
            {
                gameObject.tag = "Creature";
            }
        }
    }


    //Function that defines grabbing behaviour
    void Grab(Collision collision)
    {
        Grabbees.Add(collision.gameObject); //add grabbee to my grabbees list
        TargetDirections.Add(targetDirection); //add grabbing limb direction to my list of grabbing limbs
        GrabDirections.Remove(targetDirection); //remove this direction from my list of directions to raycast in
        ReleaseTimes.Add(Ticks + releaseRate); //add Ticks count for when to release 
        if (!collision.gameObject.CompareTag("Pick Up"))
        {
            collision.gameObject.tag = "Grabbed"; //if the grabbee is not food, change its tag to 'Grabbed'
            Rigidbody rBody_G = collision.gameObject.GetComponent<Rigidbody>(); 
            rBody_G.isKinematic = true; //turn grabbee's rbody into kinematic rbody
        }
        collision.transform.SetParent(transform, true); //set it as my child
        collision.transform.position = transform.position; //position it at my origin
        collision.transform.localPosition = 2*targetDirection; //move it out to the end of the correct limb
        gameObject.tag = "Creature"; //change my tag back to random motion
    }

    //Function that defines stinging behaviour
    void Sting(Collision collision)
    {
        //things to do only to creatures
        if (collision.gameObject.tag != "Pick Up")
        {
            Rigidbody rBody_S = collision.gameObject.GetComponent<Rigidbody>();
            rBody_S.isKinematic = true; //turn the dead creature into kinematic rbody
            //tag all child objects inert
            foreach (Transform child in collision.transform)
            {
                child.gameObject.tag = "Inert";
                child.gameObject.layer = 8;
            }
        }
        collision.gameObject.tag = "Inert"; //tag object itself as inert
        collision.gameObject.layer = 8; //add it to inert raycast layer
        collision.transform.parent = null; //detach collision object from any parent
        //change colour of entire object
        Renderer[] children;
        children = collision.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in children)
        {
            rend.material = stingerColour;
        }
        gameObject.tag = "Creature"; //change my tag back to random motion
    }

    //Function to find closest hit point to me
    Vector3 GetClosestHitPoint(List<Vector3> hits)
    {
        Vector3 Closest = Vector3.zero;
        float minDist = Mathf.Infinity;
        foreach (Vector3 hitp in hits)
        {
            float dist = (transform.position - hitp).sqrMagnitude;
            if (dist < minDist)
            {
                Closest = hitp;
                minDist = dist;
            }
        }
        return Closest;
    }

}
