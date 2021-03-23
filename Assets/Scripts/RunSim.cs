using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using ChartAndGraph;

public class RunSim : MonoBehaviour
{

    //Inputs
    public GameObject ArenaPrefab;
    public GameObject CreaturePrefab;
    public GameObject FoodPrefab;
    public GraphChart chart; // current generation creature count graph
    public GraphChart genChart; // overall generation creature count graph
    public GraphChart genChart2; // overall generation sting and grab events graph
    public PieChart pieChart; // current generation stringer/grabber proportion pie chart
    public BarChart barChart; // current generation sting and grab event
    public GameObject rollToggle; // rolling graph toggle
    public GameObject histToggle; // historical data toggle
    public Genome genome;
    public CreatureBehaviour creatureBehaviour;
    public string writepath = "Assets/Data/data.csv"; //write simulation data to this filename

    //Simulation parameters (which can be altered before a run)
    public bool global = true; //Global competetion between creatures? (if false, Local competiton)
    public bool rHigh = true; //High relatedness within a patch? (if false, Low relatedness)
    public int patchNum = 100; //number patches per generation
    public int creatureNum = 12; //number creatures per patch
    public int foodAmount = 24; //starting amount of food per patch
    public float foodProb = 0.01f; //rate of spontaneous food production in patch
    public float mutationRate = 0.01f; //mutation rate
    public int resetRate = 500; // number of ticks after which new generation begins
    public int genNum = 10; //total number of generations after which simulation stops
    public float arenaSize = 30f; // length of side of square arena
    public float minimumHeight = 2f; //min height of object placement in patch
    public float maximumHeight = 7f; //max height of object placement in patch
    public int currentCount = 0; // current number of organisms in generation
    public int stings; // current number of sting events in generation
    public int grabs; // current number of grab event in generation
 
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
    private int lastPoll; // last point added to graph (for conditional point addition)
    private double startTime;
    private bool showHist = true; // boolean toggle to show history
    private bool rolling = false; // boolean toggle for rolling graph

    // demo cam
    public Camera dirCamera;
    public int arenaFocus=0;
    private GameObject leadAgent;
    private GameObject grabAgent;
    private bool followLead = false;
    private bool followGrab = false;
    private int following = 0;

    // Graphing defaults
    public Material lineMaterial, fillMaterial,pointMaterial;
    public Material oldLineMaterial, oldFillMaterial, oldPointMaterial;
    private bool stretchFill = false;
    private double pointSize = 10.0;
    private double lineThickness = 4.0;


    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 30;
        xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed
        startTime = Time.time;
        //write file header
        WriteData("Gen, Patch, NumGrabbers, NumStingers, GrabberPref");

        // Initial chart setup
        SetInitialViews();

        //Spawn patches (arenas)
        x0 = 0f;
        z0 = 0f;
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "Arena" + i.ToString();


            //Spawn initial creatures, each with only 1 grabber
            for (var j = 0; j < creatureNum; j++)
            {
                GameObject newbaby = CreateCreature(x0, z0);
                newbaby.transform.SetParent(arena.transform, true);
                WriteGenome(newbaby);
            }

            //Spawn initial food in random locations
            SpawnFood(x0, z0, foodAmount);

            //advance to next patch location
            if (i % 10 == 9)
            {
                z0 = -(xzLim * 3) * (i+1)/10;
                x0 = 0f;
            }
            else
            {
                x0 += xzLim * 3;
            } 
        }

        arenaList = GameObject.FindGameObjectsWithTag("Arena"); //fill the array of arenas
        Generation = 0;
        Ticks = 0;
        lastPoll = currentCount; // initialise previous creature counter
    }

    // Fixed Update is called at a set interval, and deals with the physics & tick advances
    void FixedUpdate()
    {
        Ticks += 1; //count up a Tick at each physics update

        //Start a new generation after number of ticks reaches resetRate
        if (Ticks > resetRate)
        {
            Debug.Log(Time.time); //log reset time
            if (Generation >= genNum - 1)
            {
                UnityEditor.EditorApplication.isPlaying = false; //stop play mode when we reach max number of generations
                //Application.Quit(); Use the above instead when in testing mode
            }
            else
            {
                updateOverallGraphs(); // record generation data on graphs
                NewGeneration(global, rHigh); //runs the new generation method
                Ticks = 0; //set Ticks back to 0 
            }
        }
    }
    void Update()
    {
        updateCurrentGraph(); // update current generation creature count graph
        if (Input.GetKeyDown("1"))
        {
            //patchNum = 1;
            followLead = false;
            followGrab = false;
            dirCamera.transform.position = arenaList[arenaFocus].transform.position + new Vector3(0f, 40f, 0);
            arenaFocus++;
            arenaFocus = arenaFocus % patchNum;
            following = 0;

            Debug.Log("Focus arena +1");
            //Start();
        }
        if (Input.GetKeyDown("2"))
        {
            //patchNum = 1;
            followLead = false;
            followGrab = false;
            dirCamera.transform.position = new Vector3(35f*5, 200+(12* patchNum / 10), -35f*(patchNum/20));
            following = 0;
            //Start();
        }

        if (Input.GetKeyDown("3"))
        {
            // camera.transform.position = leadAgent.transform.position;
            //camera.transform.position = leadAgent.transform.position + new Vector3(0, 30, 0);
            var agents = GameObject.FindGameObjectsWithTag("Grabbed");
            agents.OrderByDescending(creature => creature.GetComponent<CreatureBehaviour>().points).Take(patchNum).ToList();
            if(agents.Length > 0)
            {
                leadAgent = agents[agents.Length-1];
            }
            
            followLead = true;
            followGrab = false;
            following = 1;
        }
        if (following == 1)
        {
            dirCamera.transform.position = leadAgent.transform.position + new Vector3(0, 20, 0);

        }
        if (Input.GetKeyDown("4"))
        {
            followGrab = true;
            followLead = false;
            following = 2;
            //grabAgent.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(100,0,0));
        }
        if (following == 2)
        {
            //dirCamera.transform.position = grabAgent.transform.position + new Vector3(0, 20, 0);
            var agents = GameObject.FindGameObjectsWithTag("Creature");
            agents.OrderByDescending(creature => creature.GetComponent<CreatureBehaviour>().points).Take(patchNum).ToList();
            grabAgent = agents[0];
            
            Debug.Log("agents num: " + agents.Length);
            for (int i = 0; i < agents.Length; i++)
            {
                agents[i].transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color((i * 1.0f / (creatureNum * patchNum * 1.0f)), (i / (creatureNum * patchNum * 1.0f)), 0));
                
                // agents[i].transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(((i*1.0f) / (creatureNum * patchNum * 1.0f)), 0, 0));

            }

        }
        // colour only those with points
        if (Input.GetKeyDown("5"))
        {
            followGrab = true;
            followLead = false;
            following = 3;

            //grabAgent.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(100,0,0));
        }
        if (following == 3)
        {
            //dirCamera.transform.position = grabAgent.transform.position + new Vector3(0, 20, 0);
            var agents = GameObject.FindGameObjectsWithTag("Creature");
            agents.OrderByDescending(creature => creature.GetComponent<CreatureBehaviour>().points).Take(patchNum).ToList();
            grabAgent = agents[0];

            Debug.Log("agents num: " + agents.Length);
            for (int i = 0; i < agents.Length; i++)
            {
                if (agents[i].GetComponent<CreatureBehaviour>().points > 0)
                {
                    agents[i].transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color((i * 1.0f / (creatureNum * patchNum * 1.0f)), (i / (creatureNum * patchNum * 1.0f)), 0));
                } else
                {
                    agents[i].transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.1f, 0.1f, 0));
                }
                // agents[i].transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(((i*1.0f) / (creatureNum * patchNum * 1.0f)), 0, 0));

            }

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

        //Function to create the next generation:
        //Adds the surviving creatures to a reproducers list in one of 2 ways (Global or Local competition)
        //Then repopulates the patches in one of 2 ways (High or Low relatedness)
        void NewGeneration(bool global, bool rHigh)
    {
        string[] creatureTags = {"Grabbed", "Creature", "GrabTargeting", "StingTargeting"}; //tags associated with non-inert creatures

        // reset current generation counters.
        currentCount = 0;
        stings = 0;
        grabs = 0;

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
            Debug.Log("Original ParentsList: " + parentList.Count + " items");
            //rank and clip the parentList
            List<GameObject> parentList_ranked = parentList.OrderByDescending(creature => creature.GetComponent<CreatureBehaviour>().points).Take(patchNum).ToList();
            parentList = parentList_ranked;
            
            Debug.Log("Ranked and clipped List: " + parentList.Count + " items");
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

        // create new category for new generation
        var lineTiling = new MaterialTiling(true,20);
        chart.DataSource.AddCategory("Gen" + Generation, lineMaterial,lineThickness,lineTiling,fillMaterial,stretchFill,pointMaterial,pointSize);
        
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

    //Function to create a new creature, either with default/random gene values, or clone of a parent, with some chance of mutation
    GameObject CreateCreature(float x, float z, GameObject parent = null)
    {
        Vector3 position = new Vector3(Random.Range(x - xzLim, x + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z - xzLim, z + xzLim)); //set random position within patch
        GameObject newbaby = Instantiate(CreaturePrefab, position, Quaternion.identity);
        currentCount++; // update counter of organisms
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

        // setup pie chart
        pieChart.DataSource.SetValue("Stingers", SGList[0] + pieChart.DataSource.GetValue("Stingers"));
        pieChart.DataSource.SetValue("Grabbers", SGList[1] + pieChart.DataSource.GetValue("Grabbers"));
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

    // Function to setup initial graph scales and views.
    void SetInitialViews() {
        chart.DataSource.Clear();

        chart.DataSource.HorizontalViewSize = resetRate / 50;
        chart.DataSource.HorizontalViewOrigin = 0;
        chart.DataSource.VerticalViewSize = (0.15 * (patchNum * creatureNum)) + 1;
        chart.DataSource.VerticalViewOrigin = 0.85 * (patchNum * creatureNum);

        genChart.DataSource.HorizontalViewSize = Mathf.Round(Mathf.Log10(genNum));
        genChart.DataSource.HorizontalViewOrigin = 0;
        genChart.DataSource.VerticalViewSize = (0.15 * (patchNum * creatureNum));
        genChart.DataSource.VerticalViewOrigin = 0.85 * (patchNum * creatureNum);

        genChart2.DataSource.HorizontalViewSize = Mathf.Round(Mathf.Log10(genNum));
        genChart2.DataSource.HorizontalViewOrigin = 0;
        genChart2.DataSource.VerticalViewOrigin = 0;    
        genChart2.DataSource.VerticalViewSize = 500;

        var lineTiling = new MaterialTiling(true,20);
        chart.DataSource.AddCategory("Gen0", lineMaterial,lineThickness,lineTiling,fillMaterial,stretchFill,pointMaterial,pointSize);
    }
    
    // Helper function to add a point to the current generation creature count graph
    void updateCurrentGraph() {
        // conditions to add new point to current generation graph
        // will add a point to the graph if the creature count decreases (death), or after a timer expires.
        if(lastPoll > currentCount || Ticks % (0.1 * resetRate) == 0) {
            // check scale to ensure adequacy
            if(currentCount >= (chart.DataSource.VerticalViewOrigin + chart.DataSource.VerticalViewSize)) {
                chart.DataSource.VerticalViewSize += 50;
            } else if(currentCount <= chart.DataSource.VerticalViewOrigin) {
                chart.DataSource.VerticalViewOrigin -= 50;
            }

            // add point to current generation graph, and update timer
            chart.DataSource.AddPointToCategoryRealtime("Gen" + Generation, (Time.time - startTime) - (Generation * (resetRate / 50)), currentCount, 0.5f);
            lastPoll = currentCount;
        }

        // update bar chart values of sting and grab events (only if necessary)
        if(barChart.isActiveAndEnabled) {
            barChart.DataSource.SetValue("Stings", "All", stings);
            barChart.DataSource.SetValue("Grabs", "All", grabs);
        }
    }

    // Helper function to add points to the overall generation graphs.
    void updateOverallGraphs() {
        double log = Mathf.Round(Mathf.Log10(Generation + 1) * 100) / 100;
                
        // ensure scale view is appropiate
        alterScale(genChart, currentCount);
        alterScale(genChart2, stings);
        alterScale(genChart2, grabs);

        // add data points to graphs
        genChart.DataSource.AddPointToCategory("GenTotal", log, currentCount, 1f);
        genChart2.DataSource.AddPointToCategory("GenStings", log, stings, 1f);
        genChart2.DataSource.AddPointToCategory("GenGrabs", log, grabs, 1f);
        
        // edit old lines so they're aren't as visible.
        var lineTiling = new MaterialTiling(true, 20);
        SetLineFade(lineTiling, Generation);
    }
 
    // Update line material for a generation
    void SetLineFade(MaterialTiling lineTiling, int Generation) {
        for(int i = 1; i < Generation + 2; i++) {
            double percentage = i / (Generation + 1);
            chart.DataSource.SetCategoryLine("Gen" + (i - 1), oldLineMaterial, lineThickness * percentage, lineTiling);
            chart.DataSource.SetCategoryFill("Gen" + (i - 1), oldFillMaterial, stretchFill);
            chart.DataSource.SetCategoryPoint("Gen" + (i - 1), oldPointMaterial, (pointSize * percentage));
            chart.DataSource.SetCategoryEnabled("Gen" + (i - 1), showHist);
        }
    }

    // Helper function to toggle rolling graph
    public void toggleRolling() {
        rolling = !rolling;
        chart.AutoScrollHorizontally = rolling;
        chart.DataSource.AutomaticHorizontalView = rolling;

        if(!rolling) {
            chart.DataSource.HorizontalViewSize = resetRate / 50;
            chart.DataSource.HorizontalViewOrigin = 0;
        }        
    }

    // Helper function to toggle historical graphing overlay
    public void toggleHistory() {
        showHist = !showHist;
        for(int i = 0; i < Generation; i++) {
            chart.DataSource.SetCategoryEnabled("Gen" + i, showHist);
        }
    }

    // Helper function to toggle a graph's visibility (i.e. Panel object)
    public void toggleGraph(GameObject panel) {
        panel.SetActive(!panel.activeInHierarchy);
    }

    // helper function to adjust scales for visibility
    void alterScale(GraphChart graph, int current) {
        if(current >= (graph.DataSource.VerticalViewOrigin + graph.DataSource.VerticalViewSize)) {
            graph.DataSource.VerticalViewSize += 50;
        } else if(current <= graph.DataSource.VerticalViewOrigin) {
            graph.DataSource.VerticalViewOrigin -= 50;
        }
    }
}



