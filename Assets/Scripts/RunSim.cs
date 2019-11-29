using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class RunSim : MonoBehaviour
{
    //Inputs
    public GameObject CreaturePrefab;
    public GameObject FoodPrefab;
    public Genome genome;
    public CreatureBehaviour creatureBehaviour;
    public string writepath = "Assets/Data/data.csv";

    //Simulation parameters
    public int creatureNum = 12;
    public int foodAmount = 24;
    public float foodProb = 0.01f;
    public float mutationRate = 0.01f;
    public float minimumHeight = 2f;
    public float maximumHeight = 7f;
    public float minSpeed = 10f;
    public float maxSpeed = 50f;
    public float maxRotationRange = 180f;
    public float resetRate = 30f;
    public float xzMin = -13f;
    public float xzMax = 13f;
    public int patchNum = 10;
    public int genNum = 10;

    //Other global variables
    public bool global = true;
    public bool rHigh = true;
    private float resetTime;
    private int Generation = 0;
    private int patch = 0;
    private List<GameObject> parentsList = new List<GameObject>(); //create empty list of reproducers
    private List<GameObject> parentsList_old; //keep track of parents from previous generation


    // Start is called before the first frame update
    void Start()
    {
        resetTime = resetRate;

        //write file header
        WriteData("Gen, Patch, NumGrabbers, NumStingers, MaxSpeed, MaxRotation, GrabberPref");

        //Spawn initial creatures, each with only 1 grabber
        for (var i = 0; i < creatureNum; ++i)
        {
            GameObject newbaby = CreateCreature();
            WriteGenome(newbaby);
        }

        //Spawn food in random locations
        SpawnFood(foodAmount);
    }


    // Update is called once per frame
    void Update()
    {
        //Constantly add new food at some slow rate
        float rand = Random.value;
        if (rand < foodProb)
        {
            Vector3 position = new Vector3(Random.Range(xzMin, xzMax), Random.Range(minimumHeight, maximumHeight), Random.Range(xzMin, xzMax));
            Instantiate(FoodPrefab, position, Quaternion.identity);
        }

        //Every time resetRate elapses, we start a new patch. 
        //If the number of patches reaches the patchNum, then we start a new generation
        if (Time.time > resetTime)
        {
            Debug.Log(Time.time);
            resetTime = Time.time + resetRate;
            AddParents(global); //Add the living creatures to the parentsList (either by global or local competition)
            DisableTag("OldGeneration"); //Disable previous creatures
            DestroyTag("Inert"); //Destroy dead creatures
            DestroyTag("Pick Up"); //Destroy previous food
            if (patch >= patchNum - 1)
            {
                if (Generation >= genNum - 1)
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                    //Application.Quit(); Use the above instead when in testing mode
                }
                else
                {
                    Debug.Log("Original ParentsList: " + parentsList.Count + " items");
                    //We're going to start a new generation, so process the reproducers list accordingly
                    if (global == true)
                    {
                        //sort list in order of points & cut off the list at the top 144
                        List<GameObject> parentsList_ranked = parentsList.OrderByDescending(creature => creature.GetComponent<CreatureBehaviour>().points).Take(patchNum).ToList();
                        parentsList = parentsList_ranked;
                        //Debugging:
                        Debug.Log("Ranked and clipped List: " + parentsList.Count + " items");
                        foreach (GameObject creature in parentsList)
                        {
                            Debug.Log(creature.GetComponent<CreatureBehaviour>().points);
                        }
                    }
                    if (rHigh == false)
                    {
                        //clone everyone in the list 12 times
                        List<GameObject> newList = new List<GameObject>(); //create new empty list for clones
                        foreach (GameObject parent in parentsList)
                        {
                            for (var i = 0; i < creatureNum; i++)
                            {
                                GameObject newbaby = CreateCreature(parent);
                                newList.Add(newbaby);
                                newbaby.SetActive(false); //make sure the creatures are inactive until they are called to populate a patch
                            }
                        }
                        //do a random shuffle on this list
                        for (int j = 0; j < newList.Count; j++)
                        {
                            GameObject temp = newList[j];
                            int randomIndex = Random.Range(j, newList.Count);
                            newList[j] = newList[randomIndex];
                            newList[randomIndex] = temp;
                        }
                        //set it as the old parentslist
                        parentsList = newList;
                        Debug.Log("Clone List: " + parentsList.Count + " items");
                    }
                    parentsList_old = parentsList;
                    parentsList = new List<GameObject>(); //empty the parentList for adding to from the current generation
                    Debug.Log("Empty List: " + parentsList.Count + " items");
                    Generation++;
                    patch = 0;
                }     
            }
            else
            {
                patch++;
            }
            SpawnFood(foodAmount); //Respawn the food
            if (Generation == 0)
            {
                //Spawn Gen 0 creatures, each with only 1 grabber
                for (var i = 0; i < creatureNum; ++i)
                {
                    GameObject newbaby = CreateCreature();
                    WriteGenome(newbaby);
                }
            }
            else
            {
                //Populate patch with creatures from parentsList_old (either rHigh or rLow)
                PopulatePatch(rHigh);
            } 
        }
    }


    //Function runs at the end of each patch. Adds the surviving creatures to a reproducers list in one of 2 ways (global or local competition)
    void AddParents(bool global)
    {
        //Get all non-inert creatures
        List<GameObject> creatureList = new List<GameObject>();
        string[] creatureTags = { "Grabbed", "Creature", "GrabTargeting", "StingTargeting"};
        foreach(string tag in creatureTags)
        {
            GameObject[] creatureList0 = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject creature in creatureList0)
            {
                if (tag == "Grabbed")
                {
                    creature.transform.parent = null; //if it was grabbed, detach it from parent
                }
                creatureList.Add(creature);
            }
        }
        //Add them to a reproducers list in one of 2 ways: 
        //global competition: Just add them all and get the top 144 when it's time to change generation
        if (global == true)
        {
            foreach (GameObject creature in creatureList)
            {
                parentsList.Add(creature); //add the creature to the list
                creature.tag = "OldGeneration"; //Tag it to be disabled
            }

        }
        //local competition: Only add highest scorer from patch (choose randomly if tie)
        else
        {
            int rand = Random.Range(0,creatureList.Count);
            GameObject highScorer = creatureList[rand]; //default winner is randomly chosen
            int maxPoints = 0;
            foreach (GameObject creature in creatureList)
            {
                int points = creature.GetComponent<CreatureBehaviour>().points; //get the points count of each creature
                if (points > maxPoints)
                {
                    highScorer = creature;
                    maxPoints = points;
                }
                creature.tag = "OldGeneration"; //Tag it to be disabled
            }
            parentsList.Add(highScorer);
            Debug.Log("High score: " + maxPoints + ", speed: " + highScorer.GetComponent<Genome>().maxSpeed);
        }
    }

    //Function to populate a new patch from the reproducer list
    void PopulatePatch(bool rHigh)
    {
        //Generate the new creatures in one of 2 ways (r high or r low):
        //High relatedness
        if (rHigh == true)
        {
            GameObject parent = parentsList_old[patch]; //each item in parentsList_old is the single parent of a patch
            //clone this parent creatureNum times (with small chance of mutation at each locus)
            for (var i = 0; i < creatureNum; i++)
            {
                GameObject newbaby = CreateCreature(parent);
                WriteGenome(newbaby);
            }

        }
        //Low relatedness 
        else
        {
            //Sequentially grab 12 creatures from parentsList_old
            for (var i = patch*creatureNum; i < patch*creatureNum + creatureNum; i++)
            {
                GameObject newbaby = parentsList_old[i];
                newbaby.SetActive(true); //activate the creatures in this patch
                WriteGenome(newbaby);
            }

        }
    }

    //Function to create a new creature, either with deafault/random gene values, or clone of a parent, with some chance of mutation
    GameObject CreateCreature(GameObject parent = null)
    {
        Vector3 position = new Vector3(Random.Range(xzMin, xzMax), Random.Range(minimumHeight, maximumHeight), Random.Range(xzMin, xzMax));
        GameObject newbaby = Instantiate(CreaturePrefab, position, Quaternion.identity);
        //min speed same for everyone
        newbaby.GetComponent<Genome>().minSpeed = minSpeed;
        //max speed
        if (parent == null)
        {
            newbaby.GetComponent<Genome>().maxSpeed = Random.Range(minSpeed + 1, maxSpeed);
        }
        else
        {
            float rand1 = Random.value;
            if (rand1 <= mutationRate)
            {
                newbaby.GetComponent<Genome>().maxSpeed = Random.Range(minSpeed + 1, maxSpeed);
                Debug.Log("Mutation!");
            }
            else
            {
                newbaby.GetComponent<Genome>().maxSpeed = parent.GetComponent<Genome>().maxSpeed;
            }
        }
        //rotation range
        float rand2 = Random.value;
        if (parent == null)
        {
            newbaby.GetComponent<Genome>().rotationRange = Random.Range(90f, maxRotationRange);
        }
        else
        {
            if (rand2 <= mutationRate)
            {
                newbaby.GetComponent<Genome>().rotationRange = Random.Range(90f, maxRotationRange);
                Debug.Log("Mutation!");
            }
            else
            {
                newbaby.GetComponent<Genome>().rotationRange = parent.GetComponent<Genome>().rotationRange;
            }
        }
        //grabber pref
        if (parent != null)
        {
            float rand3 = Random.value;
            if (rand3 <= mutationRate)
            {
                newbaby.GetComponent<Genome>().GrabberPref = Random.value;
                Debug.Log("Mutation!");
            }
            else
            {
                newbaby.GetComponent<Genome>().GrabberPref = parent.GetComponent<Genome>().GrabberPref;
            }
        }
        //leg functions
        if (parent != null)
        {
            int[] LegGenes = newbaby.GetComponent<Genome>().LegFunction;
            int[] ParentLegGenes = parent.GetComponent<Genome>().LegFunction;
            for (var i = 0; i < LegGenes.Length; ++i)
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
        string speed = creature.GetComponent<Genome>().maxSpeed.ToString();
        string rotation = creature.GetComponent<Genome>().rotationRange.ToString();
        string grabPref = creature.GetComponent<Genome>().GrabberPref.ToString();
        int[] LegGenes = creature.GetComponent<Genome>().LegFunction;
        int[] SGList = CountSG(LegGenes);
        string grabberNum = SGList[0].ToString();
        string stingerNum = SGList[1].ToString();
        //write new creature's genome data to file
        string genomeData = Generation.ToString() + "," + patch.ToString() + "," + grabberNum + "," + stingerNum + "," + speed + "," + rotation + "," + grabPref;
        WriteData(genomeData);
    }

    //Function to spawn food
    void SpawnFood(int foodAmount)
    {
        for (var i = 0; i < foodAmount; ++i)
        {
            Vector3 position = new Vector3(Random.Range(xzMin, xzMax), Random.Range(minimumHeight, maximumHeight), Random.Range(xzMin, xzMax));
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
                grabbers++;
            }
            else if (item == 2)
            {
                stingers++;
            }
        }
        int[] GSList = {grabbers, stingers};
        return GSList;
    }

}



