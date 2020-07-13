using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class Grab : MonoBehaviour
{

    public CreatureMotion creatureMotion;
    public LegBehaviour legBehaviour;
    public Grab grab;
    public int points;
    private GameObject creature;
    public int releaseRate = 100;
    public List<GameObject> grabbedFood = new List<GameObject>();
    public List<float> releaseTimes = new List<float>();
    public List<string> legsWithFood = new List<string>();
    public List<GameObject> grabbedCreatures = new List<GameObject>();
    public List<string> legsWithCreatures = new List<string>();

    void Start()
    {
        creatureMotion = GetComponent<CreatureMotion>();
        points = 0;
    }

    void Update() 
    {
        //if I'm grabbing food, digest it and get a point after releaseRate amount of time
        if (grabbedFood.Count > 0)
        {
            for (int i = grabbedFood.Count - 1; i >= 0; i--)
            {
                if (creatureMotion.Ticks > releaseTimes[i])
                {
                    grabbedFood[i].SetActive(false); //disable digested grabbee
                    grabbedFood.RemoveAt(i); //remove from Grabbee list
                    releaseTimes.RemoveAt(i); //also remove the corresponding release time
                    GameObject leg = transform.Find(legsWithFood[i]).gameObject; //get associated limb
                    leg.GetComponent<LegBehaviour>().legType = 1; //set that limb back to grabbing mode
                    legsWithFood.RemoveAt(i); //remove associated limb from busy limbs list

                    //if I'm currently grabbed, send my points up to the top parent instead
                    if (gameObject.CompareTag("Grabbed"))
                    {
                        GameObject topParent = GetTopParent(transform); //find top parent
                        topParent.GetComponent<Grab>().points += 1; //the top parent gets a point for the food I digested for it!
                    }
                    else
                    {
                        points += 1; //I get a point for digesting food! 
                    }
                }
            }
        }
    }

    //This function finds the top parent of a creature that's not the arena
    GameObject GetTopParent(Transform transf)
    {
        Transform t = transf;
        while (!t.parent.CompareTag("Arena"))
        {
            t = t.parent;
        }
        return t.gameObject;
    }

    public void GrabFood(Collider other, Vector3 legDirection, string legName)
    {
        //if the food was being grabbed by someone else, first take it off their grabbedFood list
        if (other.transform.parent != null)
        {
            GameObject otherCreature = other.transform.parent.gameObject;
            Grab otherGrab = otherCreature.GetComponent<Grab>();
            int i = otherGrab.grabbedFood.IndexOf(other.gameObject);
            otherGrab.grabbedFood.RemoveAt(i);
            otherGrab.releaseTimes.RemoveAt(i);
            GameObject otherLeg = otherCreature.transform.Find(otherGrab.legsWithFood[i]).gameObject; //get associated limb
            otherLeg.GetComponent<LegBehaviour>().legType = 1; //set that limb back to grabbing mode
            otherGrab.legsWithFood.RemoveAt(i); 
        }
        grabbedFood.Add(other.gameObject); //add it to my grabbedFood list
        releaseTimes.Add(creatureMotion.Ticks + releaseRate); //add Ticks count for when to release 
        legsWithFood.Add(legName); //add legDirection to my legsWithFood list
        other.transform.SetParent(transform); //set it as my child
        other.transform.position = transform.position; //position it at my origin
        other.transform.localPosition = 2*legDirection; //move it out to the end of the correct limb
    }

    public void GrabCreature(Collider other, Vector3 legDirection, string legName)
    {
        //first figure out if collision object is a creature or just a leg of a creature
        if (other.CompareTag("Leg"))
        {
            creature = other.transform.parent.gameObject;
        }
        else
        {
            creature = other.gameObject;
        }

        //Turn off stealing other creatures
        /*
        //if the creature was being grabbed by someone else, free that other grabber's leg so it can grab with it again
        if (creature.CompareTag("Grabbed"))
        {
            GameObject otherCreature = creature.transform.parent.gameObject;
            Grab otherGrab = otherCreature.GetComponent<Grab>();
            int i = otherGrab.grabbedCreatures.IndexOf(creature);
            GameObject otherLeg = otherCreature.transform.Find(otherGrab.legsWithCreatures[i]).gameObject; //get associated limb
            otherLeg.GetComponent<LegBehaviour>().legType = 1; //set that limb back to grabbing mode
            otherGrab.grabbedCreatures.RemoveAt(i);
            otherGrab.legsWithCreatures.RemoveAt(i);
        }
        */

        if (!creature.CompareTag("Grabbed")) 
        {
            grabbedCreatures.Add(creature); //add it to my grabbedCreatures list
            legsWithCreatures.Add(legName); //add legDirection to my legsWithCreatures list
            creature.transform.SetParent(transform); //set it as my child
            //create a hinge joint
            HingeJoint joint = gameObject.AddComponent<HingeJoint>();
            joint.connectedBody = creature.GetComponent<Rigidbody>();
            joint.anchor = 3*legDirection;
            joint.connectedAnchor = -3*legDirection;
            joint.axis = Vector3.up;
            JointLimits limits = joint.limits;
            limits.min = 0;
            limits.max = 90;
            joint.limits = limits;
            joint.useLimits = true;
            /*
            creature.transform.position = transform.position; //position it at my origin
            creature.transform.localPosition = 2*legDirection; //move it out to the end of the correct limb
            */
            creature.tag = "Grabbed"; //change its tag
            /*
            //creature.layer = 8; //add it to inert raycast layer so it can't be grabbed by anyone else now
            Rigidbody rBody = creature.GetComponent<Rigidbody>(); 
            rBody.isKinematic = true; //turn grabbee's rbody into kinematic rbody
            */
        }
    }

}