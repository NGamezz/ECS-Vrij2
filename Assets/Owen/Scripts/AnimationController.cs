using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {

        //aninimationbool for forward
        if (Input.GetButtonDown("Onmove"))
        {
            animator.SetBool("isWalkingForward", true);
        }
        else if (Input.GetButtonUp("Onmove"))
        {
            animator.SetBool("isWalkingForward", false);
        }

        //aninimationbool for sideways
        if (Input.GetButtonDown("Onmove"))
        {
            animator.SetBool("isWalkingSideways", true);
        }
        else if (Input.GetButtonUp("Onmove"))
        {
            animator.SetBool("isWalkingSideways", false);
        }

        //aninimationbool for backward
        if (Input.GetButtonDown("Onmove"))
        {
            animator.SetBool("isWalkingBackwards", true);
        }
        else if (Input.GetButtonUp("Onmove"))
        {
            animator.SetBool("isWalkingBackwards", false);
        }

        //aninimationtrigger for attack
        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger("isAttacking");
        }
        
        //aninimationtrigger for steal
        if (Input.GetButtonDown("E"))
        {
            animator.SetTrigger("isStealing");
        }

        //aninimationtrigger for dash
        if (Input.GetButtonDown("OnDash"))
        {
            animator.SetTrigger("isDodging");
        }

    }
}
