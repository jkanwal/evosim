using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class Sting : MonoBehaviour
{
    public Material stingerColour;
    private GameObject creature;
    Transform arenaTransform;

    public void StingFood(Collider other)
    {
        other.transform.parent = null; //detach collision object from any parent
        other.tag = "Inert"; //tag it as inert
        other.gameObject.layer = 8; //add it to inert layer
        Renderer rend = other.GetComponent<Renderer>(); //change its colour
        rend.material = stingerColour;
    }

    public void StingCreature(Collider other)
    {
        //first figure out if collision object is a creature or just a leg of a creature
        if (other.CompareTag("Leg"))
        {
            creature = other.transform.parent.gameObject;
            arenaTransform = other.transform.parent.parent;
        }
        else
        {
            creature = other.gameObject;
            arenaTransform = other.transform.parent;
        }
        //tag all child objects inert and change their colour
        Transform[] children = creature.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            child.gameObject.tag = "Inert";
            child.gameObject.layer = 8;
            if (child.gameObject.GetComponent<Renderer>() != null)
            {
                Renderer rend = child.gameObject.GetComponent<Renderer>();
                rend.material = stingerColour;
            }
        }
        creature.transform.SetParent(arenaTransform); //detach collision object from any immediate parents...reparent it to just the arena
        /*
        Rigidbody rBody = creature.GetComponent<Rigidbody>();
        rBody.isKinematic = true; //turn the dead creature into kinematic rbody
        creature.tag = "Inert"; //tag object itself as inert
        creature.layer = 8; //add it to inert raycast layer
        //change colour of entire object
        Renderer[] childrenR = creature.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in childrenR)
        {
            rend.material = stingerColour;
        }
        creature.GetComponent<CreatureBehaviour>().points = 0; //set fitness value to zero as it's lost its chance to reproduce
        */
    }

}