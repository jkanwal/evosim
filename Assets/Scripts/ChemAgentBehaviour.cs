using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class ChemAgentBehaviour : MonoBehaviour
{

    //load in genome, materials, etc.
    public int[] properties;
    public GameObject AgentPrefab;

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
    private LittleChem environment;
    private int collisionCounter = 0;
    public List<GameObject> connections = new List<GameObject>();
    public List<LineRenderer> snetrenderers = new List<LineRenderer>();
    private int ID;

    void Start()
    {
        AgentPrefab = Resources.Load("Agent") as GameObject;
        rBody = GetComponent<Rigidbody>();
        var testseq = new List<int>();
        testseq.Add(0);
        CodeSome(testseq);

    }

    void FixedUpdate()
    {
        /*
        Ticks += 1; //count up a Tick at each physics update   

        var lineCounter = 0;
        foreach (var connection in connections)
        {
            snetrenderers[lineCounter].SetPosition(0, transform.position);
            snetrenderers[lineCounter].SetPosition(1, connection.transform.position);
            
            lineCounter++;
        }*/

    }

    void CodeSome(List<int> geneSequence)
    {
        // todo add costs
        int index = 0;
        int rules = 4; //update this number if more rules are added
        while (index < geneSequence.Count)
        {
            var gene = geneSequence[index] % rules;
            //Debug.Log("index-seq:" + index + " - " + geneSequence.Count);
            switch (gene)
            {
                case 0:
                    geneSequence.Add(Random.Range(0, rules));   //add a random gene to the chain
                    break;
                case 1:
                    //connections.Add(environment.agents[Random.Range(0, environment.agents.Count)]); // connect to a random agent
                    break;
                case 2:
                    Debug.Log("Case 2");
                    break;
                case 3:
                    Debug.Log("Case 3");
                    break;
                default:

                    //map
                    //grow
                    //appendix
                    //clone
                    //signal receiver
                    //trade
                    //connect to 1 end of list 
                    //GP
                    Debug.Log("Case not valid");
                    break;
            }
            index++;
        }
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


    public void SetEnvironment(LittleChem env)
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

    //Collisions change the volume of the particle until 2 new particles are created
    void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Walls")){
            collisionCounter++;
            gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.white);
            GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            //volume alternation
            if (collisionCounter < 100)
            {
                gameObject.transform.localScale = gameObject.transform.localScale * (Mathf.Sin(Time.time) * 0.001f + 1.001f);
            }
        }
        if (!collision.gameObject.CompareTag("Walls") && collisionCounter >20)
        {
            rBody.velocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
            
            transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Random.ColorHSV());
            rBody.transform.localScale *= 0.5f;

            
            if (environment.agents.Count < 1000) //global reaction limiter
            {
                
                //spawning extra particles
                float force = 1.0f;
                var fagent = environment.CreateAgent(AgentPrefab, transform.position, new Vector3(Random.Range(-force, force), Random.Range(-force, force), Random.Range(-force, force)), 0.6f);
                var kagent = environment.CreateAgent(AgentPrefab, transform.position, new Vector3(Random.Range(-force, force), Random.Range(-force, force), Random.Range(-force, force)), Random.Range(0.3f, 1.0f));
                kagent.GetComponent<Rigidbody>().isKinematic = true;
                kagent.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Random.ColorHSV());
                kagent.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                kagent.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Random.ColorHSV());
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
