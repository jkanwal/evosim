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
    public Material foodColour;
    public Material creatureColour;
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
    private List<GameObject> arenaList = new List<GameObject>(); //create empty list of arenas
    private List<GameObject> foodPoolList = new List<GameObject>(); //create empty pooling list for food 
    private List<GameObject> creatureList = new List<GameObject>(); //create empty list to keep track of all creatures
    private List<GameObject> parentList0 = new List<GameObject>();  //create empty list to keep track of potential parents
    private List<GameObject> parentList = new List<GameObject>(); //create empty list for actual parents
    

    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 30;

        xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed

        //write file header
        WriteData("Gen, Patch, NumGrabbers, NumStingers, GrabFood, GrabCreature, StingFood, StingCreature, Points");

        //Spawn patches (arenas)
        x0 = 0f;
        z0 = 0f;
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            arena.name = "Arena" + i.ToString();
            arenaList.Add(arena); //add new arena to arenaList 

            //Spawn 1st gen creatures
            for (var j = 0; j < creatureNum; j++)
            {
                GameObject newbaby = CreateCreature(x0, z0); //spawn creature
                newbaby.transform.SetParent(arena.transform, true); //set current arena as its parent
                creatureList.Add(newbaby); //add to creatureList 
            }

            //Spawn entire food pool and disable
            for (var k = 0; k < foodAmount; k++)
                {
                    GameObject newfood = Instantiate(FoodPrefab, Vector3.zero, Quaternion.identity); //spawn food at zero location (will change position when re-enabling)
                    newfood.SetActive(false); //disable
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
            //Debug.Log(Time.time); //log reset time
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

        //Disable all food in the pool list, and set it back to default values
        for (var f = 0; f < foodPoolList.Count; f++)
        {   
            GameObject food = foodPoolList[f];
            food.SetActive(false); //disable
            food.transform.parent = null; //detach from parent if it was grabbed
            //if it was inert (stung)...
            if (food.CompareTag("Inert"))
            {
                food.tag = "Pick Up"; //set back to default tag
                food.layer = 9; //add it back to food layer
                Renderer rend = food.GetComponent<Renderer>();
                rend.material = foodColour; //change back to default colour
            }
        }

        //Go through all creatures. Write Data, disable and set aside the potential parents, and destroy the rest. Also clear the creatureList.
        for (int c = creatureList.Count - 1; c >= 0; c--)
        {
            GameObject creature = creatureList[c];
            WriteGenome(creature); //write its data
            //If creature is not grabbed or inert, add to parentlist0, otherwise destroy it
            if (!creature.CompareTag("Grabbed") && !creature.CompareTag("Inert"))
            {
                //creature.SetActive(false); //disable creature
                parentList0.Add(creature); //Add to potential parents list
            }
            else
            {
                creature.transform.parent = null; //detach from parent arena 
                Destroy(creature);
            }
            creatureList.RemoveAt(c);
        }
        
        //Global competition: Rank and clip parentList0
        if (global == true)
        {  
            parentList = parentList0.OrderByDescending(creature => creature.GetComponent<CreatureBehaviour>().points).Take(patchNum).ToList();
            /*
            Debug.Log("Ranked and clipped List: " + parentList.Count + " items");
            foreach (GameObject creature in parentList)
            {
                Debug.Log(creature.GetComponent<CreatureBehaviour>().points);
            }
            */
        } 

        //Local competition: Go through each patch and choose the single highest-scoring creature 
        else
        { 
            foreach (GameObject arena in arenaList)
            {
                int maxPoints = 0;
                GameObject highScorer = new GameObject();
                //look through non-destroyed top-level children of the arena (i.e. all non-grabbed or stung creatures), check for highest scorer
                foreach (Transform child in arena.transform)
                {
                    if (child.gameObject.layer == 10) //check only children in creature layer (ignore walls, cameras, etc.)
                    {
                        int points = child.gameObject.GetComponent<CreatureBehaviour>().points; //get the creature's points count
                        if (points > maxPoints)
                        {
                            highScorer = child.gameObject;
                            maxPoints = points;
                        }
                        else if (maxPoints == 0) 
                        {
                            highScorer = child.gameObject; // this is to make sure there's a non-empty highscorer, even if no one scored above 0 in this patch
                        }
                    }
                }
                parentList.Add(highScorer);
                //Debug.Log("High score: " + maxPoints); 
            }
        }

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
                    creatureList.Add(newbaby); //add to creature list
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
                    creatureList.Add(newbaby); //add to creature list
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

        //Destroy the remaining previous gen creatures, and clear parentList0 & parentList
        for (int p = parentList0.Count-1; p >= 0; p--)
        {   
            Destroy(parentList0[p]);
            parentList0.RemoveAt(p);
        }
        parentList.Clear();
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
        string ArenaName = creature.transform.root.name;
        int[] LegGenes = creature.GetComponent<Genome>().LegFunction;
        int[] SGList = CountSG(LegGenes);
        string grabberNum = SGList[0].ToString();
        string stingerNum = SGList[1].ToString();
        string grabFood = creature.GetComponent<Genome>().GrabFood.ToString();
        string grabCreat = creature.GetComponent<Genome>().GrabCreature.ToString();
        string stingFood = creature.GetComponent<Genome>().StingFood.ToString();
        string stingCreat = creature.GetComponent<Genome>().StingCreature.ToString();
        string points = creature.GetComponent<CreatureBehaviour>().points.ToString();
        //write new creature's genome data to file
        string genomeData = Generation.ToString() + "," + ArenaName + "," + grabberNum + "," + stingerNum + "," + grabFood + "," + grabCreat + "," + stingFood + "," + stingCreat + "," + points;
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



