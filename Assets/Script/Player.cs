using UnityEngine;
using UnityEngine.InputSystem; // Required for InputValue

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

    public bool IsMoving => playerAnimator.GetBool("run") || playerAnimator.GetBool("stop");

    public bool IsSafe => isSafe;


    private void OnTriggerEnter(Collider other)
    {
        if(GetComponent<Collider>().tag == "RedLine")
        {
            isSafe = true;
        }
    }

    public void KillPlayer()
    {
        if(PlayerIsDead()) return;
        audioSource.Play();
        playerAnimator.SetBool("die", true);
        GetComponent<Collider>().enabled = false;
    }
    
    public bool PlayerIsDead()
    {
        return playerAnimator.GetBool("die");
    }

    protected virtual void onEnable()
    {

    }

    void Awake()
    {
        characterController = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        audioSource = transform.Find("GunShot").GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void FixedUpdate()
    {
        PlayerMovement();
    }

    void PlayerMovement()
    {
        // Convert input to movement direction
        currentMovement = new Vector3(currentMovementInput.x, 0, currentMovementInput.y);

        if (currentMovement != Vector3.zero && !PlayerIsDead())
        {
            characterController.AddForce(currentMovement.normalized * acceleration, ForceMode.Acceleration);
        }
        else
        {
            playerAnimator.SetBool("stop", true);
            characterController.AddForce(-characterController.linearVelocity.normalized * deceleration, ForceMode.Acceleration);
        }
        //Limit speed
        if (characterController.linearVelocity.magnitude > maxSpeed)
        {
            characterController.linearVelocity = characterController.linearVelocity.normalized * maxSpeed;
        }

        if (characterController.linearVelocity.magnitude < 0.1f)
        {
            playerAnimator.SetBool("stop", false);
            characterController.linearVelocity = Vector3.zero;
        }
    }

    public void OnMove(InputValue value)
    {
        if (PlayerIsDead()) return;
        currentMovementInput = value.Get<Vector2>();

        if (currentMovementInput != Vector2.zero)
        {
            playerAnimator.SetBool("run", true);
        }
        else
        {
            playerAnimator.SetBool("run", false);
        }

        currentMovement.x = currentMovementInput.x;
        currentMovement.z = Mathf.Abs(currentMovement.y); //Restrict movemnet backward
    }

    bool isMagnitudeLowerThan(float minMagnitude = 0.1f)
    {
        return characterController.linearVelocity.magnitude < minMagnitude;
    }
}