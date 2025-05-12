using System.Collections;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class NPC : Player
{
    protected override void onEnable()
    {
        base.onEnable();
        EventManager.Instance.Subscribe("GirlSinging", onGirlSinging);
    }

    private void onGirlSinging(object message)
    {
        StopAllCoroutines();
        StartCoroutine(MoveForSeconds((float)message));
    }

    IEnumerator MoveForSeconds(float secondsToMove)
    {
        //Calculate random delays for starting and stopping
        float randomStartDelay = Random.Range(0f, 1f);
        float randomStopDelay = Random.Range(0f, 1f);

        // Wait for the random start delay
        yield return new WaitForSeconds(randomStartDelay);

        //Start moving 
        float moveDuration = secondsToMove - 1.64f - randomStartDelay; //Account for decelaration and stop delay

        if (moveDuration > 0f)
        {
            playerAnimator.SetBool("run", true);
            currentMovement = Vector3.forward;
            yield return new WaitForSeconds(moveDuration);
        }

        playerAnimator.SetBool("run", false);
        currentMovement = Vector3.zero;
        yield return new WaitForSeconds(1.64f + randomStopDelay);

        //Stop Completely

    }

}
