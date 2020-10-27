using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class LifeCycle : MonoBehaviour
{
    //Load in any prefabs
    public GameObject HostPrefab;

    //Set base birth and death rates
    public float birthRate = 0.01f;
    public float deathRate = 0.01f;
    public float myDeathRate; //my death rate could differ from the base rate if I'm infected

    //other variables
    //public float offset = 2.0f; //how far away to place any offspring I make
    //private Transform arena;
    //private float xzLim;
    //private float minimumHeight;
    //private float maximumHeight;

    void Start()
    {
        myDeathRate = deathRate; //unless this is changed later through infection

        /*
        RunSim runSim = GameObject.Find("RunSim").GetComponent<RunSim>();
        xzLim = runSim.xzLim;
        minimumHeight = runSim.minimumHeight;
        maximumHeight = runSim.maximumHeight;
        */

        //What arena am I in? Need this for assigning my offspring to the correct arena
        //arena = transform.parent;
    }

    void Update() 
    {
       //At any given time there is a chance of reproducing, i.e. creating a new host nearby
       float rand0 = Random.value; //generates a random number between 0 and 1
       if (rand0 <= birthRate)
       {
           Vector3 direction = Random.insideUnitCircle.normalized; //set a random direction
           Vector3 position = transform.position; //place the offspring some distance (equal to offset) away from my position, in this random direction (this is already causing a bug, by sometimes placing offspring outside the walls! Can you think of a solution?)
           //Vector3 position = Vector3.zero; // using this instead of the above 2 lines will make all the offspring explode out of the centre of the arena
           //Vector3 position = new Vector3(Random.Range(-xzLim, xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(-xzLim, xzLim)); //set random position within patch for offspring
           GameObject newHost = GameObject.Find("ReproductionHandler").GetComponent<ReproductionHandler>().Reproduce(position);
           //Debug.Log("newHost: " + newHost.GetComponent<Virus>().infected.ToString() + ", " + newHost.GetComponent<Virus>().virulence.ToString());
           int ID = GameObject.Find("RunSimVirus").GetComponent<RunSimVirus>().hostID;
           newHost.name = "host" + ID.ToString();
           GameObject.Find("RunSimVirus").GetComponent<RunSimVirus>().hostID += 1;
           //newHost.transform.parent = arena;
           //newHost.transform.localScale = new Vector3(1, 1, 1);
       }

       //At any given time there is also a chance of dying
       float rand1 = Random.value;
       if (rand1 <= myDeathRate)
       {
           Destroy(gameObject); // I'm deid!
       }
    }

}

