using System;
using System.Collections;
using TMPro;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    protected Vector3 currentMovement;
    Vector2 currentMovementInput;
    Rigidbody characterController;
    protected Animator playerAnimator;
    AudioSource audioSource;

    [SerializeField] float acceleration = 10f;
    [SerializeField] float maxSpeed = 5f;
    [SerializeField] float deceleration = 3f;
    

    bool isSafe = false;

    public bool IsMoving => playerAnimator.GetBool("running") || playerAnimator.GetBool("stopping");
    public bool IsSafe => isSafe;

    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "RedLine" )
        {
            isSafe = true;
            if (gameObject.CompareTag("Player"))
            {
                GameManager.Instance.PlayerWon();
            }
            
        }
    }

    public void KillPlayer()
    {
        if (PlayerIsDead() || isSafe) return;
        audioSource.Play();
        playerAnimator.SetBool("die", true);
        GetComponent<Collider>().enabled = false;

        // Trigger result UI only if this is the Player
        if (CompareTag("Player"))
        {
            GameManager.Instance.PlayerDied();
        }

    }

    public bool PlayerIsDead()
    {
        return playerAnimator.GetBool("die");
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(WaitForGameManagerAndStartUIUpdate());
    }

    IEnumerator WaitForGameManagerAndStartUIUpdate()
    {
        // Wait until GameManager.Instance and PlayerState are initialized
        while (GameManager.Instance == null || GameManager.Instance.PlayerState == null)
        {
            yield return null;
        }

        StartCoroutine(UpdatePlayerStateUI());
    }

    void Awake()
    {
        characterController = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        audioSource = transform.Find("GunShot").GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void PlayerMovement()
    {
        if (currentMovement != Vector3.zero && !PlayerIsDead() && !IsSafe)
        {
            playerAnimator.SetBool("stopping", false);
            characterController.AddForce(currentMovement.normalized * acceleration, ForceMode.Acceleration);
        }
        else
        {
            playerAnimator.SetBool("stopping", true);
            characterController.AddForce(-characterController.linearVelocity.normalized * deceleration, ForceMode.Acceleration);
        }

        // limit speed
        if (characterController.linearVelocity.magnitude > maxSpeed)
        {
            characterController.linearVelocity = characterController.linearVelocity.normalized * maxSpeed;
        }

        if (IsMagnituteLowerThan())
        {
            playerAnimator.SetBool("stopping", false);
            characterController.linearVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.GameStarted) return;
        PlayerMovement();
    }

    public void OnMove(InputValue value)
    {
        if (!GameManager.Instance.GameStarted || PlayerIsDead()) return;

        if (PlayerIsDead()) return;
        currentMovementInput = value.Get<Vector2>();

        if (currentMovementInput != Vector2.zero)
        {
            playerAnimator.SetBool("running", true);
            GameManager.Instance.PlayerState.text = "<color=red>Running</color>";
        }
        else
        {
            playerAnimator.SetBool("running", false);
            GameManager.Instance.PlayerState.text = "<color=yellow>Stopping</color>";
        }

        currentMovement.x = currentMovementInput.x;
        currentMovement.z = Math.Abs(currentMovementInput.y); // Restrict movement backward 
    }

    bool IsMagnituteLowerThan(float minMagnitute = 0.1f)
    {
        return characterController.linearVelocity.magnitude < minMagnitute;
    }

    IEnumerator UpdatePlayerStateUI()
    {
        while (true && gameObject.CompareTag("Player"))
        {
            if (playerAnimator.GetBool("running"))
            {
                GameManager.Instance.PlayerState.text = "<color=red>Running</color>";
            }
            else if (playerAnimator.GetBool("die"))
            {
                GameManager.Instance.PlayerDied();
            }
            else if (playerAnimator.GetBool("stopping"))
            {
                GameManager.Instance.PlayerState.text = "<color=yellow>Stopping</color>";
            }
            else
            {
                GameManager.Instance.PlayerState.text = "<color=green>Stopped</color>";
            }

            yield return new WaitForSeconds(0.1f); // smooth update
        }
    }

}

