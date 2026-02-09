using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //VALUES FOR PLAYER MOVEMENT
    public CharacterController playerController;
    private Vector3 moveDirection;
    public float gravityScale = 0.04f;
    public float moveSpeed = 7f;
    public float jumpForce = 26f;
    public float rotateSpeed = 8f;

    //FOR CAMERA
    public Transform CinnemachineCamera;
    //public Transform pivot;
    public GameObject playerModel;

    //BULLET FOR SHOOTING
    public GameObject bullet;
    public GameObject cannon;
    public float bulletForce = 1750;
    private float bulletDespawnTime = 4f;


    //ROTATION TO FACE MOUSE, BULLET (possibly head too?)
    public Camera mainCamera;
    //public GameObject playerHead;
    private Vector3 playerMouse;
    private Quaternion lookRotation;
    private Vector3 directionTarget;

    void Start()
    {
        playerController = GetComponent<CharacterController>();

        //FOR LOOKING AT MOUSE
        mainCamera = FindAnyObjectByType<Camera>();

        //LOCK CURSOR TO GAME WINDOW
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        float yStore = moveDirection.y; // PROPER GRAVITY APPLIED
        moveDirection = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));
        moveDirection = moveDirection.normalized * moveSpeed;
        moveDirection.y = yStore; // PROPER GRAVITY APPLIED

        //y-MOVEMENT : SINGLE JUMP
        if (playerController.isGrounded)
        {
            moveDirection.y = 0f; //DEAL W/ 'INFINITE SCALING GRAVITY' Y-VALUE
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
            }
        }

        //ADDING GRAVITY TO PLAYER
        moveDirection.y = moveDirection.y + (Physics.gravity.y * gravityScale);

        //CONSISTENT MOVE SPEED, REGARDLESS OF MACHINE
        playerController.Move(moveDirection * Time.deltaTime);

        AimingRotation();

        //MOVE 'playerModel' TOWARDS DIRECTION OF INPUT
       /*if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f); //SNAPS PLAYER FACING PIVOT DIRECTION
            Quaternion newRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z)); //ROTATES PLAYER TO DIRECTION OF PIVOT

            //transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, newRotation, rotateSpeed * Time.deltaTime); //ENSURES ROTATION LOOKS SMOOTH
            
        }*/ 
            
        






        //SHOOTING MECHANIC
        if (Input.GetButtonDown("Fire1"))
        {
            /*GameObject newBullet = GameObject.Instantiate(bullet, cannon.transform.position, cannon.transform.rotation) as GameObject;
            newBullet.GetComponent<Rigidbody>().linearVelocity += Vector3.up * 2;
            newBullet.GetComponent<Rigidbody>().AddForce(newBullet.transform.forward * 1500);*/

            AimingBullet();
        }

        

    }//END OF UPDATE()

    public void AimingRotation()
    {
        //SHOOTS RAY CAST TOWARDS MOUSE
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        //CHECK IF RAY CAST HITS OBJECT
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Default")))
        {
            playerMouse = hit.point;
        }

        //
        
        
            directionTarget = (playerMouse - transform.position).normalized;
            
            //TARGET LOOK ROTATION
            lookRotation = Quaternion.LookRotation(directionTarget);
            
            //SMOOTHLY ROTATE TOWARDS TARGET
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
        
        
        
            

        

        //LOCK X-ROTATION SO DOESN'T MOVE UP/DOWN
        //transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);


    }

    public void AimingBullet()
    {
        //SHOOTS RAY CAST TOWARDS MOUSE
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        //CHECK IF RAY CAST HITS OBJECT
        if(Physics.Raycast(ray, out hit))
        {
            playerMouse = hit.point;
        }

        
            directionTarget = (playerMouse - transform.position).normalized;

            //TARGET LOOK ROTATION
            lookRotation = Quaternion.LookRotation(directionTarget);

            //SMOOTHLY ROTATE TOWARDS TARGET
            GameObject newBullet = GameObject.Instantiate(bullet, cannon.transform.position, cannon.transform.rotation) as GameObject;
            newBullet.transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
            cannon.transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);

            //SHOOT BULLET @ PLAYER'S MOUSE LOCATION
            newBullet.transform.eulerAngles = new Vector3(playerMouse.x, playerMouse.y, playerMouse.x);
            newBullet.GetComponent<Rigidbody>().linearVelocity += (Vector3.up * playerMouse.z).normalized;
            newBullet.GetComponent<Rigidbody>().AddForce(directionTarget * bulletForce);
            //transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            Destroy(newBullet, bulletDespawnTime);
        
    }
}
