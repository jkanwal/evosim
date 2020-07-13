using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class LegBehaviour : MonoBehaviour
{
    //load in genome, materials, public lists of food/creatures
    public Genome genome;
    public Grab grab;
    public Sting sting;

    public Material grabberColour;
    public Material stingerColour;
    public int legType; //0 (inert), 1 (grabber), or 2 (stinger)
    private float foodPref = 0;
    private float creaturePref = 0;
    private Vector3[] legDirections;
    public Vector3 legDirection;
    public string legName;

    void Start()
    {
        //Get the instructions from the genome as to what type of leg this is
        int[] LegGenes = transform.parent.GetComponent<Genome>().LegFunction;
        legName = gameObject.name; 
        int legNum = int.Parse(legName[legName.Length-1].ToString()) - 1;
        legType = LegGenes[legNum];

        //set my colour and get food & creature prefs from genome
        if (legType == 1)
        {
            GetComponent<Renderer>().material = grabberColour;
            foodPref = transform.parent.GetComponent<Genome>().GrabFood;
            creaturePref = transform.parent.GetComponent<Genome>().GrabCreature;
        }
        else if (legType == 2)
        {
            GetComponent<Renderer>().material = stingerColour;
            foodPref = transform.parent.GetComponent<Genome>().StingFood;
            creaturePref = transform.parent.GetComponent<Genome>().StingCreature;
        }     

        //What's my leg direction?
        legDirections = new Vector3[] { transform.up, -transform.up, transform.right, -transform.right, transform.forward, -transform.forward }; 
        legDirection = legDirections[legNum];

        //Ignore collisions with my parent creature
        Physics.IgnoreCollision(transform.parent.gameObject.GetComponent<Collider>(), GetComponent<Collider>());

        //Ignore collisions with legs of 

        sting = GetComponent<Sting>();
    }

    //Different collision behaviour depending on whether you're a stinger or grabber
    void OnTriggerEnter(Collider other)
    {            
        /*
        //if my parent creature is grabbed, ignore collisions with the grabbing creature
        if (transform.parent.CompareTag("Grabbed"))
        {
            Physics.IgnoreCollision(transform.parent.parent.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
        }
        */
        //if I haven't been stung, do stuff:
        if (!gameObject.CompareTag("Inert"))
        {
            //if I'm a grabber...
            if (legType == 1)
            {
                if (other.gameObject.layer == 9)
                {
                    float rand = Random.value;
                    if (other.CompareTag("Pick Up")) //if it's food, grab with probability foodPref
                    {
                        if (rand <= foodPref)
                        {
                            transform.parent.GetComponent<Grab>().GrabFood(other, legDirection, legName); 
                            legType = 0; //turn this leg inert, until it gets a message to change back to a grabber
                        }
                    }
                    else if (!other.CompareTag("Grabbed")) //if it's a creature (not already grabbed), or part of one (e.g. a leg), grab with probability foodPref
                    {
                        if (rand <= creaturePref)
                        {
                            transform.parent.GetComponent<Grab>().GrabCreature(other, legDirection, legName); 
                            legType = 0; //turn this leg inert, until it gets a message to change back to a grabber
                        }
                    }
                }
            }
            //if I'm a stinger...
            if (legType == 2)
            {
                if (other.gameObject.layer == 9)
                {
                    float rand = Random.value;
                    if (other.CompareTag("Pick Up")) //if it's food, sting with probability foodPref
                    {
                        if (rand <= foodPref)
                        {
                            sting.StingFood(other);
                        }
                    }
                    else //if it's a creature, or part of one (e.g. a leg), sting with probability foodPref
                    {
                        if (rand <= creaturePref)
                        {
                            sting.StingCreature(other);
                        }
                    }
                }
            }
        }    
    }

}