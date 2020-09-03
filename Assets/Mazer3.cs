using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Mazer3 : MonoBehaviour
{
    public GameObject arenaPrefab;
    public Camera camera;
    public int mazeSize;
    private bool[,] visited;
    private bool[,] discovered;
    private GameObject[,] tiles;
    private List<GameObject> spareTiles = new List<GameObject>();
    private int sparesCounter = 0;
    private Dictionary<(int, int), List<(int, int)>> neighbourhood = new Dictionary<(int, int), List<(int, int)>>();
    Stack<(int, int)> myStack = new Stack<(int, int)>();
    private List<(int, int)> myList = new List<(int, int)>();
    private (int, int) currentTile;
    private float distributionFactor = 0f;
    private int stepper = 0;
    private int steps = 10;
    private bool park = false;
    //private PixelMaze textureMazer;
    private Texture2D texture;
    private PixelMazeGen pixelGen;
    private PixelMazeGen subMazeGen;
    private PixelMazeGen groundGen;
    private List<PixelMazeGen> mazeGens = new List<PixelMazeGen>();
    private List<GameObject> mazes2D = new List<GameObject>();
    public AudioClip audioClip;
    private float speeder = 0;
    GameObject subMaze;

    private float noiseFactor = 0.40f;

    // Start is called before the first frame update
    void Start()
    {
        pixelGen = new PixelMazeGen();
        subMazeGen = new PixelMazeGen();
        groundGen = new PixelMazeGen();


        addFlatMaze(Vector3.zero);

        /*
        for (int i = 0; i < mazeSize *mazeSize*3; i++)
        {
            GameObject tile = Instantiate(arenaPrefab, new Vector3(0f, 0f,0f), Quaternion.identity);
            tile.transform.localScale *= 0.5f;
            tile.SetActive(false);
            spareTiles.Add(tile);
        }*/
        //camera.transform.position = new Vector3((mazeSize - 1) * 105f / 2, mazeSize * 30f * 5, (mazeSize - 1) * 105f / 2);
        camera.transform.position = new Vector3(0, 22500, 0);
        var rotation = Quaternion.Euler(90, 180, 0);
        camera.transform.rotation = rotation;
        //initMaze();

        //pixelGen.initPixels(20);
        //texture = pixelGen.PartialMaze(30);

               
        //Material material = tiles[0, 0].transform.Find("Ground").gameObject.GetComponent<Renderer>().material;
        //material.SetColor("_Color", Color.grey);
        //tiles[0, 0].SetActive(true);
        //tiles[0, 0].transform.Find("Ground").gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        /*
        GameObject room = (GameObject)Resources.Load("Arena");
        room = Instantiate(room, new Vector3(0f, 0f, 0f), Quaternion.identity);
        pixelGen.initPixels(100);
        texture = pixelGen.PartialMaze(2);
        room.transform.Find("Walls").gameObject.transform.Find("South Wall").gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        groundGen.initPixelsSides(100);
        room.transform.Find("Ground").gameObject.GetComponent<Renderer>().material.mainTexture = groundGen.PartialMaze(8000);

        */


        subMaze = Instantiate(arenaPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        subMaze.transform.localScale *= 300f;
        subMaze.SetActive(false);

        
        //subMazeGen.initPixels(100);
        //subMazeGen.addStoCircleMask2();
        //subMazeGen.noiseField(noiseFactor);
        //subMaze.GetComponent<Renderer>().material.mainTexture = subMazeGen.PartialMaze(200); 

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int x = 0; x < 1; x++)
        {
            //BuildMazeStep();
        }
       
        if (stepper % 30 == 0)
        {
            steps++;
        }
        stepper++;
        var rotation = Quaternion.Euler(0, .011f, 0);
        //camera.transform.rotation *= rotation;

        //speeder += 1.11f;
        camera.transform.position += new Vector3(0, 0.11f+speeder, 0);
        //pixelGen.PartialMaze(1);
        //subMazeGen.PartialMaze(5);        
        
        //groundGen.PartialMaze(5);

        Vector3 targetDirection = Vector3.zero - camera.transform.position;
        float singleStep = 0.1f * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(camera.transform.forward, targetDirection, singleStep, 0.0f);
        //camera.transform.rotation = Quaternion.LookRotation(newDirection);

        foreach (var mazeG in mazeGens)
        {
            // mazeG.mazeSurface.transform.LookAt(camera.transform.position);
            //if (camera.transform.position.y > mazeG.mazeSurface.transform.position.y && (camera.transform.position.y > mazeG.mazeSurface.transform.position.x+4500 || camera.transform.position.y - (mazeG.mazeSurface.transform.position.x + 4500)  >0))
            //{
            //    mazeG.PartialMaze(1);
            //}
            if (mazeG.mazeSurface.GetComponent<Renderer>().isVisible)
            {
                mazeG.PartialMaze(1);
                if (!mazeG.finished)
                {
                    mazeG.mazeSurface.transform.localScale *= 1.001f;
                }
                
            }



            /*
            if (camera.transform.position.y - mazeG.mazeSurface.transform.position.y > 2000)
            {
                //mazeG.mazeSurface.transform.position = new Vector3(Random.Range(0, 20000), camera.transform.position.y, Random.Range(0, 20000));
                addFlatMaze(new Vector3(Random.Range(0, camera.transform.position.y), Random.Range(0, camera.transform.position.y), Random.Range(0, camera.transform.position.y)));

            }
            */
        }

        if (Random.value < 0.04f)
        {
            addFlatMaze(new Vector3(Random.Range(-camera.transform.position.y, camera.transform.position.y), Random.Range(camera.transform.position.y/4, camera.transform.position.y/2), Random.Range(-camera.transform.position.y, camera.transform.position.y)));
        
        }
        


    }

    void Update()
    {
        if (Input.GetKeyDown("1"))
        {

            subMazeGen.initPixels(100);
            //subMazeGen.addStoCircleMask2();1            
            subMazeGen.noiseField(noiseFactor);
            subMaze.GetComponent<Renderer>().material.mainTexture = subMazeGen.PartialMaze(2);

        }

        if (Input.GetKeyDown("2"))
        {
            addFlatMaze(new Vector3(Random.Range(-camera.transform.position.y, camera.transform.position.y), Random.Range(camera.transform.position.y / 2, camera.transform.position.y), Random.Range(-camera.transform.position.y, camera.transform.position.y))).mazeSurface.transform.localScale *= camera.transform.position.y;
        }

        if (Input.GetKeyDown("3"))
        {
            speeder += 10;
        }
        if (Input.GetKeyDown("4"))
        {
            speeder -= 10;
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
                GameObject tile = Instantiate(arenaPrefab, new Vector3(x * 105f, 0f, z * 105f), Quaternion.identity);
                tile.SetActive(false);
                pixelGen.initPixels(x+z+1);
                tile.transform.Find("Ground").gameObject.GetComponent<Renderer>().material.mainTexture = pixelGen.PartialMaze(x*z+1); 
                visited[x, z] = false;
                tiles[x, z] = tile;
                neighbourhood.Add((x, z), new List<(int, int)>());
            }
        }
        //starting tile
        //tiles[0, 0].transform.Find("Ground").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        myList = new List<(int, int)>();
        myList.Add((mazeSize / 2, mazeSize / 2));
        currentTile = myList[0];
        discovered[0, 0] = true;
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
                currentTile = myList[Random.Range(0,myList.Count - 1)];
                //currentTile = myList[Random.Range(0, (myList.Count - 1)) / 2];
                //currentTile = myList[myList.Count - 1];
            }

        }
        else
        {
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

            var nextTile = neighbourhood[currentTile][nextDirection];
            //Debug.Log(tempTile);
            if (!visited[nextTile.Item1, nextTile.Item2])
            {
                myList.Add(nextTile);
                // Debug.Log(currentTile + " ->" + tempTile);

                // item1 is x coord item2 is z coord
                if (currentTile.Item1 > nextTile.Item1)
                {
                    spareTiles[sparesCounter].transform.position = new Vector3(tiles[currentTile.Item1, currentTile.Item2].transform.position.x- 35f, 0f, tiles[currentTile.Item1, currentTile.Item2].transform.position.z);
                    spareTiles[sparesCounter+1].transform.position = new Vector3(tiles[currentTile.Item1, currentTile.Item2].transform.position.x - 70f, 0f, tiles[currentTile.Item1, currentTile.Item2].transform.position.z);
                    //tiles[currentTile.Item1, currentTile.Item2].transform.position.x;


                    //tiles[tempTile.Item1, tempTile.Item2].transform.Find("Walls").gameObject.transform.Find("East Wall").gameObject.SetActive(false);

                }
                else if (currentTile.Item1 < nextTile.Item1)
                {
                    spareTiles[sparesCounter].transform.position = new Vector3(tiles[currentTile.Item1, currentTile.Item2].transform.position.x + 35f, 0f, tiles[currentTile.Item1, currentTile.Item2].transform.position.z);
                    spareTiles[sparesCounter+1].transform.position = new Vector3(tiles[currentTile.Item1, currentTile.Item2].transform.position.x + 70f, 0f, tiles[currentTile.Item1, currentTile.Item2].transform.position.z);
                }
                else if (currentTile.Item2 > nextTile.Item2)
                {
                    spareTiles[sparesCounter].transform.position = new Vector3(tiles[currentTile.Item1, currentTile.Item2].transform.position.x , 0f, tiles[currentTile.Item1, currentTile.Item2].transform.position.z-35);
                    spareTiles[sparesCounter+1].transform.position = new Vector3(tiles[currentTile.Item1, currentTile.Item2].transform.position.x , 0f, tiles[currentTile.Item1, currentTile.Item2].transform.position.z-70);
                }
                else
                {
                    spareTiles[sparesCounter].transform.position = new Vector3(tiles[currentTile.Item1, currentTile.Item2].transform.position.x , 0f, tiles[currentTile.Item1, currentTile.Item2].transform.position.z + 35);
                    spareTiles[sparesCounter+1].transform.position = new Vector3(tiles[currentTile.Item1, currentTile.Item2].transform.position.x , 0f, tiles[currentTile.Item1, currentTile.Item2].transform.position.z + 70);
                }
                //yield return new WaitForSeconds(1.0f);

                spareTiles[sparesCounter].SetActive(true);
                spareTiles[sparesCounter + 1].SetActive(true);
                sparesCounter += 2;
                currentTile = nextTile;
                //Debug.Log(neighb);
            }
            else
            {
                neighbourhood[currentTile].Remove((nextTile.Item1, nextTile.Item2));
                //currentTile = myList[0];
            }
        }
    }

    public void cameraMove()
    {

    }

    public void addFlatMaze()
    {
        
            var mazeGen = new PixelMazeGen();
            mazeGen.initPixels(Random.Range(80,120));
            mazeGen.noiseField(noiseFactor);

            mazeGen.mazeSurface = Instantiate(arenaPrefab, new Vector3(Random.Range(0, 32000), Random.Range(0, 20000), Random.Range(0, 18000)), Quaternion.Euler(new Vector3(90, 0, 0)));
            //  for mazes always facing camera  
            //mazeGen.mazeSurface = Instantiate(arenaPrefab, new Vector3(Random.Range(0, 20000), Random.Range(0, 20000), Random.Range(0, 20000)), Quaternion.Euler(new Vector3(180, 0, 0)));
            mazeGen.mazeSurface.transform.localScale *= 350.0f;
            mazeGen.mazeSurface.SetActive(true);

            mazeGen.mazeSurface.GetComponent<Renderer>().material.mainTexture = mazeGen.PartialMaze(1);
            
            mazeGens.Add(mazeGen);
    }

    public PixelMazeGen addFlatMaze(Vector3 location)
    {

        var mazeGen = new PixelMazeGen();
        mazeGen.initPixels(Random.Range(80, 120));
        mazeGen.noiseField(noiseFactor);

        mazeGen.mazeSurface = Instantiate(arenaPrefab, location, Quaternion.Euler(new Vector3(90,0, 0)));
        //  for mazes always facing camera  
        //mazeGen.mazeSurface = Instantiate(arenaPrefab, new Vector3(Random.Range(0, 20000), Random.Range(0, 20000), Random.Range(0, 20000)), Quaternion.Euler(new Vector3(180, 0, 0)));
        mazeGen.mazeSurface.transform.localScale *= 350.0f;
        mazeGen.mazeSurface.SetActive(true);

        mazeGen.mazeSurface.GetComponent<Renderer>().material.mainTexture = mazeGen.PartialMaze(1);

        mazeGens.Add(mazeGen);

        return mazeGen;
    }

}
