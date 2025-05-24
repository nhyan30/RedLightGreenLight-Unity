using System.Collections;
using UnityEngine;

public class NPC : Player
{


    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.Instance.Subscribe("GirlSinging", onGirlSinging);
    }

    private void onGirlSinging(object message)
    {
        StopAllCoroutines();
        StartCoroutine(MoveForSeconds((float)message));
    }

    IEnumerator MoveForSeconds(float secondsToMove)
    {
        // Calculate random delays for starting and stopping 
        float randomStartDelay = Random.Range(0f, 1f);
        float randomStopDelay = Random.Range(0f, 1f);

        // wait for the random start delay
        yield return new WaitForSeconds(randomStartDelay);

        // Start moving 
        float extraMoveTime = Random.Range(-0.5f, 0.5f);
        float moveDuration = secondsToMove - 1.64f - randomStartDelay +extraMoveTime; // Account for decleration and stop delay

        if (moveDuration > 0f)
        {
            playerAnimator.SetBool("running", true);
            currentMovement = Vector3.forward; // Start moving forward 
            yield return new WaitForSeconds(moveDuration); // move for the remaining time
        }

        playerAnimator.SetBool("running", false);
        currentMovement = Vector3.zero;
        yield return new WaitForSeconds(1.64f + randomStopDelay);

        // Stop completely
        playerAnimator.SetBool("stopping", true);
    }

}
