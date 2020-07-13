using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Genome : MonoBehaviour
{
    //Limb function genes for Legs 1 through 6 (if 0, nothing; if 1, grabber; if 2, stinger)
    public int[] LegFunction = { 1, 0, 0, 0, 0, 0 };

    //Motion genes
    public float minSpeed = 10f;
    public float maxSpeed = 50f;
    public float rotationRange = 45f;

    //Behaviour genes
    public float GrabFood = 0.01f; //goes for food with the grabber this probability
    public float GrabCreature = 0.01f; //goes for other creatures with the grabber with this probability
    public float StingFood = 0.01f; //goes for food with the stinger with this probability
    public float StingCreature = 0.01f; //goes for other creatures with the stinger with this probability

}
              