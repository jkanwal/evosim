using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TestRun : MonoBehaviour
{
    //Inputs
    public GameObject CreaturePrefab;
    public GameObject FoodPrefab;
    //public Genome genome;
    //public Behaviour behaviour;

    //Simulation parameters
    public int foodAmount = 20;
    public float foodProb = 0.001f;
    public float minimumHeight = 2f;
    public float maximumHeight = 7f;
    public int creatureNum = 10;
    //public float resetRate = 30f;
    //private float resetTime;

    // Start is called before the first frame update
    void Start()
    {
        //Spawn food in random locations
        SpawnFood(foodAmount);

        //Spawn some random creatures
        for (var i = 0; i < creatureNum; ++i)
        {
            Vector3 position = new Vector3(Random.Range(-15f, 15f), 2f, Random.Range(-15f, 15f));
            GameObject baby = Instantiate(CreaturePrefab, position, Quaternion.identity);
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
}
