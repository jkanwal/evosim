using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using ProceduralModeling;
using Random = UnityEngine.Random;
//using UnityEditor;
//using UnityEditor.UIElements;

public class LittleChem : MonoBehaviour
{

    //Inputs
    public GameObject ArenaPrefab;
    public GameObject AgentPrefab;
    public Genome genome;
    public ChemAgentBehaviour chemBehaviour;
    public bool[] agentProps;
    
    //Simulation parameters (which can be altered before a run)

    public int patchNum = 1; //number patches per generation
    public int agentsNum = 2; //number creatures per patch

    public int resetRate = 100; // number of ticks after which new generation begins
    public int genNum = 10; //total number of generations after which simulation stops
    public float arenaSize = 30f; // length of side of square arena
    public float minimumHeight = 2f; //min height of object placement in patch
    public float maximumHeight = 7f; //max height of object placement in patch
 
    //Other global variables
    private int Ticks=0;
    private float xzLim = 0;
    private GameObject mainarena;


    public List<GameObject> agents = new List<GameObject>();
    public List<GameObject> netrenderers = new List<GameObject>();
    public List<LineRenderer> snetrenderers = new List<LineRenderer>();
    List<(int, int)> sconnections = new List<(int, int)>();
    Dictionary<(GameObject, GameObject), LineRenderer> connections = new Dictionary<(GameObject, GameObject), LineRenderer>();
    List<(GameObject, GameObject)> connClearer = new List<(GameObject, GameObject)>();

    public float initSpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
        mainarena = Instantiate(ArenaPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //Application.targetFrameRate = 30;

        xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed

        // Colorful reaction (edges appearance)
        // 2 colliding spheres -> k-cnets 
        //
        
        SpawnCollidingPair(initSpeed);
        
        // from unit to structure



    }

    // Fixed Update is called at a set interval, and deals with the physics & tick advances
    void FixedUpdate()
    {
        Ticks++; //count up a Tick at each physics update

        //Start a new reaction after number of ticks reaches resetRate or max agents reached
        //Debug.Log(Ticks);
        if(Ticks >= resetRate || agents.Count > 1000)
        {
            //remove all agents
            foreach (GameObject x in agents)
            {
                x.SetActive(false);
                Destroy(x);
            }
            //Debug.Log("a:" + agents.Count);
            agents = new List<GameObject>();
            
            //if all agents removed create new pair
            if (agents.Count == 0)
            {
                Debug.Log("spawn Pair");
                SpawnCollidingPair(initSpeed);
                //initSpeed *= 2;
            }
            Ticks = 0;
        }

        //lines updates
        foreach (var connection in connections)
        {
            try
            {
                //update position if the particle still exist
                if(connection.Key.Item1 != null && connection.Key.Item2 != null) { 
                    connection.Value.SetPosition(0, connection.Key.Item1.transform.position);
                    connection.Value.SetPosition(1, connection.Key.Item2.transform.position);
                }
                else // or remove it the line 
                {
                    connClearer.Add(connection.Key);
                }
            }
            catch (InvalidCastException e)
            {
                Debug.Log(e);
            }
                        
        }

        foreach(var conn in connClearer)
        {
            connections.Remove(conn);
        }



    }


    // create new pair of particles moving toward each other
    private void SpawnCollidingPair(float speed)
    {
        var agent1 = CreateAgent(AgentPrefab, -4f, 5f, 0f, 2f);
        var agent2 = CreateAgent(AgentPrefab, 4f, 5f, 0f, 2f);
        
        agent1.GetComponent<Rigidbody>().velocity = new Vector3(speed, 0f, 0f);
        agent2.GetComponent<Rigidbody>().velocity = new Vector3(-speed, 0f, 0f);
        agent2.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Random.ColorHSV());
    }

        public GameObject CreateAgent(GameObject pref, float x, float y, float z, float size)
    {
        var a = Instantiate(pref) as GameObject;
        a.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        a.transform.position = new Vector3(x, y, z);
        a.transform.localScale = new Vector3(size, size, size);
        a.GetComponent<ChemAgentBehaviour>().SetEnvironment(this);
        agents.Add(a);
        return a;
    }

    public GameObject CreateAgent(GameObject pref, Vector3 pos, Vector3 vel, float size)
    {
        var a = Instantiate(pref) as GameObject;
        a.transform.position = pos;
        a.GetComponent<Rigidbody>().velocity = vel;
        a.transform.localScale = new Vector3(size, size, size);
        a.GetComponent<ChemAgentBehaviour>().SetEnvironment(this);
        agents.Add(a);
        return a;
    }

    // create a springhy connection between two agents
    public void connectAgents(GameObject a1, GameObject a2)
    {
        SpringJoint spring = a1.AddComponent<SpringJoint>();
        //spring.autoConfigureConnectedAnchor = false;
        spring.connectedBody = a2.gameObject.GetComponent<Rigidbody>();
        var dist = Random.Range(1f, 3f);
        spring.minDistance = dist;
        spring.maxDistance = dist;
        spring.spring = 280f;
        spring.damper = 0.9f;
        GameObject myLine = new GameObject();
        myLine.transform.position = a1.transform.position;
        myLine.transform.SetParent(a2.gameObject.transform);
        myLine.AddComponent<LineRenderer>();
        myLine.AddComponent<Rigidbody>();
        myLine.GetComponent<Rigidbody>().isKinematic = true;
        LineRenderer line = myLine.GetComponent<LineRenderer>();
        line.material = a1.GetComponent<MeshRenderer>().material;
        line.material.SetColor("_Color", Random.ColorHSV());
        line.GetComponent<Renderer>().material.SetColor("_EmissionColor", Random.ColorHSV());
        line.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        line.SetColors(Color.white, Color.white);
        line.SetWidth(0.05f, 0.05f);

        line.SetPosition(0, a1.transform.position);
        line.SetPosition(1, a2.gameObject.transform.position);

        connections.Add((a1, a2), line);
    }
       

}



