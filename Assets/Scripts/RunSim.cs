using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using Random = UnityEngine.Random;
//using UnityEditor;
//using UnityEditor.UIElements;

public class RunSim : MonoBehaviour
{

    [SerializeField] GameObject treeprefab;
    [SerializeField] GameObject speedtree1;
    //Inputs
    public GameObject ArenaPrefab;
    public GameObject CreaturePrefab;
    public GameObject FoodPrefab;
    public Genome genome;
    public CreatureBehaviour creatureBehaviour;
    public string writepath = "Assets/Data/data.csv"; //write simulation data to this filename

    //Simulation parameters (which can be altered before a run)
    public bool global = true; //Global competetion between creatures? (if false, Local competiton)
    public bool rHigh = true; //High relatedness within a patch? (if false, Low relatedness)
    public int patchNum = 100; //number patches per generation
    public int creatureNum = 100; //number creatures per patch
    public int foodAmount = 24; //starting amount of food per patch
    public float foodProb = 0.01f; //rate of spontaneous food production in patch
    public float mutationRate = 0.01f; //mutation rate
    public int resetRate = 1200; // number of ticks after which new generation begins
    public int genNum = 10; //total number of generations after which simulation stops
    public float arenaSize = 30f; // length of side of square arena
    public float minimumHeight = 2f; //min height of object placement in patch
    public float maximumHeight = 7f; //max height of object placement in patch
 
    //Other global variables
    private int Ticks;
    private int Generation;
    private float x0;
    private float z0;
    private float xzLim;
    private GameObject[] arenaList; //array of arenas
    //private GameObject[] poolList; //pooling list for creatures 
    //private GameObject[] foodList; //pooling list for food objects 
    private List<GameObject> parentList = new List<GameObject>(); //create empty list of reproducers

    GameObject mainarena;
    public GameObject trees1Prefab;
    public GameObject trees2Prefab;
    public GameObject trees3Prefab;
    public GameObject sphere3Prefab;
    public List<GameObject> netrenderers = new List<GameObject>();
    public List<LineRenderer> snetrenderers = new List<LineRenderer>();
    List<(int, int)> sconnections = new List<(int, int)>();
    List<GameObject> sagents = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
        mainarena = Instantiate(ArenaPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //Application.targetFrameRate = 30;

        xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed

        //write file header
        WriteData("Gen, Patch, NumGrabbers, NumStingers, GrabberPref");

        //speedtrees loader
        trees1Prefab = Resources.Load("Broadleaf_Desktop") as GameObject;
        trees2Prefab = Resources.Load("Conifer_Desktop") as GameObject;
        trees3Prefab = Resources.Load("Palm_Desktop") as GameObject;
        sphere3Prefab = Resources.Load("Org3") as GameObject;

        GameObject[] trees = { trees1Prefab, trees2Prefab, trees3Prefab };

        //Spawn patches (arenas)
        x0 = 0f;
        z0 = 0f;
        for (var i = 1; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "Arena" + i.ToString();


            //Spawn initial creatures, each with only 1 grabber
            for (var j = 0; j < creatureNum+i; j++)
            {
                GameObject newbaby = CreateCreature(((float)Math.Sin(j*0.5) * j*0.4f)+x0, (float)Math.Cos(j*0.5)*j*0.4f);
                newbaby.transform.SetParent(arena.transform, true);
                var size = Random.Range(1f, 3f)/i;
                newbaby.transform.localScale = new Vector3(size, size, size);
                WriteGenome(newbaby);
            }
            
             x0 += xzLim * 3;
                    
        }
        //variable size creatures
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "Arena1-" + i.ToString();

            //Spawn initial creatures, each with only 1 grabber
            for (var j = 0; j < creatureNum*3; j++)
            {
                GameObject newbaby = CreateCreature(((float)Math.Sin(j * 0.5) * j * 0.3f) + x0, (float)Math.Cos(j * 0.5) * j * 0.3f);
                newbaby.transform.SetParent(arena.transform, true);
                newbaby.GetComponent<CreatureBehaviour>().SetOscillate(true);
                newbaby.GetComponent<CreatureBehaviour>().SetPhase(j*(i*0.05f));
                newbaby.GetComponent<CreatureBehaviour>().BodyColour(Color.red);
                
                WriteGenome(newbaby);
            }
            x0 += xzLim * 3;            
        }
       
        //trees
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "Arena2-" + i.ToString();

            //Spawn initial creatures, each with only 1 grabber
            for (var j = 0; j < creatureNum; j++)
            {
                
                createSpeedTree(((float)Math.Sin(j * 0.5) * j * 0.4f) + x0, 0.7f, (float)Math.Cos(j * 0.5) * j * 0.4f);
            }
            x0 += xzLim * 3;
        }

        //custom trees
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "Arena2-" + i.ToString();

            //Spawn trees
            for (var j = 0; j < creatureNum; j++)
            {
                createTree(trees[Random.Range(0, 2)],x0 + Random.Range(0f, xzLim*1.5f) - xzLim / 2, 0.7f, z0+ Random.Range(0f, xzLim*1.5f) - xzLim / 2, 0.8f);
            }
            x0 += xzLim * 3;
        }



        arenaList = GameObject.FindGameObjectsWithTag("Arena"); //fill the array of arenas
        Generation = 0;
        Ticks = 0;

        for (var i = 0; i < 8; i++)
        {
            for (var j = 0; j < 8; j++)
            {

                createSpeedTree(Random.Range(1f, xzLim) -xzLim/2, 0.7f, Random.Range(1f, xzLim) - xzLim / 2);
                
            }
        }


        // network of agents
        List<GameObject> agents = new List<GameObject>();
        List<(int,int)> connections = new List<(int, int)>();
        int counter = 0;
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "ArenaN-" + i.ToString();

            //spawn spheres
            for (var j = 0; j < creatureNum * 3; j++)
            {
                
                //node.GetComponent<CreatureBehaviour>().SetOscillate(true);
                //node.GetComponent<CreatureBehaviour>().SetPhase(j * (i * 0.01f));
                GameObject node = createTree(sphere3Prefab, x0+ Random.Range(0f, xzLim*1.5f)- xzLim/2, 5f+ Random.Range(0f, 10f), z0+ Random.Range(0f, xzLim*1.5f) - xzLim / 2, 1f);
                
                GameObject body = node.transform.Find("Sphere").gameObject;
                body.transform.position = node.transform.position;
                
                agents.Add(node);
                //if (Random.Range(0f, 1f) < 0.5)
                //{
                    int connectedAgent = (i * creatureNum * 3) + (j +Random.Range(1, creatureNum * 3))%(creatureNum * 3);                   
                    connections.Add((counter, connectedAgent));
                body.GetComponent<Renderer>().material.SetColor("_Color", Random.ColorHSV());
                counter++;
                
                //}

            }
            x0 += xzLim * 3;
        }
        
        //connections
        foreach (var connection in connections)
        {
            //GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);            
            //cylinder.transform.localScale = new Vector3(0.1f, 0.9f, 0.2f);
            //cylinder.transform.SetParent(agents[connection.Item1].transform);
            

            GameObject myLine = new GameObject();
            myLine.transform.position = agents[connection.Item1].transform.position;
            myLine.AddComponent<LineRenderer>();
            LineRenderer line = myLine.GetComponent<LineRenderer>();
            line.material = agents[connection.Item1].GetComponent<MeshRenderer>().material;
            line.material.SetColor("_Color", Random.ColorHSV());
            line.GetComponent<Renderer>().material.SetColor("_EmissionColor", Random.ColorHSV());
            line.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            line.SetColors(Color.white, Color.white);
            line.SetWidth(0.1f, 0.1f);
          
            line.SetPosition(0, agents[connection.Item1].transform.position);
            line.SetPosition(1, agents[connection.Item2].transform.position);
        }

        //
        // networks of neurons ala' computer scientist
        List<GameObject> neurons = new List<GameObject>();
        List<(int, int)> axons = new List<(int, int)>();
        int layers = 5;
        int layerSize = 10;
        int ncounter = 0;
        for (var i = 0; i < 1; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "ArenaN-" + i.ToString();

            //spawn spheres
            for (var j = 0; j < layers; j++)
            {

                //node.GetComponent<CreatureBehaviour>().SetOscillate(true);
                //node.GetComponent<CreatureBehaviour>().SetPhase(j * (i * 0.01f));
                for (var k = 0; k < layerSize; k++)
                {
                    GameObject node = createTree(sphere3Prefab, x0 + j * 3f-5f, 10f , z0 + 2*k-5f, 0.5f);

                    GameObject body = node.transform.Find("Sphere").gameObject;
                    body.transform.position = node.transform.position;
                    node.GetComponent<Rigidbody>().isKinematic = true;
                    neurons.Add(node);

                    if (j < layers - 1)
                    {
                        for (var n = 0; n < layerSize; n++)
                        {
                            int connectedAgent = (i * layers) + ((j + 1) * layerSize) + n;
                            Debug.Log(ncounter + " - " + connectedAgent);

                            axons.Add((ncounter, connectedAgent));
                            

                        }
                    }
                    body.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                    ncounter++;
                }
                //}

            }
            x0 += xzLim * 3;
        }
        Debug.Log(neurons.Count);
        //connections
        foreach (var axon in axons)
        {
            //GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);            
            //cylinder.transform.localScale = new Vector3(0.1f, 0.9f, 0.2f);
            //cylinder.transform.SetParent(agents[connection.Item1].transform);

            GameObject myLine = new GameObject();
            myLine.transform.position = neurons[axon.Item1].transform.position;
            myLine.AddComponent<LineRenderer>();
            LineRenderer line = myLine.GetComponent<LineRenderer>();
            line.material = neurons[axon.Item1].GetComponent<MeshRenderer>().material;
            line.material.SetColor("_Color", Random.ColorHSV());
            line.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(Random.Range(0f, 1f), 0.7f, 0.7f, Random.Range(0f,1f)));
            line.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            line.SetColors(Color.white, Color.white);
            line.SetWidth(0.05f, 0.05f);

            line.SetPosition(0, neurons[axon.Item1].transform.position);
            line.SetPosition(1, neurons[axon.Item2].transform.position);
        }

        // springy network of agents


        int scounter = 0;
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "ArenaN-" + i.ToString();

            //spawn spheres
            for (var j = 0; j < creatureNum * 3; j++)
            {

                //node.GetComponent<CreatureBehaviour>().SetOscillate(true);
                //node.GetComponent<CreatureBehaviour>().SetPhase(j * (i * 0.01f));
                GameObject node = createTree(sphere3Prefab, x0 + Random.Range(0f, xzLim * 1.2f) - xzLim / 2, Random.Range(3f, 6f), z0 + Random.Range(0f, xzLim * 1.2f) - xzLim / 2, 1f);
                GameObject body = node.transform.Find("Sphere").gameObject;
                body.transform.position = node.transform.position;
                node.GetComponent<Rigidbody>().isKinematic = false;
                node.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(0f,5f)-2.5f, Random.Range(0f, 5f) - 2.5f, Random.Range(0f, 5f) - 2.5f)*i*10;
                //node.GetComponent<Rigidbody>().detectCollisions = true;
                sagents.Add(node);
                //if (Random.Range(0f, 1f) < 0.5)
                //{
                int connectedAgent = (i * creatureNum * 3) + (j + Random.Range(1, creatureNum * 3)) % (creatureNum * 3);
                sconnections.Add((scounter, connectedAgent));
                body.GetComponent<Renderer>().material.SetColor("_Color", Random.ColorHSV());
                scounter++;

                //}

            }
            x0 += xzLim * 3;
        }
        //Debug.Log("Agents" + agents.Count);
        //Debug.Log("connections" + connections.Count);
        //connections
        foreach (var connection in sconnections)
        {
            //GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);            
            //cylinder.transform.localScale = new Vector3(0.1f, 0.9f, 0.2f);
            //cylinder.transform.SetParent(agents[connection.Item1].transform);
            //Debug.Log("connection" + connection.Item1 + " - " + connection.Item2);
            SpringJoint spring = sagents[connection.Item1].AddComponent<SpringJoint>();
            //spring.autoConfigureConnectedAnchor = false;
            spring.connectedBody = sagents[connection.Item2].GetComponent<Rigidbody>();
            spring.minDistance = 0.5f;
            spring.maxDistance = 1f;
            spring.spring = 30f;
            spring.damper = 0.05f;

            GameObject myLine = new GameObject();
            myLine.transform.position = sagents[connection.Item1].transform.position;
            myLine.transform.SetParent(sagents[connection.Item1].transform);
            myLine.AddComponent<LineRenderer>();
            LineRenderer line = myLine.GetComponent<LineRenderer>();
            line.material = sagents[connection.Item1].GetComponent<MeshRenderer>().material;
            line.material.SetColor("_Color", Random.ColorHSV());
            line.GetComponent<Renderer>().material.SetColor("_EmissionColor", Random.ColorHSV());
            line.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            line.SetColors(Color.white, Color.white);
            line.SetWidth(0.1f, 0.1f);

            line.SetPosition(0, sagents[connection.Item1].transform.position);
            line.SetPosition(1, sagents[connection.Item2].transform.position);

            snetrenderers.Add(line);
        }

        // Colorful reaction (edges appearance)
        // 2 colliding spheres -> k-cnets 
        //




    }

    // Fixed Update is called at a set interval, and deals with the physics & tick advances
    void FixedUpdate()
    {
        Ticks += 1; //count up a Tick at each physics update

        //Start a new generation after number of ticks reaches resetRate
        if (Ticks > resetRate)
        {
            //Debug.Log(Time.time); //log reset time --;
            if (Generation >= genNum - 1)
            {
                //UnityEditor.EditorApplication.isPlaying = false; //stop play mode when we reach max number of generations
                //Application.Quit(); Use the above instead when in testing mode
            }
            else
            {
                //    NewGeneration(global, rHigh); //runs the new generation method
                Ticks = 0; //set Ticks back to 0 
            }
        }
        //lines updates
        var lineCounter = 0;
        foreach (var connection in sconnections)
        {
            snetrenderers[lineCounter].SetPosition(0, sagents[connection.Item1].transform.position);
            snetrenderers[lineCounter].SetPosition(1, sagents[connection.Item2].transform.position);
            lineCounter++;
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

        GameObject createTree(GameObject pref,float x, float y, float z,float size)
    {
        var go = Instantiate(pref) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        //go.transform.localScale = new Vector3(Random.Range(0.3f, 1f), Random.Range(0.3f, 15f), Random.Range(1f, 15f));
        go.transform.localScale = new Vector3(size, size, size);

        go.transform.localRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);

        //var tree = go.GetComponent<ProceduralTree>();
        //tree.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
        //tree.Data.randomSeed = Random.Range(0, 300);
        return go;
    }

    void createSpeedTree(float x, float y, float z)
    {
        var go = Instantiate(trees2Prefab) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        //go.transform.localScale = new Vector3(Random.Range(0.3f, 1f), Random.Range(0.3f, 15f), Random.Range(1f, 15f));
        go.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        go.transform.localRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);

        //var tree = go.GetComponent<ProceduralTree>();
        //tree.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
        //tree.Data.randomSeed = Random.Range(0, 300);
    }


    //Function to create the next generation:
    //Adds the surviving creatures to a reproducers list in one of 2 ways (Global or Local competition)
    //Then repopulates the patches in one of 2 ways (High or Low relatedness)
    void NewGenerationold(bool global, bool rHigh)
    {
        string[] creatureTags = {"Grabbed", "Creature", "GrabTargeting", "StingTargeting"}; //tags associated with non-inert creatures

        

        //Global competition: Add all non-inert creatures to parentList, then rank and clip the list
        if (global == true)
        {
            foreach (string tag in creatureTags)
            {
                GameObject[] creatureList0 = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject creature in creatureList0)
                {
                    if (tag == "Grabbed")
                    {
                        creature.transform.parent = null; //if it was grabbed, detach it from parent
                    }
                    creature.tag = "OldGeneration"; //tag it to be disabled
                    parentList.Add(creature); //add to parentList
                }
            }
            //Debug.Log("Original ParentsList: " + parentList.Count + " items");
            //rank and clip the parentList
            List<GameObject> parentList_ranked = parentList.OrderByDescending(creature => creature.GetComponent<CreatureBehaviour>().points).Take(patchNum).ToList();
            parentList = parentList_ranked;
            //Debug.Log("Ranked and clipped List: " + parentList.Count + " items");
            foreach (GameObject creature in parentList)
            {
                Debug.Log(creature.GetComponent<CreatureBehaviour>().points);
            }

        } 
        //Local competition: Go through each patch and choose the highest-scoring creature 
        else
        { 
            foreach (GameObject arena in arenaList)
            {
                //first make list of non-inert creatures left in patch
                List<GameObject> creatureList = new List<GameObject>(); //create empty list 
                foreach (Transform child in arena.transform)
                {
                    if (creatureTags.Contains(child.tag)) //if child has a non-inert tag...
                    {
                        if (child.tag == "Grabbed")
                        {
                            child.transform.parent = arena.transform; //if it was grabbed by a creature, change parent back to arena
                        }
                        creatureList.Add(child.gameObject); //add to creatureList
                    }
                }
                //Now go through creatureList and find highest scorer (choose randomly if tie)
                int maxPoints = 0;
                int rand = Random.Range(0, creatureList.Count);
                GameObject highScorer = creatureList[rand];
                foreach (GameObject creature in creatureList)
                {
                    int points = creature.GetComponent<CreatureBehaviour>().points; //get the creature's points count
                    if (points > maxPoints)
                    {
                        highScorer = creature;
                        maxPoints = points;
                    }
                    creature.tag = "OldGeneration"; //Tag it to be disabled
                }
                parentList.Add(highScorer);
                Debug.Log("High score: " + maxPoints); 
            }
            Debug.Log("ParentsList: " + parentList.Count + " items");
        }

        

        //Clear out old creatures & food
        DisableTag("OldGeneration"); //Disable previous non-inert creatures
        DestroyTag("Inert"); //Destroy inert creatures
        DestroyTag("Pick Up"); //Destroy previous food 

        //Repopulate the patches for the next generation:
        Generation += 1; //begin the next generation!
        x0 = 0f; //starting coordinates for patch 0
        z0 = 0f;
        //If High relatedness: populate each new patch by going through parentList and cloning each member creatureNum times
        if (rHigh == true)
        {
            for (int i = 0; i < patchNum; i++)
            {
                GameObject parent = parentList[i]; //each item in parentsList is the single parent of a patch
                //clone this parent creatureNum times (with small chance of mutation at each locus)
                for (int j = 0; j < creatureNum; j++)
                {
                    GameObject newbaby = CreateCreature(x0, z0, parent);
                    newbaby.transform.SetParent(arenaList[i].transform, true);
                    WriteGenome(newbaby);
                }
                SpawnFood(x0, z0, foodAmount); //Spawn initial food in patch
                //advance to next patch location
                if (i % 10 == 9)
                {
                    z0 = -(xzLim * 3) * (i + 1) / 10;
                    x0 = 0f;
                }
                else
                {
                    x0 += xzLim * 3;
                }
            } 
        }
        //If Low relatedness: clone each item in parentList creatureNum times, then shuffle the list, and divide into patches
        else
        {
            //Clone each item in parentList creatureNum times
            List<GameObject> newList = new List<GameObject>(); //create new empty list for clones 
            foreach (GameObject parent in parentList)
            {
                for (int i = 0; i < creatureNum; i++)
                {
                    GameObject newbaby = CreateCreature(x0, z0, parent); //place all creatures in the first patch for now
                    newList.Add(newbaby);
                    newbaby.SetActive(false); //make sure the creatures are inactive until they are called to populate a patch
                }
            }
            //Do a random shuffle on the newList
            for (int j = 0; j < newList.Count; j++)
            {
                GameObject temp = newList[j];
                int randomIndex = Random.Range(j, newList.Count);
                newList[j] = newList[randomIndex];
                newList[randomIndex] = temp;
            }
            Debug.Log("Clone List: " + newList.Count + " items");
            //Now we sequentially grab createNum creatures at a time from the newList and place them in patches
            int n = 0;
            for (int k = 0; k < patchNum; k++)
            {
                for (int l = n; l < n + creatureNum; l++)
                {
                    GameObject newbaby = newList[l];
                    newbaby.transform.position = new Vector3(Random.Range(x0 - xzLim, x0 + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z0 - xzLim, z0 + xzLim)); //change the position to be in the current patch
                    newbaby.transform.SetParent(arenaList[k].transform, true); //parent the creature to its arena
                    newbaby.SetActive(true); //re-activate the creature
                    WriteGenome(newbaby);
                }
                SpawnFood(x0, z0, foodAmount); //Spawn initial food in patch
                n += creatureNum; //advance sliding window in newList
                //advance to next patch location
                if (k % 10 == 9)
                {
                    z0 = -(xzLim * 3) * (k + 1) / 10;
                    x0 = 0f;
                }
                else
                {
                    x0 += xzLim * 3;
                }
            }
        }
        parentList = new List<GameObject>(); //empty the parentList for the next generation
        Debug.Log("Empty List: " + parentList.Count + " items");
    }

    void NewGeneration(bool global, bool rHigh)
    {
        Generation += 1; //begin the next generation!
        x0 = 0f; //starting coordinates for patch 0
        z0 = 0f;
        float radius = 0;
        float inclination = 0;
        float azimuth = 0;
        GameObject root = CreateCreature(3, 3);
        //List<GameObject> branches = new List<GameObject>();
        //branches.Add(root);


        for (var j = 0; j < creatureNum; j++)
        {          
            

            radius = j * 2;
            inclination += Random.Range(0.1f, 0.3f);
            azimuth += Random.Range(0.1f, 0.3f);
            float x = (float)(radius * Math.Sin(inclination) * Math.Cos(azimuth));
            float y = (float)(radius * Math.Sin(inclination) * Math.Sin(azimuth));
            float z = (float)(radius * Math.Cos(inclination));
            GameObject newbaby = CreateCreature(x,y,z);
            newbaby.transform.SetParent(mainarena.transform, true);

            //branches.RemoveAt(0);

            WriteGenome(newbaby);
        }
        parentList = new List<GameObject>();
    }

    //Function to create a new creature, either with deafault/random gene values, or clone of a parent, with some chance of mutation
    GameObject CreateCreature(float x, float z, GameObject parent = null)
    {
        //Vector3 position = new Vector3(Random.Range(x - xzLim, x + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z - xzLim, z + xzLim)); //set random position within patch
        Vector3 position = new Vector3(x, 7, z);
        GameObject newbaby = Instantiate(CreaturePrefab, position, Quaternion.identity);
        //if parent == null, do nothing (keep default gene values). Else...
        if (parent != null)
        {
            //copy parent's grabber pref (with chance of mutation)
            float rand = Random.value;
            if (rand <= mutationRate)
            {
                newbaby.GetComponent<Genome>().GrabberPref = Random.value;
                Debug.Log("Mutation!");
            }
            else
            {
                newbaby.GetComponent<Genome>().GrabberPref = parent.GetComponent<Genome>().GrabberPref;
            }
            //copy parent's leg functions (with chance of mutation at each leg)
            int[] LegGenes = newbaby.GetComponent<Genome>().LegFunction;
            int[] ParentLegGenes = parent.GetComponent<Genome>().LegFunction;
            for (var i = 0; i < LegGenes.Length; i++)
            {
                float rand4 = Random.value;
                if (rand4 <= mutationRate)
                {
                    LegGenes[i] = Random.Range(0, 3);
                    Debug.Log("Mutation!");
                }
                else
                {
                    LegGenes[i] = ParentLegGenes[i];
                }
            }
            newbaby.GetComponent<Genome>().LegFunction = LegGenes;
        }
        return newbaby;
    }
    GameObject CreateCreature(float x,float y, float z, GameObject parent = null)
    {
        //Vector3 position = new Vector3(Random.Range(x - xzLim, x + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z - xzLim, z + xzLim)); //set random position within patch
        Vector3 position = new Vector3(x, y, z);
        GameObject newbaby = Instantiate(CreaturePrefab, position, Quaternion.identity);
        //if parent == null, do nothing (keep default gene values). Else...
        if (parent != null)
        {
            //copy parent's grabber pref (with chance of mutation)
            float rand = Random.value;
            if (rand <= mutationRate)
            {
                newbaby.GetComponent<Genome>().GrabberPref = Random.value;
                Debug.Log("Mutation!");
            }
            else
            {
                newbaby.GetComponent<Genome>().GrabberPref = parent.GetComponent<Genome>().GrabberPref;
            }
            //copy parent's leg functions (with chance of mutation at each leg)
            int[] LegGenes = newbaby.GetComponent<Genome>().LegFunction;
            int[] ParentLegGenes = parent.GetComponent<Genome>().LegFunction;
            for (var i = 0; i < LegGenes.Length; i++)
            {
                float rand4 = Random.value;
                if (rand4 <= mutationRate)
                {
                    LegGenes[i] = Random.Range(0, 3);
                    Debug.Log("Mutation!");
                }
                else
                {
                    LegGenes[i] = ParentLegGenes[i];
                }
            }
            newbaby.GetComponent<Genome>().LegFunction = LegGenes;
        }
        newbaby.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        newbaby.transform.GetChild(0);
        /*
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Rigidbody gameObjectsRigidBody = cylinder.AddComponent<Rigidbody>();
        cylinder.transform.localScale = new Vector3 (0.1f,0.9f, 0.2f);
        cylinder.transform.SetParent(newbaby.transform);
        SpringJoint springJoint = cylinder.AddComponent<SpringJoint>();
        */
        //GameObject cylinder2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //cylinder.transform.localScale = new Vector3(0.1f, 0.9f, 0.2f);
        //cylinder.transform.SetParent(cylinder.transform);

        return newbaby;
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
        WriteData(genomeData);
    }

    //Function to spawn food
    void SpawnFood(float x, float z, int foodAmount)
    {
        for (var i = 0; i < foodAmount; i++)
        {
            Vector3 position = new Vector3(Random.Range(x - xzLim, x + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z - xzLim, z + xzLim));
            Instantiate(FoodPrefab, position, Quaternion.identity);
        }
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

    //Function to write data
    void WriteData(string data)
    {
        StreamWriter writer = new StreamWriter(writepath, true);
        writer.WriteLine(data);
        writer.Close();
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



