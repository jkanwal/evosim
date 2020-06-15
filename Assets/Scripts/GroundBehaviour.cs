using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class GroundBehaviour : Ground
{

    //load in genome, materials, etc.
    public int[] properties;
    public GameObject AgentPrefab;
    public GameObject FoodPrefab;
    //Declare variables for motion
    private float timeVar = 0;
    private float step = Mathf.PI / 60;
    private float minSpeed;
    private float maxSpeed;
    private float rotationRange;
    private Rigidbody rBody;

    private int Ticks;

    public float moveseed;

    public bool oscillate = false;
    public float phase = 0;
    private GroundSim environment;
    private int collisionCounter = 0;
    public List<GameObject> connections = new List<GameObject>();
    public List<LineRenderer> snetrenderers = new List<LineRenderer>();
    private int ID;

    public bool isLava = false;
    

    void Start()
    {
        //AgentPrefab = Resources.Load("Agent") as GameObject;
        rBody = GetComponent<Rigidbody>();



    }

    void FixedUpdate()
    {
        Ticks += 1; //count up a Tick at each physics update   


    }

    public override GameObject spawnGoodies()
    {
        GameObject food = Instantiate(FoodPrefab, this.transform.position, Quaternion.identity);
        return food;
    }


    public void BodyColour(Color bcolor)
    {

        GameObject body = transform.Find("Sphere").gameObject;
        body.GetComponent<Renderer>().material.SetColor("_Color", bcolor);
    }

    public void SetOscillate(bool oscillate)
    {
        this.oscillate = oscillate;
    }


    public void SetEnvironment(GroundSim env)
    {
        this.environment = env;
    }

    public void SetID(int id)
    {
        this.ID = id;
    }


    public void SetPhase(float phase)
    {
        this.phase = phase;
    }

    //What happens when you collide with an object?  
    void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Walls")){
            collisionCounter++;
            gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.white);
            GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            if (collisionCounter < 100)
            {
                gameObject.transform.localScale = gameObject.transform.localScale * (Mathf.Sin(Time.time) * 0.001f + 1.001f);

            }
        }
        if (!collision.gameObject.CompareTag("Walls") && collisionCounter >12)
        {
            rBody.velocity = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));
            transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Random.ColorHSV());
            if (environment.agents.Count < 40)
            {
                var fagent = environment.CreateAgent(AgentPrefab, transform.position, new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, 4f), Random.Range(-4f, 4f)), 0.6f);
                var kagent = environment.CreateAgent(AgentPrefab, transform.position, new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, 4f), Random.Range(-4f, 4f)), Random.Range(0.3f, 1.0f));
                kagent.GetComponent<Rigidbody>().isKinematic = true;
                kagent.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Random.ColorHSV());
                kagent.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                collisionCounter = 0;

                environment.connectAgents(kagent, fagent);
                
            }
            //if (Random.Range(0f, 1f) < 0.5f)
            //{

            //}
            //Destroy(gameObject);
        }


    }


}
