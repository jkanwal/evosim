using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class RunSim : MonoBehaviour
{
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
    public int patchNum = 10; //number patches per generation
    public int creatureNum = 12; //number creatures per patch
    public int foodAmount = 24; //starting amount of food per patch
    public float foodProb = 0.01f; //rate of spontaneous food production in patch
    public float mutationRate = 0.01f; //mutation rate
    public int resetRate = 500; // number of ticks after which new generation begins
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
    private List<GameObject> parentList = new List<GameObject>(); //create empty list of reproducers

    // Public lists accessible by the CreatureBehaviour script attached to each creature
    public List<GameObject> foodPoolList = new List<GameObject>(); //create empty pooling list for food 
    public List<GameObject> liveCreatureList = new List<GameObject>(); //create empty list for live creatures 

    public List<GameObject> inertCreatureList = new List<GameObject>(); //create empty list for inert creatures 
    


    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 30;

        xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed

        //write file header
        WriteData("Gen, Patch, NumGrabbers, NumStingers, GrabFood, GrabCreature, StingFood, StingCreature");

        //Spawn patches (arenas)
        x0 = 0f;
        z0 = 0f;
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "Arena" + i.ToString();

            //Spawn 1st gen creatures
            for (var j = 0; j < creatureNum; j++)
            {
                GameObject newbaby = CreateCreature(x0, z0); //spawn creature
                newbaby.transform.SetParent(arena.transform, true); //set current arena as its parent
                liveCreatureList.Add(newbaby); //add creatures to live list 
                WriteGenome(newbaby);
            }

            //Spawn initial food and disable
            for (var k = 0; k < foodAmount; k++)
                {
                    GameObject newfood = Instantiate(FoodPrefab, Vector3.zero, Quaternion.identity); //spawn all food at zero location (will change this later)
                    newfood.SetActive(false);
                    foodPoolList.Add(newfood); //add it to the pooling list
                }

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
    }

    // Fixed Update is called at a set interval, and deals with the physics & tick advances
    void FixedUpdate()
    {
        Ticks += 1; //count up a Tick at each physics update

        //Spawn food halfway through a generation
        if (Ticks == resetRate/2)
        {
            x0 = 0f;
            z0 = 0f;
            //spawn food in each patch
            for (var i = 0; i < patchNum; i++)
            {
                //re-activate food from pooling list
                for (var j = 0; j < foodAmount; j++)
                {
                    Vector3 position = new Vector3(Random.Range(x0 - xzLim, x0 + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z0 - xzLim, z0 + xzLim));
                    GameObject newfood = GetPooledObject(foodPoolList);
                    newfood.transform.position = position;
                    newfood.SetActive(true);
                }
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
        }

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
                NewGeneration(global, rHigh); //runs the new generation method
                Ticks = 0; //set Ticks back to 0 
            }
        }  
    }

    //Function to create the next generation:
    //Adds the surviving creatures to a reproducers list in one of 2 ways (Global or Local competition)
    //Then repopulates the patches in one of 2 ways (High or Low relatedness)
    void NewGeneration(bool global, bool rHigh)
    {

        //Disable all food in the pool list. If a list item is null, create a new piece of food in its place
        for (var f = 0; f < foodPoolList.Count; f++)
        {
            if (foodPoolList[f] == null) 
            {
                GameObject newfood = Instantiate(FoodPrefab, Vector3.zero, Quaternion.identity); //spawn piece of food at zero location
                newfood.SetActive(false);
                foodPoolList[f] = newfood;
            }
            else
            {
                foodPoolList[f].SetActive(false);
            }
        }
        
        //Global competition: 
        //First go through live creature listDetach & disable all creatures, add non-inert creatures to parentList, then rank and clip the list
        if (global == true)
        {
            foreach (GameObject creature in liveCreatureList)
            {
                creature.transform.parent = null; //detach creature from all parents (arena and other creature if it was grabbed)
                creature.SetActive(false); //disable creature
                //if creature is non-inert (i.e. alive in patch)...
                if (creature.CompareTag("Creature") || creature.CompareTag("Grabbed") || creature.CompareTag("GrabTargeting") || creature.CompareTag("StingTargeting"))
                {
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
                List<GameObject> creatureList = new List<GameObject>(); //create empty list for non-inert creatures in patch
                foreach (Transform child in arena.transform)
                {
                    child.parent = null; //detach creature from all parents (arena and other creature if it was grabbed)
                    child.gameObject.SetActive(false); //disable creature
                    //if child has a non-inert tag...
                    if (child.CompareTag("Creature") || child.CompareTag("Grabbed") || child.CompareTag("GrabTargeting") || child.CompareTag("StingTargeting"))
                    {
                        creatureList.Add(child.gameObject); //add to creatureList
                    }
                }
                //Now go through creatureList and find highest scorer (choose randomly if tie)                
                int rand = Random.Range(0, creatureList.Count);
                GameObject highScorer = creatureList[rand];
                int maxPoints = highScorer.GetComponent<CreatureBehaviour>().points;
                foreach (GameObject creature in creatureList)
                {
                    int points = creature.GetComponent<CreatureBehaviour>().points; //get the creature's points count
                    if (points > maxPoints)
                    {
                        highScorer = creature;
                        maxPoints = points;
                    }
                }
                parentList.Add(highScorer);
                Debug.Log("High score: " + maxPoints); 
            }
            Debug.Log("ParentsList: " + parentList.Count + " items");
        }

        //Repopulate the patches for the next generation:
        liveCreatureList.Clear(); //empty Creature list for next gen
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
                    liveCreatureList.Add(newbaby); //add to live creature list
                    WriteGenome(newbaby);
                }
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
                    liveCreatureList.Add(newbaby); //add to live creature list
                    WriteGenome(newbaby);
                }
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
        parentList.Clear(); //empty the parentList for the next generation
        Debug.Log("Empty List: " + parentList.Count + " items");
    }

    //Function to create a new creature, either with deafault/random gene values, or clone of a parent, with some chance of mutation
    GameObject CreateCreature(float x, float z, GameObject parent = null)
    {
        Vector3 position = new Vector3(Random.Range(x - xzLim, x + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z - xzLim, z + xzLim)); //set random position within patch
        GameObject newbaby = Instantiate(CreaturePrefab, position, Quaternion.identity);
        //if parent == null, do nothing (keep default gene values). Else...
        if (parent != null)
        {
            //copy parent's grab & sting behaviours (with chance of mutation)
            float rand1 = Random.value;
            if (rand1 <= mutationRate)
            {
                newbaby.GetComponent<Genome>().GrabFood = Random.value;
            }
            else
            {
                newbaby.GetComponent<Genome>().GrabFood = parent.GetComponent<Genome>().GrabFood;
            }
            float rand2 = Random.value;
            if (rand2 <= mutationRate)
            {
                newbaby.GetComponent<Genome>().GrabCreature = Random.value;
            }
            else
            {
                newbaby.GetComponent<Genome>().GrabCreature = parent.GetComponent<Genome>().GrabCreature;
            }
            float rand3 = Random.value;
            if (rand3 <= mutationRate)
            {
                newbaby.GetComponent<Genome>().StingFood = Random.value;
            }
            else
            {
                newbaby.GetComponent<Genome>().StingFood = parent.GetComponent<Genome>().StingFood;
            }
            float rand4 = Random.value;
            if (rand4 <= mutationRate)
            {
                newbaby.GetComponent<Genome>().StingCreature = Random.value;
            }
            else
            {
                newbaby.GetComponent<Genome>().StingCreature = parent.GetComponent<Genome>().StingCreature;
            }

            //copy parent's leg functions (with chance of mutation at each leg)
            int[] LegGenes = newbaby.GetComponent<Genome>().LegFunction;
            int[] ParentLegGenes = parent.GetComponent<Genome>().LegFunction;
            for (var i = 0; i < LegGenes.Length; i++)
            {
                float rand5 = Random.value;
                if (rand5 <= mutationRate)
                {
                    LegGenes[i] = Random.Range(0, 3);
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
        string grabFood = creature.GetComponent<Genome>().GrabFood.ToString();
        string grabCreat = creature.GetComponent<Genome>().GrabCreature.ToString();
        string stingFood = creature.GetComponent<Genome>().StingFood.ToString();
        string stingCreat = creature.GetComponent<Genome>().StingCreature.ToString();
        int[] LegGenes = creature.GetComponent<Genome>().LegFunction;
        int[] SGList = CountSG(LegGenes);
        string grabberNum = SGList[0].ToString();
        string stingerNum = SGList[1].ToString();
        //write new creature's genome data to file
        string genomeData = Generation.ToString() + "," + ArenaName + "," + grabberNum + "," + stingerNum + "," + grabFood + "," + grabCreat + "," + stingFood + "," + stingCreat;
        WriteData(genomeData);
    }

    //Function to get an object from a poolList
    GameObject GetPooledObject(List<GameObject> poolList) 
    {
        for (int i = 0; i < poolList.Count; i++) 
        {
            if (!poolList[i].activeInHierarchy) 
            {
                GameObject newItem = poolList[i];
                poolList.RemoveAt(i);
                poolList.Insert(poolList.Count-1, newItem);
                return newItem;
            }
        } 
        Debug.Log("no inactive objects left in pool!");
        return null;
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



