using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class CreatureBehaviour : MonoBehaviour
{

    //load in genome, materials, public lists of food/creatures
    public Genome genome;
    //public RunSim runSim;
    public CreatureBehaviour creatureBehaviour;
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
    private float grabFood;
    private float grabCreature;
    private float stingFood;
    private float stingCreature;
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
        grabFood = gameObject.GetComponent<Genome>().GrabFood;
        grabCreature = gameObject.GetComponent<Genome>().GrabCreature;
        stingFood = gameObject.GetComponent<Genome>().StingFood;
        stingCreature = gameObject.GetComponent<Genome>().StingCreature;

        //Get Rigid body component just once at Start, as this is an expensive operation
        rBody = GetComponent<Rigidbody>();

        //Access liveList and sterileList from RunSim script, so that this creature can be moved between lists if grabbed or stung
        //runSim = GameObject.Find("RunSim").GetComponent<RunSim>(); //access the RunSim script

        Ticks = 0;
    }

    void FixedUpdate() 
    {
        Ticks += 1; //count up a Tick at each physics update

        MotionController(); //defines all the different conditions for different types of motion
    }

    void Update()
    {
        //Do stuff only every 15 ticks, to save on CPU
        if (Ticks % 15 == 0) 
        {
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
                                Grabbees[i].SetActive(false); //disable digested grabbee
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
        }
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
        //Else, check my raycasts every 10 ticks and target if something found, otherwise move randomly
        else
        {
            if (Ticks % 10 == 0) 
            {
                List<RaycastHit> StingHits = new List<RaycastHit>();
                List<RaycastHit> GrabHits = new List<RaycastHit>();
                List<Vector3> GrabTargetDirections = new List<Vector3>();
                //first check if there are any sting targets, and get the closest one
                if (StingDirections.Any())
                {
                    foreach (Vector3 direction in StingDirections)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, direction, out hit, raycastLimit, layerMask))
                        {
                            StingHits.Add(hit);
                        }
                    }
                    if (StingHits.Count > 0) 
                    {
                        RaycastHit targetHit = GetClosestHitPoint(StingHits);
                        //if the closest sting target is food, go for it with probability [stingFood]
                        if (targetHit.transform.CompareTag("Pick Up"))
                        {
                            float rand = Random.value;
                            if (rand <= stingFood)
                            {
                                target = targetHit.point;
                                gameObject.tag = "StingTargeting";
                            }
                        }
                        //if the closest sting target is a creature, go for it with probability [stingCreature]
                        else
                        {
                            float rand = Random.value;
                            if (rand <= stingCreature)
                            {
                                target = targetHit.point;
                                gameObject.tag = "StingTargeting";
                            }
                        }
                    }   
                }
                //if I'm not now sting targeting, check for grab targets, and get the closest one
                if (!gameObject.CompareTag("StingTargeting"))
                {
                    if (GrabDirections.Any())
                    {
                        foreach (Vector3 direction in GrabDirections)
                        {
                            RaycastHit hit;
                            if (Physics.Raycast(transform.position, direction, out hit, raycastLimit, layerMask))
                            {
                                GrabHits.Add(hit);
                                GrabTargetDirections.Add(direction);
                            }
                        }
                        if (GrabHits.Count > 0) 
                        {
                            RaycastHit targetHit = GetClosestHitPoint(GrabHits);
                            //if the closest grab target is food, go for it with probability [grabFood]
                            if (targetHit.transform.CompareTag("Pick Up"))
                            {
                                float rand = Random.value;
                                if (rand <= grabFood)
                                {
                                    target = targetHit.point;
                                    targetDirection = GrabTargetDirections[GrabHits.IndexOf(targetHit)];
                                    gameObject.tag = "GrabTargeting";
                                }
                            }
                            //if the closest grab target is a creature, go for it with probability [grabCreature]
                            else if (targetHit.transform.CompareTag("Creature") || targetHit.transform.CompareTag("Grabbed"))
                            {
                                float rand = Random.value;
                                if (rand <= grabCreature)
                                {
                                    target = targetHit.point;
                                    targetDirection = GrabTargetDirections[GrabHits.IndexOf(targetHit)];
                                    gameObject.tag = "GrabTargeting";
                                }
                            }
                        }    
                    }
                }
            }
            //If I'm not now grab or sting targeting, continue with random motion
            if (!gameObject.CompareTag("GrabTargeting") && !gameObject.CompareTag("StingTargeting"))
            {
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
        collision.transform.SetParent(transform, true); //set it as my child
        collision.transform.position = transform.position; //position it at my origin
        collision.transform.localPosition = 2*targetDirection; //move it out to the end of the correct limb
        //If another creature is grabbed...
        if (!collision.gameObject.CompareTag("Pick Up"))
        {
            collision.gameObject.tag = "Grabbed"; //change its tag
            Rigidbody rBody_G = collision.gameObject.GetComponent<Rigidbody>(); 
            rBody_G.isKinematic = true; //turn grabbee's rbody into kinematic rbody
            int points_G = collision.gameObject.GetComponent<CreatureBehaviour>().points; //get grabbee's points value
            points += points_G; //add its points to mine
            collision.gameObject.GetComponent<CreatureBehaviour>().points = 0; //and set its points to zero as it's lost its chance to reproduce
            Renderer[] children = collision.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in children)
            {
                rend.material = grabberColour;
            }
        }  
        gameObject.tag = "Creature"; //change my tag back to random motion
    }

    //Function that defines stinging behaviour
    void Sting(Collision collision)
    {
        //things to do only to creatures
        if (!collision.gameObject.CompareTag("Pick Up"))
        {
            Rigidbody rBody_S = collision.gameObject.GetComponent<Rigidbody>();
            rBody_S.isKinematic = true; //turn the dead creature into kinematic rbody
            //tag all child objects inert
            foreach (Transform child in collision.transform)
            {
                child.gameObject.tag = "Inert";
                child.gameObject.layer = 8;
            }
            collision.gameObject.GetComponent<CreatureBehaviour>().points = 0; //set fitness value to zero as it's lost its chance to reproduce
            Transform arenaTransform = collision.transform.root;
            collision.transform.SetParent(arenaTransform); //detach collision object from immediate parents...reparent it to just the arena
        }
        else //things to do only to food
        {
            collision.transform.parent = null; //detach collision object from any parent
        }
        collision.gameObject.tag = "Inert"; //tag object itself as inert
        collision.gameObject.layer = 8; //add it to inert raycast layer
        //change colour of entire object
        Renderer[] children = collision.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in children)
        {
            rend.material = stingerColour;
        }
        gameObject.tag = "Creature"; //change my tag back to random motion
    }

    //Function to find closest RaycastHit object to me
    RaycastHit GetClosestHitPoint(List<RaycastHit> hits)
    {
        RaycastHit closestHit = new RaycastHit();
        float minDist = Mathf.Infinity;
        foreach (RaycastHit hit in hits)
        {
            float dist = (transform.position - hit.point).sqrMagnitude;
            if (dist < minDist)
            {
                closestHit = hit;
                minDist = dist;
            }
        }
        return closestHit;
    }

}
