using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Meandrome : MonoBehaviour
{

    public GameObject arenaPrefab;
    public int mazeSize;
    private bool[,] visited;
    private bool[,] discovered;
    private GameObject[,] tiles;
    private Dictionary<(int, int), List<(int, int)>> neighbourhood = new Dictionary<(int, int), List<(int, int)>>();
    Stack<(int, int)> myStack = new Stack<(int, int)>();
    private List<(int, int)> myList = new List<(int, int)>();
    private (int,int) currentTile;
    private float distributionFactor = 0f;
    private int stepper = 7;
    private Vector3 currentPosition = new Vector3(0, 0, 0);
    private Vector3 direction = new Vector3(1, 0, 0);
    private int inversor = -1;

    // Start is called before the first frame update
    void Start()
    {
        
        //myStack.Push((0,0));

       // initMaze();
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
            //initMaze();
        }

        // steps forwards
        for (int i = 0; i <stepper; i++)
        {
            GameObject tile = Instantiate(arenaPrefab, new Vector3(currentPosition.x * 30f, 0f, currentPosition.z * 30f), Quaternion.identity);
            currentPosition = currentPosition + direction;

        }
        direction = Quaternion.Euler(0, 90 * inversor, 0) * direction;
        stepper += inversor;
        
        //invert stepper
        if(stepper <=2)
        {
            inversor *= -1;
        } else if (stepper >= 7)
        {
            inversor *= -1;
        }


    }

    private void BuildMazeStep()
    {


        GameObject tile = Instantiate(arenaPrefab, new Vector3(currentPosition.x * 30f, 0f, currentPosition.z * 30f), Quaternion.identity);


    }
}
