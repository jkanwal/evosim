using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrabFood : MonoBehaviour
{
    public int count;
    private Vector3 offset = new Vector3(0f, 2f, 0f);

    void Start()
    {
        count = 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!gameObject.CompareTag("Grabbing") && collision.gameObject.CompareTag("Pick Up"))
        {
            gameObject.tag = "Grabbing";
            count += 1;
            collision.transform.SetParent(transform);
            collision.transform.localPosition = offset;
        }
    }

}
