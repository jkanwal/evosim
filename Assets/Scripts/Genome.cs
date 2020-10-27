using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Genome : MonoBehaviour
{
    //Limb function genes for Legs 1 through 6 (if 0, nothing; if 1, grabber; if 2, stinger)
    public int[] LegFunction = { 1, 0, 0, 0, 0, 0 };
    public List<int> genomeSequence = new List<int>();

    //Motion genes
    public float minSpeed = 10f;
    public float maxSpeed = 50f;
    public float rotationRange = 120f;

    //Behaviour genes
    public float GrabberPref = 0.5f; //prefers grabber over stinger if >= 0.5
    //public float GrabberBehaviour = 0.5f; //goes for other creatures over food with this probability
    //public float StingerBehaviour = 0.5f; //goes for other creatures over food with this probability

}
              