using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PixelMazeGen
{
    public GameObject arenaPrefab;
    private Texture2D maze;
    //public Camera camera;
    public int mazeSize = 50;
    private bool[,] visited;
    private bool[,] discovered;
    //private GameObject[,] tiles;
    private Dictionary<(int, int), List<(int, int)>> neighbourhood = new Dictionary<(int, int), List<(int, int)>>();
    Stack<(int, int)> myStack = new Stack<(int, int)>();
    private List<(int, int)> myList = new List<(int, int)>();
    private (int, int) currentTile;
    private float distributionFactor = 0f;
    private int stepper = 0;
    public int steps = 10;
    private bool park = false;
    private GameObject tile;
    //private Renderer renderer;
    private Material material;
    private AudioSource[] audioSource= new AudioSource[20];
    private AudioClip aClip;
    private bool hasAudio = false;
    private int dinger = 0;
    private int player = 0;

    public GameObject mazeSurface;

    public bool finished = false;


    // Start is called before the first frame update
    public PixelMazeGen()
    {
        
       
    }
    


    public void initPixels(int size)
    {
        mazeSize = size;
        //arenaPrefab = (GameObject)Resources.Load("OpenArena");
        maze = new Texture2D(mazeSize * 3, mazeSize * 3);

        var fillColorArray = maze.GetPixels();

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = new Vector4(0, 0, 0, 0); 
        }

        maze.SetPixels(fillColorArray);

        maze.Apply();
        //GetComponent<Renderer>();

        //gameObject.GetComponent<Renderer>().material.mainTexture = maze;


        //tiles = new GameObject[mazeSize, mazeSize];
        visited = new bool[mazeSize, mazeSize];

        discovered = new bool[mazeSize, mazeSize];
        neighbourhood = new Dictionary<(int, int), List<(int, int)>>();
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                //tiles[x, z].SetActive(false);
                //Destroy(tiles[x, z]);

                visited[x, z] = false;
                //tiles[x, z] = tile;
                neighbourhood.Add((x, z), new List<(int, int)>());
            }
        }


        //tile = Instantiate(arenaPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        //tile.SetActive(true);
        //starting tile
        //tile.transform.Find("Ground").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
        //tile.transform.Find("Ground").gameObject.GetComponent<Renderer>().material.mainTexture = maze;
        myList = new List<(int, int)>();
        myList.Add((mazeSize/2, mazeSize / 2));
        /*
        for (int i = 1; i < mazeSize-2; i++)
        {
                myList.Add((i, 0));
                myList.Add((0, i));
                myList.Add((mazeSize-1, i));
                myList.Add((i, mazeSize - 1));
            
        }*/
        currentTile = myList[0];
        discovered[0, 0] = true;
        maze.SetPixel((currentTile.Item1 * 3) + 1, currentTile.Item2 * 3 + 1, Color.grey);

        // maze.SetPixel(mazeSize / 2, mazeSize / 2, Color.grey);
    }

    public void initPixelsSides(int size)
    {
        mazeSize = size;
        //arenaPrefab = (GameObject)Resources.Load("OpenArena");
        maze = new Texture2D(mazeSize * 3, mazeSize * 3);

        var fillColorArray = maze.GetPixels();

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = new Vector4(0, 0, 0, 0); ;
        }

        maze.SetPixels(fillColorArray);

        maze.Apply();
        //GetComponent<Renderer>();

        //gameObject.GetComponent<Renderer>().material.mainTexture = maze;


        //tiles = new GameObject[mazeSize, mazeSize];
        visited = new bool[mazeSize, mazeSize];

        discovered = new bool[mazeSize, mazeSize];
        neighbourhood = new Dictionary<(int, int), List<(int, int)>>();
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                //tiles[x, z].SetActive(false);
                //Destroy(tiles[x, z]);

                visited[x, z] = false;
                //tiles[x, z] = tile;
                neighbourhood.Add((x, z), new List<(int, int)>());
            }
        }


        //tile = Instantiate(arenaPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        //tile.SetActive(true);
        //starting tile
        //tile.transform.Find("Ground").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
        //tile.transform.Find("Ground").gameObject.GetComponent<Renderer>().material.mainTexture = maze;
        myList = new List<(int, int)>();
        //myList.Add((mazeSize / 2, mazeSize / 2));
        
        for (int i = 1; i < mazeSize-2; i++)
        {
                myList.Add((i, 0));
                myList.Add((0, i));
                myList.Add((mazeSize-1, i));
                myList.Add((i, mazeSize - 1));
            
        }
        currentTile = myList[0];
        discovered[0, 0] = true;
    }

    public void addPixel()
    {
        visited[currentTile.Item1, currentTile.Item2] = true;
        //tiles[currentTile.Item1, currentTile.Item2].SetActive(true);
        var x = currentTile.Item1;
        var z = currentTile.Item2;
        List<(int, int)> neighbours = new List<(int, int)>();


        // look around current node + add neighbours
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
                //currentTile = myList[Random.Range(0,(myList.Count - 1)/3)];
                //currentTile = myList[myList.Count - 1]; //deapth first
                currentTile = myList[0]; //breath
            }else
            {
                finished = true;
            }
            /*
            if (hasAudio && dinger>0)
            {
                player = player % 20;
                audioSource[player].pitch = 0.5f/dinger;
                audioSource[player].PlayOneShot(aClip);
                player++;
                //audioSource.
            }
            dinger = 0;*/
        }
        else
        {
            dinger++;
            // random dir

            float r = Random.Range(0, 0f + distributionFactor % 3.1415f);

            var val = Math.Abs(0.999999f - Math.Sin(stepper * 0.02)) * neighbourhood[currentTile].Count;

            //var straight = 0;
            stepper++;
            val = Random.Range(0, neighbourhood[currentTile].Count);

            var nextDirection = (int)val;

            if (park)
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
                    //tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("West Wall").gameObject.SetActive(false);
                    //tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("East Wall").gameObject.SetActive(false);
                    maze.SetPixel((currentTile.Item1*3), currentTile.Item2*3+1, Color.white);
                    maze.SetPixel((tempTile.Item1 * 3)+2, tempTile.Item2*3+1, Color.white);


                }
                else if (currentTile.Item1 < tempTile.Item1)
                {
                    //tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("East Wall").gameObject.SetActive(false);
                    //tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("West Wall").gameObject.SetActive(false);
                    maze.SetPixel((currentTile.Item1 * 3)+2, currentTile.Item2 * 3 + 1, Color.white);
                    maze.SetPixel((tempTile.Item1 * 3), tempTile.Item2 * 3 + 1, Color.white);

                }
                else if (currentTile.Item2 > tempTile.Item2)
                {
                    //tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("South Wall").gameObject.SetActive(false);
                    //tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("North Wall").gameObject.SetActive(false);
                    maze.SetPixel((currentTile.Item1 * 3)+1, currentTile.Item2 * 3, Color.white);
                    maze.SetPixel((tempTile.Item1 * 3) +1, tempTile.Item2 * 3+2 , Color.white);
                }
                else
                {
                    //tiles[currentTile.Item1, currentTile.Item2].transform.Find("Walls").gameObject.transform.Find("North Wall").gameObject.SetActive(false);
                    //tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("South Wall").gameObject.SetActive(false);
                    maze.SetPixel((currentTile.Item1 * 3) + 1, currentTile.Item2 * 3+2 , Color.white);
                    maze.SetPixel((tempTile.Item1 * 3) + 1, tempTile.Item2 * 3, Color.white);
                }
                //yield return new WaitForSeconds(1.0f);
                maze.SetPixel((tempTile.Item1 * 3) + 1, tempTile.Item2 * 3 + 1, Color.white);
                
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

    public Texture2D PartialMaze(int nsteps)
    {
        if (!finished)
        {
            for (int x = 0; x < nsteps; x++)
            {
                addPixel();
            }
            maze.Apply();
        }
        

        return maze;
    }

    public void addCircleMask()
    {
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                if (Vector2.Distance(new Vector2(x,z),new Vector2(mazeSize / 2, mazeSize / 2)) >mazeSize/2){ 
                    visited[x, z] = true;
                 }
                
            }
        }

    }
    public void addStoCircleMask()
    {
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                if (Vector2.Distance(new Vector2(x, z), new Vector2(mazeSize / 2, mazeSize / 2)) > mazeSize / 80)
                {
                    
                    if (Random.value < 0.4)
                        {
                            visited[x, z] = true;
                        }
                }
                

            }
        }

    }
    public void addStoCircleMask2()
    {
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                if (Vector2.Distance(new Vector2(x, z), new Vector2(mazeSize / 2, mazeSize / 2)) > mazeSize / 90)
                {

                    if (Random.value < Vector2.Distance(new Vector2(x, z), new Vector2(mazeSize / 2, mazeSize / 2))/mazeSize)
                    {
                        visited[x, z] = true;
                    }
                }


            }
        }

    }

    public void noiseField(float density)
    {
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                if (Random.Range(0f,1f)< density)                   {
                        visited[x, z] = true;
                    
                } else
                {
                    //maze.SetPixel((x * 3) + 1, z * 3 + 1, Color.green);

                }



            }
        }

    }

    public void addAudio(AudioSource aSource, AudioClip clip)
    {
        hasAudio = true;
        for (int i = 0; i < 20; i++)
        {
            
            audioSource[i] = aSource;

        }
        //audioSource = aSource;
        aClip = clip;
        //audioSource.pitch = 3;
    }




}
