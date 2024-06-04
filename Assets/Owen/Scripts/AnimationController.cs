using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    void Start ()
    {
        animator = (Animator)GetComponentInChildren(typeof(Animator));

        if ( animator == null )
            animator = (Animator)GetComponent(typeof(Animator));
    }

    public void OnShoot ()
    {
        animator.SetTrigger("isAttacking");
    }

    public void OnDash ()
    {
    }

    public void OnAbilitySteal ()
    {
        animator.SetTrigger("isStealing");
    }

    public void OnMovement ( InputAction.CallbackContext ctx )
    {
        SetInput(ctx.ReadValue<Vector2>());
    }

    private void SetInput ( Vector2 pos )
    {
        if ( pos.x > 0 || pos.x < 0 )
        {
            animator.SetBool("isWalkingSideways", true);
        }
        else
        {
            animator.SetBool("isWalkingSideways", false);
        }

        if ( pos.y > 0 )
        {
            animator.SetBool("isWalkingForward", true);
        }
        else
        {
            animator.SetBool("isWalkingForward", false);
        }

        if ( pos.y < 0 )
        {
            animator.SetBool("isWalkingBackward", true);
        }
        else
        {
            animator.SetBool("isWalkingBackward", false);
        }
    }
}