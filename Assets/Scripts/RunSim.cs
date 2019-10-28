using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RunSim : MonoBehaviour
{
    //inputs
    public GameObject CreaturePrefab;
    public GameObject FoodPrefab;
    public Genome genome;
    public Behaviour behaviour;

    //simulation parameters
    public int foodAmount = 20;
    public float foodProb = 0.001f;
    public float minimumHeight = 2f;
    public float maximumHeight = 7f;
    public int creatureNum = 4;
    public float minimumSpeed = 0f;
    public float maximumSpeed = 35f;
    public float minumumRotation = 80f;
    public float resetRate = 30f;
    private float resetTime;
    //public float mutationRate = 0.1;

    // Start is called before the first frame update
    void Start()
    {
        resetTime = resetRate;

        //Spawn food in random locations
        SpawnFood(foodAmount);

        //Spawn some random creatures
        for (var i = 0; i < creatureNum; ++i)
        {
            Vector3 position = new Vector3(Random.Range(-15f, 15f), 0.5f, Random.Range(-15f, 15f));
            GameObject creature = new GameObject("creature");
            creature.AddComponent<Genome>();
            creature.AddComponent<Behaviour>();
            

        
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

        //Every several seconds, we respawn a set of new creatures based on who was most successful
        if (Time.time > resetTime)
        {
            resetTime = Time.time + resetRate;

            //Count creature success & Create new creatures
            Reproduction();

            //Also respawn the food
            DestroyTag("Pick Up");
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
        GameObject[] creatureList3 = GameObject.FindGameObjectsWithTag("Targeting");
        GameObject[] creatureList0 = creatureList1.Concat(creatureList2).ToArray();
        GameObject[] creatureList = creatureList0.Concat(creatureList3).ToArray();
        //Debug.Log(creatureList);
        foreach (GameObject creature in creatureList)
        {
            creature.tag = "OldGeneration"; //Tag it to be destroyed
            var count = creature.GetComponent<GrabFood>().count; //get the food count of each creature
            for (var j = 0; j < (count + 1) * 5; j++)
            {
                babiesList.Add(creature); //add the creature to the list j number of times, where j is the food count
            }
        }
        //Now generate the new creatures
        for (var k = 0; k < creatureNum; k++)
        {
            int rand2 = Random.Range(0, babiesList.Count - 1); //generate random number
            GameObject babyToClone = babiesList[rand2]; //get the creature corresponding to this index in the list
            float speed = babyToClone.GetComponent<AntennaSteering>().maxSpeed;
            float rotRange = babyToClone.GetComponent<AntennaSteering>().rotationRange;
            Vector3 position = new Vector3(Random.Range(-15f, 15f), 0.5f, Random.Range(-15f, 15f));
            GameObject newbaby = Instantiate(CreaturePrefab, position, Quaternion.identity);
            newbaby.GetComponent<AntennaSteering>().maxSpeed = speed;
            newbaby.GetComponent<AntennaSteering>().rotationRange = rotRange;
            //Instantiate(babyToClone, position, Quaternion.identity); //clone this creature at a random location
            //Chance of a mutation??

        }
        //Destroy previous creatures
        DestroyTag("OldGeneration");
    }
}
