using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class UnitNetworkStateMachine : NetworkBehaviour
{
    private Animator anim;


    #region animationStrings

    [Header("Start Animation")]
    [SerializeField] private string currentAnimation = "Idle";


    [Header("Animation Names")]

    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string runAnimation = "Walk";

    [SerializeField] private string attackAnimation = "Attack";

    [SerializeField] private string hurtAnimation = "Hurt";
    [SerializeField] private string deathAnimation = "Death";

    #endregion


    [SerializeField] private bool dead;




    private void Start()
    {
        anim = GetComponent<Animator>();
    }




    #region Change/Transition Animation + Server Sync Functions

    /// <returns>true if the animation has changed, false otherwise</returns>
    private bool TryTransitionAnimation(string animationString, float transitionDuration = 1, float speed = 1, int layer = 0)
    {
        //if the new animation is the same as current, return false
        if (currentAnimation == animationString) return false;


        SyncAnimation_ServerRPC(animationString, transitionDuration, speed, layer);

        currentAnimation = animationString;

        anim.speed = speed;
        anim.CrossFade(animationString, transitionDuration, layer);

        return true;
    }

    /// <summary>
    /// Sent Animation Data trough server, back to all clients.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void SyncAnimation_ServerRPC(string animationString, float transitionDuration = 1, float speed = 1, int layer = 0)
    {
        SyncAnimation_ClientRPC(animationString, transitionDuration, speed, layer);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncAnimation_ClientRPC(string animationString, float transitionDuration = 1, float speed = 1, int layer = 0)
    {
        //dont execute RPC for ownerClient of this unit (since that executes locally)
        if (OwnerClientId == NetworkManager.LocalClientId) return;

        anim.speed = speed;
        anim.CrossFade(animationString, transitionDuration, layer);
    }

    #endregion



    public void Idle()
    {
        if (dead) return;

        TryTransitionAnimation(idleAnimation);
    }

    public void Run()
    {
        if (dead) return;

        TryTransitionAnimation(runAnimation);
    }

    public void Attack()
    {
        if (dead) return;

        TryTransitionAnimation(attackAnimation);

        StopAllCoroutines();
        StartCoroutine(AutoTransition(idleAnimation));
    }

    public void GetHurt()
    {
        if (dead) return;

        TryTransitionAnimation(hurtAnimation);

        StopAllCoroutines();
        StartCoroutine(AutoTransition(idleAnimation));
    }

    public void Die()
    {
        dead = true;

        TryTransitionAnimation(deathAnimation);
    }



    private IEnumerator AutoTransition(string animationString)
    {
        float clipTime = anim.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(clipTime);

        TryTransitionAnimation(animationString);
    }
}