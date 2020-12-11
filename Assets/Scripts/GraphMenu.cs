using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphMenu : MonoBehaviour
{   
    public GameObject Panel;
    // Start is called before the first frame update
    public void openPanel() {
        if(Panel != null) {
            Animator animator = Panel.GetComponent<Animator>();

            if(animator != null) {
                bool isOpen = animator.GetBool("Open");
                animator.SetBool("Open", !isOpen);
            }
        }
    }
}
