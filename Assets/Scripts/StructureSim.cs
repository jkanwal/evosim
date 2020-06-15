using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureSim : MonoBehaviour
{
    public GameObject cellPrefab;
    public List<GameObject> cells = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        var c = Instantiate(cellPrefab) as GameObject;
        c.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        //c.transform.localScale = new Vector3(baseSize, baseSize, baseSize);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
