using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class ReproductionHandler : MonoBehaviour
{
    //Load in any prefabs
    public GameObject HostPrefab;

    public GameObject Reproduce(Vector3 position) 
    {
        GameObject newHost = Instantiate(HostPrefab, position, Quaternion.identity); //create the new host at the given position
        return newHost;
    }

}