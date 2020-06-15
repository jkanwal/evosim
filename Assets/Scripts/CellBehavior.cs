using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CellBehavior : MonoBehaviour
{

    public GameObject cellPrefab;

    private int cellRole = 0;  //0 core 1 leg 2 tailend 
    private int tailNum;
    private int tails = 0;
    private int energy = 1;
    private int phase; // 0 Feed 1 expand 2 multiply 3 stay
    private float baseSize = 1f;
    private int cellNum;
    private Dictionary<(int, int), GameObject> subStructure = new Dictionary<(int, int), GameObject>();  // source target cell
    private Dictionary<int, GameObject> cells = new Dictionary<int, GameObject>();  // source target cell
    private int headBranch = 6;
    private int branchesDepth = 10;

    public int CellRole { get => cellRole; set => cellRole = value; }

    // Start is called before the first frame update
    void Start()
    {
        cellPrefab = Resources.Load("Cell") as GameObject;
        cells.Add(0, this.gameObject);


    }

    // Update is called once per frame
    void Update()
    {
        switch (cellRole)
        {
            case 0:
                switch (phase)
                {
                    case 0:
                        feed();
                        if (energy > 60)
                        {
                            phase = 1;
                        }
                        break;
                    case 1:
                        expand();
                        //linearexpand();
                        
                        if(tails < 6)
                        {
                            phase = 0;
                        }
                        else
                        {
                            phase = 2;
                        }
                        //phase = 2;
                        break;
                        
                    case 2:
                        //multiply
                        break;
                    case 3:
                        //stay
                        break;
                    default:
                        Debug.Log("phase error");
                        break;

                }
                break;
            default:
                break;

        }

    }

    public void feed()
    {
        energy++;
    }

    public void Multiply()
    {

    }

    public void expand()
    {
        if (CountBranches(0) < headBranch)
        {
            var c = Instantiate(cellPrefab) as GameObject;
            c.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
            c.transform.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f));
            c.transform.localScale = new Vector3(baseSize, baseSize, baseSize);
            c.GetComponent<CellBehavior>().CellRole = 1; // set leg

            SpringJoint spring = c.AddComponent<SpringJoint>();
            //spring.autoConfigureConnectedAnchor = false;
            spring.connectedBody = gameObject.GetComponent<Rigidbody>();
            var dist = 0.5f;
            spring.minDistance = dist;
            spring.maxDistance = dist;
            spring.spring = 380f;
            spring.damper = 0.8f;
            //add co spring
            energy = 1;
            var num = cells.Count;
            subStructure.Add((num, 0), c);
            cells.Add(num, c);
        }
        else
        {
            for (var i = 1; i <= headBranch; i++)
            {
                if (CountDepth(i) < branchesDepth)
                {
                    var leg = Instantiate(cellPrefab) as GameObject;
                    leg.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
                    leg.transform.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f));
                    leg.transform.localScale = new Vector3(baseSize, baseSize, baseSize);
                    leg.GetComponent<CellBehavior>().CellRole = 1; // set leg
                    leg.GetComponent<Renderer>().material.SetColor("_Color", Color.green);

                    SpringJoint spring = leg.AddComponent<SpringJoint>();
                    //spring.autoConfigureConnectedAnchor = false;
                    var connection = Deepest(i);
                    spring.connectedBody = cells[connection].gameObject.GetComponent<Rigidbody>();
                    var dist = 0.5f;
                    spring.minDistance = dist;
                    spring.maxDistance = dist;
                    spring.spring = 380f;
                    spring.damper = 1.8f;
                    var num = cells.Count;
                    subStructure.Add((num, connection), leg);
                    cells.Add(num, leg);
                    energy = 1;
                    break;
                } else
                {
                    var tail = Instantiate(cellPrefab) as GameObject;
                    tail.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
                    tail.transform.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f));
                    tail.transform.localScale = new Vector3(baseSize, baseSize, baseSize);
                    tail.GetComponent<CellBehavior>().CellRole = 1; // set leg
                    tail.GetComponent<Renderer>().material.SetColor("_Color", Color.red);

                    SpringJoint TSpring = tail.AddComponent<SpringJoint>();
                    //spring.autoConfigureConnectedAnchor = false;
                    var connection = Deepest(i);
                    TSpring.connectedBody = gameObject.GetComponent<Rigidbody>();
                    var dist = 10.5f;
                    TSpring.minDistance = dist;
                    TSpring.maxDistance = dist;
                    TSpring.spring = 380f;
                    TSpring.damper = 1.8f;


                    var num = cells.Count;
                    subStructure.Add((num, connection), tail);
                    cells.Add(num, tail);
                    energy = 1;
                    tails++;

                }
            }
        }
    }



    public void linearexpand()
    {
        var c = Instantiate(cellPrefab) as GameObject;
        c.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        c.transform.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f));
        c.transform.localScale = new Vector3(baseSize, baseSize, baseSize);

        SpringJoint spring = c.AddComponent<SpringJoint>();
        //spring.autoConfigureConnectedAnchor = false;
        spring.connectedBody = gameObject.GetComponent<Rigidbody>();
        var dist = 0.5f;
        spring.minDistance = dist;
        spring.maxDistance = dist;
        spring.spring = 380f;
        spring.damper = 0.8f;
        //add co spring
        energy = 1;
    }

    public int CountBranches(int cellNum)
    {
        int branches = 0;
        var keys = subStructure.Keys.ToArray();
        foreach (var key in keys)
        {
            if (key.Item2 == cellNum)
            {
                branches++;
            }
        }

        return branches;
    }

    public int CountDepth(int cellNum)
    {
        int depth = 0;
        var keys = subStructure.Keys.ToArray();
        foreach (var key in keys)
        {
            if (key.Item2 == cellNum)
            {
                depth++;
                depth += CountDepth(key.Item1);
                break;
            }
        }
        Debug.Log("depth: " + depth);
        return depth;
    }

    public int Deepest(int cellNum)
    {
        int deepest = cellNum;
        var keys = subStructure.Keys.ToArray();
        foreach (var key in keys)
        {
            if (key.Item2 == cellNum)
            {
                deepest = key.Item1;
                deepest = Deepest(key.Item1);
                break;
            }
        }
        return deepest;

    }




}
