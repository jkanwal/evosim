using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoArena : Ground
{
    List<GameObject> boxes = new List<GameObject>();
    public GameObject GoodiePrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject g in boxes)
        {
            g.transform.Translate(0,Mathf.Sin(Time.time)*0.01f,0);
        }
    }

    public override GameObject spawnGoodies()
    {
        
        GameObject goodie = Instantiate(GoodiePrefab, new Vector3(transform.position.x, 
            transform.position.y + 3f,transform.position.z), Quaternion.identity);
        boxes.Add(goodie);
        return goodie;
        
    }
}
