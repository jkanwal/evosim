using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class RunSimVirus : MonoBehaviour
{
    //Inputs
    public GameObject ArenaPrefab;
    public GameObject HostPrefab;
    public string writepath = "Assets/Data/data.csv"; //write simulation data to this filename (if the file exists already, it will continue to add lines to it. If it doesn't it will first create a file with this name)

    //General simulation parameters (which can be altered before a run)
    public int patchNum = 20; //number patches per generation
    public int hostNum = 20; //number of hosts per patch
    public float initialInfections = 0.1f; //initial percentage of population infected
    public float baseVirulence = 0.1f; //what is the base (initial) virulence of the virus?
    public int simTime = 1000000; //total number of Ticks after which simulation stops
    public float arenaSize = 30f; // length of side of square arena
    public float minimumHeight = 2f; //min height of host placement in patch
    public float maximumHeight = 7f; //max height of host placement in patch
    public int hostID; //this number gives a unique ID to each host
 
    //Other global variables
    private int Ticks;
    private int WriteCount;
    private float x0;
    private float z0;
    public float xzLim;
    private List<GameObject> hostList = new List<GameObject>(); //create empty list to keep track of hosts


    // Start is called before the first frame update
    void Start()
    {
        hostID = 0;

        xzLim = (arenaSize / 2) - 2; //max distance from centre of arena at which objects can be placed (this is to make sure nothing is placed too close to the edges, which can cause bugs)

        /*
        //write file header
        StreamWriter writer = new StreamWriter(writepath, true);
        writer.WriteLine("Ticks, Host, Infected, Virulence");
        writer.Close();
        */

        //Spawn patches (arenas)
        x0 = 0f;
        z0 = 0f;
        for (var i = 0; i < patchNum; i++)
        {
            Vector3 position = new Vector3(x0, 0, z0);
            GameObject arena = Instantiate(ArenaPrefab, position, Quaternion.identity);
            //scale arena correctly depending on arenasize
            float scale = Mathf.RoundToInt(arenaSize/30f);
            arena.transform.localScale = new Vector3(scale, 1, scale);
            //assign arena id

            //Spawn initial hosts in this patch:
            int infectedNum = Mathf.RoundToInt(initialInfections*hostNum); //get number of infected hosts (round to nearest integer if it's a fraction)
            int uninfectedNum = hostNum - infectedNum; 
            //Spawn the uninfected hosts
            for (var j = 0; j < uninfectedNum; j++)
            {
                Vector3 newPosition = new Vector3(Random.Range(x0 - xzLim, x0 + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z0 - xzLim, z0 + xzLim)); //set random position within patch
                GameObject newHost = Instantiate(HostPrefab, newPosition, Quaternion.identity); //spawn an uninfected host
                newHost.name = "host" + hostID.ToString();
                hostID += 1;
                //newHost.transform.parent = arena.transform.Find("GameObject");
                //newHost.transform.localScale = new Vector3(1, 1, 1); //rescale this so it doesn't inherit weird scaling of arena
            }
            //Spawn the infected hosts
            for (var k = 0; k < infectedNum; k++)
            {
                Vector3 newPosition = new Vector3(Random.Range(x0 - xzLim, x0 + xzLim), Random.Range(minimumHeight, maximumHeight), Random.Range(z0 - xzLim, z0 + xzLim)); //set random position within patch
                GameObject newHost = Instantiate(HostPrefab, newPosition, Quaternion.identity);
                newHost.name = "host" + hostID.ToString();
                hostID += 1;
                //newHost.transform.parent = arena.transform.Find("GameObject");
                //newHost.transform.localScale = new Vector3(1, 1, 1);
                newHost.GetComponent<Virus>().infected = true; //set it to infected 
                newHost.GetComponent<Virus>().virulence = baseVirulence; //we could also try setting each initial infection to a different random virulence value...
                newHost.GetComponent<LifeCycle>().myDeathRate = newHost.GetComponent<LifeCycle>().deathRate + baseVirulence; 
                newHost.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.5f + baseVirulence*50f, 0.5f - baseVirulence*50f, 0f);
            }

            //advance to next patch location
            if (i % 5 == 4)
            {
                z0 = -(xzLim * 3) * (i+1)/5;
                x0 = 0f;
            }
            else
            {
                x0 += xzLim * 3;
            } 
        }

        Ticks = 0;

        WriteCount = 0;
    }

    // Fixed Update is called at a set interval, and deals with the tick advances
    void FixedUpdate()
    {
        Ticks += 1; //count up a Tick at each physics update

        /*
        //at certain Tick intervals, we need to write some data about the current infection rates and virulence in the population
        if (Ticks >= 1000)
        {
            WriteData();
            Ticks = 0;
            WriteCount += 1;

        }
        */
    }

    //Function to write data
    void WriteData()
    {
        StreamWriter writer = new StreamWriter(writepath, true);
        hostList = GameObject.FindGameObjectsWithTag("Host").ToList<GameObject>();
        foreach (GameObject host in hostList)
        {
            bool i = host.GetComponent<Virus>().infected;
            float v = host.GetComponent<Virus>().virulence;
            string hostName = host.name;
            writer.WriteLine(WriteCount.ToString() + "," + hostName + ", " + i.ToString() + ", " + v.ToString());
        }
        writer.Close();
    }

}



