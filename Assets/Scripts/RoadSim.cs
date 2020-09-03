using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using Random = UnityEngine.Random;
//using UnityEditor;
//using UnityEditor.UIElements;


public class RoadSim : MonoBehaviour
{

    [SerializeField] GameObject treeprefab;
    [SerializeField] GameObject speedtree1;
    //Inputs
    public GameObject ArenaPrefab;
    public GameObject AgentPrefab;
    public Genome genome;
    public ChemAgentBehaviour chemBehaviour;
    public bool[] agentProps;
    
    //Simulation parameters (which can be altered before a run)
    public bool global = true; //Global competetion between creatures? (if false, Local competiton)
    public bool rHigh = true; //High relatedness within a patch? (if false, Low relatedness)
    public int patchNum = 1; //number patches per generation
    public int agentsNum = 2; //number creatures per patch

    public int resetRate = 100; // number of ticks after which new generation begins
    public int genNum = 10; //total number of generations after which simulation stops
    public float arenaSize = 30f; // length of side of square arena
    public float minimumHeight = 2f; //min height of object placement in patch
    public float maximumHeight = 7f; //max height of object placement in patch
 
    //Other global variables
    private int Ticks=0;
    private int Generation;
    private float x0;
    private float z0;
    private float xzLim;
    private GameObject[] arenaList; //array of arenas
    GameObject mainarena;


    public List<GameObject> agents = new List<GameObject>();
    public List<GameObject> netrenderers = new List<GameObject>();
    public List<LineRenderer> snetrenderers = new List<LineRenderer>();
    List<(int, int)> sconnections = new List<(int, int)>();
    Dictionary<(GameObject, GameObject), LineRenderer> connections = new Dictionary<(GameObject, GameObject), LineRenderer>();
    List<(GameObject, GameObject)> connClearer = new List<(GameObject, GameObject)>();
    Dictionary<string, Vector3> vertices = new Dictionary<string, Vector3>();

    // Start is called before the first frame update
    void Start()
    {

        //mainarena = Instantiate(ArenaPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //Application.targetFrameRate = 30;

        //xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed
        string text = File.ReadAllText("Assets/Resources/Edi.json");
        
        JSONObject json = new JSONObject(text);
        foreach (JSONObject j in json.list)
        {
            
           // Debug.Log("List Elements: " +j.type + " " + j);
        }
        Debug.Log("nodes " + json.GetField("nodes"));
        foreach(JSONObject node in json.GetField("nodes").list)
        {
            Debug.Log("node : " + node.GetField("x"));
            var v = new Vector3(float.Parse(node.GetField("x").ToString()) * 100f,0 , float.Parse(node.GetField("y").ToString()) * 100f);
            vertices.Add(node.GetField("osmid").ToString(), v);
        }
        foreach (JSONObject edge in json.GetField("edges").list)
        {           
            createEdge(vertices[edge.GetField("source").ToString()], vertices[edge.GetField("target").ToString()]); 
        }

    }

    // Fixed Update is called at a set interval, and deals with the physics & tick advances
    void FixedUpdate()
    {
        Ticks++; //count up a Tick at each physics update
        //Start a new generation after number of ticks reaches resetRate
        //Debug.Log(Ticks);
        if(Ticks >= resetRate || agents.Count > 50)
        {
            foreach (GameObject x in agents)
            {
                Destroy(x);
            }
            //Debug.Log("a:" + agents.Count);
            agents = new List<GameObject>();
            
        }

        //lines updates
        foreach (var connection in connections)
        {
            try
            {
                if(connection.Key.Item1 != null && connection.Key.Item2 != null) { 
                    connection.Value.SetPosition(0, connection.Key.Item1.transform.position);
                    connection.Value.SetPosition(1, connection.Key.Item2.transform.position);
                }
                else
                {
                    connClearer.Add(connection.Key);
                }
            }
            catch (InvalidCastException e)
            {
                
            }
                        
        }

        foreach(var conn in connClearer)
        {
            connections.Remove(conn);
        }



    }


    // Update is called once per frame
    /*
    void Update()
    {

        //Constantly add new food at some slow rate
        float rand = Random.value;
        if (rand < foodProb)
        {
            Vector3 position = new Vector3(Random.Range(x0 - xzLim, x0 + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z0 - xzLim, z0 + xzLim));
            Instantiate(FoodPrefab, position, Quaternion.identity);
        }
    }
    */

    void accessData(JSONObject obj)
    {
        switch (obj.type)
        {
            case JSONObject.Type.OBJECT:
                
                for (int i = 0; i < obj.list.Count; i++)
                {
                    string key = (string)obj.keys[i];
                    JSONObject j = (JSONObject)obj.list[i];
                    Debug.Log("keys: "+ obj.keys[i]);
                    //accessData(j);
                }
                break;
            case JSONObject.Type.ARRAY:
                foreach (JSONObject j in obj.list)
                {
                    accessData(j);
                }
                break;
            case JSONObject.Type.STRING:
                Debug.Log(obj.str);
                break;
            case JSONObject.Type.NUMBER:
                Debug.Log(obj.n);
                break;
            case JSONObject.Type.BOOL:
                Debug.Log(obj.b);
                break;
            case JSONObject.Type.NULL:
                Debug.Log("NULL");
                break;

        }
    }

    public void createEdge(Vector3 vertex1, Vector3 vertex2)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = vertex1;
        //myLine.transform.SetParent(a2.gameObject.transform);
        myLine.AddComponent<LineRenderer>();
        
        LineRenderer line = myLine.GetComponent<LineRenderer>();
        //line.material = a1.GetComponent<MeshRenderer>().material;
        line.material.SetColor("_Color", Random.ColorHSV());
        line.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
        line.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        line.SetColors(Color.white, Color.white);
        line.SetWidth(0.01f, 0.01f);

        line.SetPosition(0, vertex1);
        line.SetPosition(1, vertex2);
    }

    public void connectAgents(GameObject a1, GameObject a2)
    {
        SpringJoint spring = a1.AddComponent<SpringJoint>();
        //spring.autoConfigureConnectedAnchor = false;
        spring.connectedBody = a2.gameObject.GetComponent<Rigidbody>();
        var dist = Random.Range(2f, 4f);
        spring.minDistance = dist;
        spring.maxDistance = dist;
        spring.spring = 180f;
        spring.damper = 0.8f;
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
        //line.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        line.SetColors(Color.white, Color.white);
        line.SetWidth(0.05f, 0.05f);

        line.SetPosition(0, a1.transform.position);
        line.SetPosition(1, a2.gameObject.transform.position);

        connections.Add((a1, a2), line);
    }
       

    //Function to write Genome data of a creature
    void WriteGenome(GameObject creature)
    {
        string ArenaName = creature.transform.parent.name;
        string grabPref = creature.GetComponent<Genome>().GrabberPref.ToString();
        int[] LegGenes = creature.GetComponent<Genome>().LegFunction;
        int[] SGList = CountSG(LegGenes);
        string grabberNum = SGList[0].ToString();
        string stingerNum = SGList[1].ToString();
        //write new creature's genome data to file
        string genomeData = Generation.ToString() + "," + ArenaName + "," + grabberNum + "," + stingerNum + "," + grabPref;
    }

    //Function to destroy all objects with a specific tag
    void DestroyTag(string tag)
    {
        var ThingList = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject thing in ThingList)
        {
            Destroy(thing);
        }
    }

    //Function to disable all objects with a specific tag
    void DisableTag(string tag)
    {
        var ThingList = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject thing in ThingList)
        {
            thing.SetActive(false);
        }
    }

    

    //Function to count number of stingers & grabbers in a leg function gene segment
    int[] CountSG(int[] LegGenes)
    {
        int grabbers = 0;
        int stingers = 0;
        foreach (int item in LegGenes)
        {
            if (item == 1)
            {
                grabbers += 1;
            }
            else if (item == 2)
            {
                stingers += 1;
            }
        }
        int[] GSList = {grabbers, stingers};
        return GSList;
    }

}



