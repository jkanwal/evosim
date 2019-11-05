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
    public float resetRate = 60f;
    private float resetTime;

    // Start is called before the first frame update
    void Start()
    {
        resetTime = resetRate;

        //Spawn food in random locations
        SpawnFood(foodAmount);

        //Spawn initial creatures, each with 1 grabber
        for (var i = 0; i < creatureNum; ++i)
        {
            Vector3 position = new Vector3(Random.Range(-15f, 15f), Random.Range(minimumHeight, maximumHeight), Random.Range(-15f, 15f));
            GameObject baby = Instantiate(CreaturePrefab, position, Quaternion.identity);
            //keep all genes as default except set a random maxSpeed & rotationRange
            baby.GetComponent<Genome>().minSpeed = minSpeed;
            baby.GetComponent<Genome>().maxSpeed = Random.Range(minSpeed+1,maxSpeed);
            baby.GetComponent<Genome>().rotationRange = Random.Range(90f, maxRotationRange);            
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Constantly add new food at some slow rate
        float rand = Random.value;
        if (rand < foodProb)
        {
            Vector3 position = new Vector3(Random.Range(-15f, 15f), Random.Range(minimumHeight, maximumHeight), Random.Range(-15f, 15f));
            Instantiate(FoodPrefab, position, Quaternion.identity);
        }

        //Every time resetRate elapses, we respawn a set of new creatures based on who had the most points
        if (Time.time > resetTime)
        {
            resetTime = Time.time + resetRate;

            //Count creature success & Create new creatures
            Reproduction();
             
            //Also respawn the food
            DestroyTag("Pick Up");
            DestroyTag("Inert");
            SpawnFood(foodAmount);
        }
    }

    //Function to spawn food
    void SpawnFood(int foodAmount)
    {
        for (var i = 0; i < foodAmount; ++i)
        {
            Vector3 position = new Vector3(Random.Range(-15f, 15f), Random.Range(minimumHeight, maximumHeight), Random.Range(-15f, 15f));
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

    //Function to count creature success & reproduce accordingly
    void Reproduction()
    {
        List<GameObject> babiesList = new List<GameObject>(); //create empty list for creatures
        GameObject[] creatureList1 = GameObject.FindGameObjectsWithTag("Creature");
        GameObject[] creatureList2 = GameObject.FindGameObjectsWithTag("Grabbing");
        GameObject[] creatureList3 = GameObject.FindGameObjectsWithTag("GrabTargeting");
        GameObject[] creatureList4 = GameObject.FindGameObjectsWithTag("StingTargeting");
        GameObject[] creatureList5 = GameObject.FindGameObjectsWithTag("StingTargeting_G");
        GameObject[] creatureListA = creatureList1.Concat(creatureList2).ToArray();
        GameObject[] creatureListB = creatureList3.Concat(creatureList4).ToArray();
        GameObject[] creatureListC = creatureListA.Concat(creatureListB).ToArray();
        GameObject[] creatureList = creatureListC.Concat(creatureList5).ToArray();
        //Debug.Log(creatureList);
        foreach (GameObject creature in creatureList)
        {
            creature.tag = "OldGeneration"; //Tag it to be destroyed
            var count = creature.GetComponent<CreatureBehaviour>().points; //get the points count of each creature
            for (var j = 0; j < (count + 1) * 5; j++)
            {
                babiesList.Add(creature); //add the creature to the list j number of times, where j is the points*5
            }
        }
        //Now generate the new creatures
        for (var k = 0; k < creatureNum; k++)
        {
            int rand = Random.Range(0, babiesList.Count - 1); //generate random number
            GameObject babyToClone = babiesList[rand]; //get the creature corresponding to this index in the list
            CreateCreature(babyToClone); //Clone this creature (with small chance of mutation at each locus)
        }
        //Destroy previous creatures
        DestroyTag("OldGeneration");
    }

    //Function to create a new creature, either with random gene values, or clone of a parent, with some chance of mutation
    void CreateCreature(GameObject parent = null)
    {
        Vector3 position = new Vector3(Random.Range(-15f, 15f), Random.Range(minimumHeight, maximumHeight), Random.Range(-15f, 15f));
        GameObject newbaby = Instantiate(CreaturePrefab, position, Quaternion.identity);
        //min speed same for everyone
        newbaby.GetComponent<Genome>().minSpeed = minSpeed;
        //max speed
        float rand1 = Random.value;
        if (rand1 <= mutationRate || parent == null)
        {
            newbaby.GetComponent<Genome>().maxSpeed = Random.Range(minSpeed + 1, maxSpeed);
            Debug.Log("Mutation!");
        }
        else
        {
            newbaby.GetComponent<Genome>().maxSpeed = parent.GetComponent<Genome>().maxSpeed;
        }
        //rotation range
        float rand2 = Random.value;
        if (rand2 <= mutationRate || parent == null)
        {
            newbaby.GetComponent<Genome>().rotationRange = Random.Range(90f, maxRotationRange);
            Debug.Log("Mutation!");
        }
        else
        {
            newbaby.GetComponent<Genome>().rotationRange = parent.GetComponent<Genome>().rotationRange;
        }
        //grabber pref
        float rand3 = Random.value;
        if (rand3 <= mutationRate || parent == null)
        {
            newbaby.GetComponent<Genome>().GrabberPref = Random.value;
            Debug.Log("Mutation!");
        }
        else
        {
            newbaby.GetComponent<Genome>().GrabberPref = parent.GetComponent<Genome>().GrabberPref;
        }
        //leg functions
        int[] LegGenes = newbaby.GetComponent<Genome>().LegFunction;
        int[] ParentLegGenes;
        if (parent == null)
        {
            ParentLegGenes = LegGenes;
        }
        else
        {
            ParentLegGenes = parent.GetComponent<Genome>().LegFunction;
        }
        for (var i = 0; i < LegGenes.Length; ++i)
        {
            float rand4 = Random.value;
            if (rand4 <= mutationRate || parent == null)
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

    void WriteData(float data)
    {
        string path = "Assets/Data/Data.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(data.ToString());
        writer.Close();
        /*
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = Resources.Load("Data");
        */
    }
}



