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

public class CatP : LittleChem
{

    
    //Other global variables
    private int Ticks=0;
    private int Generation;
    private float x0;
    private float z0;
    private float xzLim;
    private GameObject[] arenaList; //array of arenas


    Dictionary<(GameObject, GameObject), LineRenderer> connections = new Dictionary<(GameObject, GameObject), LineRenderer>();
    List<(GameObject, GameObject)> connClearer = new List<(GameObject, GameObject)>();


    // Start is called before the first frame update
    void Start()
    {
        
        var mainarena = Instantiate(ArenaPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //Application.targetFrameRate = 30;

        xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed

        // Colorful reaction (edges appearance)
        // 2 colliding spheres -> k-cnets 
        //

        //SpawnCollidingPair(initSpeed);
        PoolAndDrop();
        // from unit to structure



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
                if(x.transform.localScale.magnitude < 0.1f && x != null)
                {
                   // x.SetActive(false);
                    Destroy(x);
                    agents.Remove(x);
                }
                //
                //
            }
            //Debug.Log("a:" + agents.Count);
            //agents = new List<GameObject>();
            
            
            if (agents.Count == 0)
            {
               // Debug.Log("spawn Pair");
                //SpawnCollidingPair(initSpeed);
                //initSpeed *= 2;
            }
            Ticks = 0;
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
                Debug.Log(e);
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

    private void SpawnCollidingPair(float speed)
    {
        var agent1 = CreateAgent(AgentPrefab, -4f, 5f, 0f, 2f);
        var agent2 = CreateAgent(AgentPrefab, 4f, 5f, 0f, 2f);
        
        agent1.GetComponent<Rigidbody>().velocity = new Vector3(speed, 0f, 0f);
        agent2.GetComponent<Rigidbody>().velocity = new Vector3(-speed, 0f, 0f);
        agent2.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Random.ColorHSV());
    }

    private void PoolAndDrop()
    {
        CreateAgent(AgentPrefab, 0f, 1f, 0f, 1f);
        for (int i = 0; i < agentsNum; i++)
        {
            var agent1 = CreateAgent(AgentPrefab, Random.Range(-15,15f), Random.Range(0.5f, 4f), Random.Range(-15, 15f), 1f);
            agent1.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);

        }
        
        var agent2 = CreateAgent(AgentPrefab, 0f, 8f, 0f, 2f);

        
        agent2.GetComponent<Rigidbody>().velocity = new Vector3(0f, -3f, 0f);
        agent2.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Random.ColorHSV());
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



