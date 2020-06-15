using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour
{

    public GameObject arenaPrefab;
    public Camera camera;
    public int mazeSize;
    private bool[,] visited;
    private bool[,] discovered;
    private GameObject[,] tiles;
    private Dictionary<(int, int), List<(int, int)>> neighbourhood = new Dictionary<(int, int), List<(int, int)>>();
    Stack<(int, int)> myStack = new Stack<(int, int)>();
    private List<(int, int)> myList = new List<(int, int)>();
    private (int,int) currentTile;
    private float distributionFactor = 0f;
    private int stepper = 0;
    private int steps = 10;
    private bool park = false;

    // Start is called before the first frame update
    void Start()
    {
        
        //myStack.Push((0,0));

        initMaze();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ThingList = GameObject.FindGameObjectsWithTag("Arena");
            foreach (GameObject thing in ThingList)
            {
                Destroy(thing);
            }
            distributionFactor += 0.31415f;
            initMaze();
        }
        

        if (Input.GetKeyDown("1"))
        {
            var ThingList = GameObject.FindGameObjectsWithTag("Arena");
            foreach (GameObject thing in ThingList)
            {
                Destroy(thing);
            }
            mazeSize = 1;
            steps = 1;
            camera.transform.position = new Vector3((mazeSize-1) * 35f / 2, mazeSize * 30f * 5, (mazeSize - 1) * 35f / 2);
            initMaze();
        }

        if (Input.GetKeyDown("2"))
        {
            var ThingList = GameObject.FindGameObjectsWithTag("Arena");
            foreach (GameObject thing in ThingList)
            {
                Destroy(thing);
            }
            mazeSize = 2;
            steps = 1;
            camera.transform.position = new Vector3((mazeSize - 1) * 35f / 2, mazeSize * 30f * 1.6f, (mazeSize - 1) * 35f / 2);
            initMaze();
        }

        if (Input.GetKeyDown("3"))
        {
            var ThingList = GameObject.FindGameObjectsWithTag("Arena");
            foreach (GameObject thing in ThingList)
            {
                Destroy(thing);
            }
            mazeSize *= 2;
            if (mazeSize > 22)
            {
                steps *= 4;
            }
            
            camera.transform.position = new Vector3((mazeSize - 1) * 35f / 2, (mazeSize - 1) * 35f * 1.2f+40f, (mazeSize - 1) * 35f / 2);
            park = false;
            initMaze();
        }


        if (Input.GetKeyDown("space"))
        {
            var ThingList = GameObject.FindGameObjectsWithTag("Arena");
            foreach (GameObject thing in ThingList)
            {
                Destroy(thing);
            }
            mazeSize = 20;
            steps = 8;
            camera.transform.position = new Vector3(mazeSize * 35f / 2, mazeSize * 35f * 3, mazeSize * 35f / 2);
            park = true;

            initMaze();
        }

        if (Input.GetKeyDown("0"))
        {
            var ThingList = GameObject.FindGameObjectsWithTag("Arena");
            foreach (GameObject thing in ThingList)
            {
                Destroy(thing);
            }
            mazeSize = 1;
            steps = 1;
            camera.transform.position = new Vector3(mazeSize * 30f / 2, 10000f, mazeSize * 30f / 2);
            park = true;

            initMaze();
        }

        for (int i = 0; i < steps; i++)
        {
            BuildMazeStep();
        }

    }

    private void BuildMazeStep()
    {
        visited[currentTile.Item1, currentTile.Item2] = true;
        tiles[currentTile.Item1, currentTile.Item2].SetActive(true);
        var x = currentTile.Item1;
        var z = currentTile.Item2;
        List<(int, int)> neighbours = new List<(int, int)>();

        if (x > 0 && !visited[x - 1, z] && !discovered[x - 1, z]) { neighbours.Add((x - 1, z)); discovered[x - 1, z] = true; } // west
        if (z > 0 && !visited[x, z - 1] && !discovered[x, z - 1]) { neighbours.Add((x, z - 1)); discovered[x, z - 1] = true; } //south
        if (x < mazeSize - 1 && !visited[x + 1, z] && !discovered[x + 1, z]) { neighbours.Add((x + 1, z)); discovered[x + 1, z] = true; } // est
        if (z < mazeSize - 1 && !visited[x, z + 1] && !discovered[x, z + 1]) { neighbours.Add((x, z + 1)); discovered[x, z + 1] = true; } //north

        if (neighbours.Count > 0) { neighbourhood[(x, z)].AddRange(neighbours); }

        //check if dead end
        if (neighbourhood[currentTile].Count == 0)
        {
            //Debug.Log(currentTile + " dead end");
            myList.Remove(currentTile);
            if (myList.Count > 0)
            {
                //currentTile = myList[Random.Range(0,myList.Count - 1)];
                currentTile = myList[myList.Count - 1];
            }
            
        }
        else
        {
            // random dir
            
            float r = Random.Range(0, 0f + distributionFactor%3.1415f);

            var val = Math.Abs(0.999999f - Math.Sin(stepper*0.02)) * neighbourhood[currentTile].Count;

            var straight = 0;
            stepper++;
            val = Random.Range(0, neighbourhood[currentTile].Count);

            var nextDirection = (int)val;

            if(park)
            {
                nextDirection = 0;
            }

            //Debug.Log(nextDirection);
            nextDirection = Mathf.Clamp(nextDirection, 0, neighbourhood[currentTile].Count - 1);

           // nextDirection = (int)(Math.Abs(Math.Sin(stepper)))%neighbourhood[currentTile].Count;
            
            //Debug.Log(Math.Sin(stepper));
            //Debug.Log(nextDirection);

            var tempTile = neighbourhood[currentTile][nextDirection];
            //Debug.Log(tempTile);
            if (!visited[tempTile.Item1, tempTile.Item2])
            {
                myList.Add(tempTile);
                // Debug.Log(currentTile + " ->" + tempTile);


                if (currentTile.Item1 > tempTile.Item1)
                {
                    tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("West Wall").gameObject.SetActive(false);
                    tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("East Wall").gameObject.SetActive(false);

                }
                else if (currentTile.Item1 < tempTile.Item1)
                {
                    tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("East Wall").gameObject.SetActive(false);
                    tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("West Wall").gameObject.SetActive(false);
                }
                else if (currentTile.Item2 > tempTile.Item2)
                {
                    tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("South Wall").gameObject.SetActive(false);
                    tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("North Wall").gameObject.SetActive(false);

                }
                else
                {
                    tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("North Wall").gameObject.SetActive(false);
                    tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("South Wall").gameObject.SetActive(false);

                }
                //yield return new WaitForSeconds(1.0f);
                currentTile = tempTile;
                //Debug.Log(neighb);
            }
            else
            {
                neighbourhood[currentTile].Remove((tempTile.Item1, tempTile.Item2));
                //currentTile = myList[0];
            }
        }
    }

    public void initMaze()
    {
        visited = new bool[mazeSize, mazeSize];
        tiles = new GameObject[mazeSize, mazeSize];
        discovered = new bool[mazeSize, mazeSize];
        neighbourhood = new Dictionary<(int, int), List<(int, int)>>();
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                //tiles[x, z].SetActive(false);
                //Destroy(tiles[x, z]);


                GameObject tile = Instantiate(arenaPrefab, new Vector3(x * 35f, 0f, z * 35f), Quaternion.identity);
                tile.SetActive(false);
                visited[x, z] = false;
                tiles[x, z] = tile;
                neighbourhood.Add((x, z), new List<(int, int)>());
            }
        }

        //starting tile
        tiles[0, 0].transform.Find("Ground").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        myList = new List<(int, int)>();
        myList.Add((0, 0));
        currentTile = myList[0];
        discovered[0, 0] = true;
    }

    public void buildMaze()
    {

        visited = new bool[mazeSize, mazeSize];
        tiles = new GameObject[mazeSize, mazeSize];
        discovered = new bool[mazeSize, mazeSize];
        neighbourhood = new Dictionary<(int, int), List<(int, int)>>();
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                //tiles[x, z].SetActive(false);
                //Destroy(tiles[x, z]);
                

                GameObject tile = Instantiate(arenaPrefab, new Vector3(x * 30f, 0f, z * 30f), Quaternion.identity);
                visited[x, z] = false;
                tiles[x, z] = tile;
                neighbourhood.Add((x, z), new List<(int, int)>());
            }
        }

        //starting tile
        tiles[0, 0].transform.Find("Ground").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        myList.Add((0, 0));
        currentTile = myList[0];
        discovered[0, 0] = true;

        while (myList.Count > 0)
        {
            visited[currentTile.Item1, currentTile.Item2] = true;
            var x = currentTile.Item1;
            var z = currentTile.Item2;
            List<(int, int)> neighbours = new List<(int, int)>();

            if (x > 0 && !visited[x - 1, z] && !discovered[x - 1, z]) { neighbours.Add((x - 1, z)); discovered[x - 1, z] = true; } // west
            if (z > 0 && !visited[x, z - 1] && !discovered[x, z - 1]) { neighbours.Add((x, z - 1)); discovered[x, z - 1] = true; } //south
            if (x < mazeSize - 1 && !visited[x + 1, z] && !discovered[x + 1, z]) { neighbours.Add((x + 1, z)); discovered[x + 1, z] = true; } // est
            if (z < mazeSize - 1 && !visited[x, z + 1] && !discovered[x, z + 1]) { neighbours.Add((x, z + 1)); discovered[x, z + 1] = true; } //north

            if (neighbours.Count > 0) { neighbourhood[(x, z)].AddRange(neighbours); }

            //check if dead end
            if (neighbourhood[currentTile].Count == 0)
            {
                //Debug.Log(currentTile + " dead end");
                myList.Remove(currentTile);
                if (myList.Count > 0)
                {
                    currentTile = myList[myList.Count-1];
                }
                else
                {
                    break;
                }
            }
            else
            {
                // random dir
                //int nextDirection = Random.Range(0, neighbourhood[currentTile].Count);
                float r = Random.Range(0, 0.01f + distributionFactor);

                var val = Math.Abs(0.9999f-Math.Sin(r))* neighbourhood[currentTile].Count;
                var nextDirection = (int)val;
               // Debug.Log(nextDirection);
                nextDirection = Mathf.Clamp(nextDirection,0, neighbourhood[currentTile].Count-1);

                var tempTile = neighbourhood[currentTile][nextDirection];
                //Debug.Log(tempTile);
                if (!visited[tempTile.Item1, tempTile.Item2])
                {
                    myList.Add(tempTile);
                   // Debug.Log(currentTile + " ->" + tempTile);


                    if (currentTile.Item1 > tempTile.Item1)
                    {
                        tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("West Wall").gameObject.SetActive(false);
                        tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("East Wall").gameObject.SetActive(false);

                    }
                    else if (currentTile.Item1 < tempTile.Item1)
                    {
                        tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("East Wall").gameObject.SetActive(false);
                        tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("West Wall").gameObject.SetActive(false);
                    }
                    else if (currentTile.Item2 > tempTile.Item2)
                    {
                        tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("South Wall").gameObject.SetActive(false);
                        tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("North Wall").gameObject.SetActive(false);

                    }
                    else
                    {
                        tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("North Wall").gameObject.SetActive(false);
                        tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("South Wall").gameObject.SetActive(false);

                    }
                    //yield return new WaitForSeconds(1.0f);
                    currentTile = tempTile;
                    //Debug.Log(neighb);
                }
                else
                {
                    neighbourhood[currentTile].Remove((tempTile.Item1, tempTile.Item2));
                    //currentTile = myList[0];
                }
            }


        }
        //yield return null;
    }


}
