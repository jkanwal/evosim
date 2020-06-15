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

public class GroundSim : MonoBehaviour
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

    

    public float maximumHeight = 100f; //max height of object placement in patch

    //Other global variables
    private int Ticks = 0;
    private int Generation;
    private float x0;
    private float z0;
    private float xzLim;
    private GameObject[] arenaList; //array of arenas
    GameObject mainarena;


    public List<GameObject> agents = new List<GameObject>();
   

    List<GameObject> arenas = new List<GameObject>();
    List<GameObject> goodies = new List<GameObject>();
    


    // Start is called before the first frame update
    void Start()
    {

        mainarena = Instantiate(ArenaPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //Application.targetFrameRate = 30;

        xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed

        //Spawn STILL arenas
        x0 = 0f;
        z0 = 0f;
        var spacer = 4;
        for (var i = 0; i < patchNum; i++)
        {
            for (var j = 0; j < patchNum; j++)
            {
                Vector3 position = new Vector3(x0, 0, z0);
                GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
                arena.name = "Arena" + i.ToString();
                goodies.Add(arena.GetComponent<ProtoArena>().spawnGoodies());
                arenas.Add(arena);
                x0 += xzLim * spacer;
            }
            x0 = 0;
            z0 += xzLim * spacer;
        }

    }

    // Fixed Update is called at a set interval, and deals with the physics & tick advances
    void FixedUpdate()
    {
        Ticks++; //count up a Tick at each physics update
        //Debug.Log(Ticks);
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("do stuff");
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

    private void SpawnCollidingPair()
    {
        var agent1 = CreateAgent(AgentPrefab, -1f, 5f, 5f, 1f);
        var agent2 = CreateAgent(AgentPrefab, 1f, 5f, 5f, 1f);

        agent1.GetComponent<Rigidbody>().velocity = new Vector3(3f, 0f, 0f);
        agent2.GetComponent<Rigidbody>().velocity = new Vector3(-3f, 0f, 0f);
    }

    public GameObject CreateAgent(GameObject pref, float x, float y, float z, float size)
    {
        var a = Instantiate(pref) as GameObject;
        a.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        a.transform.position = new Vector3(x, y, z);
        a.transform.localScale = new Vector3(size, size, size);
        a.GetComponent<GroundBehaviour>().SetEnvironment(this);
        agents.Add(a);
        return a;
    }

    public GameObject CreateAgent(GameObject pref, Vector3 pos, Vector3 vel, float size)
    {
        var a = Instantiate(pref) as GameObject;
        a.transform.position = pos;
        a.GetComponent<Rigidbody>().velocity = vel;
        a.transform.localScale = new Vector3(size, size, size);
        a.GetComponent<GroundBehaviour>().SetEnvironment(this);
        agents.Add(a);
        return a;
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

        //connections.Add((a1, a2), line);
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
        int[] GSList = { grabbers, stingers };
        return GSList;
    }

}



