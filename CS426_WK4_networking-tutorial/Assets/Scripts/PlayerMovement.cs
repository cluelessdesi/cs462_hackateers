using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

// adding namespaces
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
// because we are using the NetworkBehaviour class
// NewtorkBehaviour class is a part of the Unity.Netcode namespace
// extension of MonoBehaviour that has functions related to multiplayer
public class PlayerMovement : NetworkBehaviour
{
    public float speed = 2f;
    // create a list of colors
    public List<Color> colors = new List<Color>();

    // getting the reference to the prefab
    [SerializeField]
    private GameObject spawnedPrefab;
    // save the instantiated prefab
    private GameObject instantiatedPrefab;

    public GameObject cannon;
    public GameObject bullet;

    // reference to the camera audio listener
    [SerializeField] private AudioListener audioListener;
    // reference to the camera
    [SerializeField] private Camera playerCamera;

    //PLAYER PROPERTIES
    public CharacterController playerController;
    private Vector3 moveDirection;
    public float gravityScale = 0.04f;
    public float moveSpeed = 7f;
    public float jumpForce = 26f;
    public float rotateSpeed = 8f;

    //CAMERA FOLLOWS MOUSE
    public Camera mainCamera;
    private Vector3 playerMouse;
    private Quaternion lookRotation;
    private Vector3 directionTarget;

    //BULLET PROPERTIES
    public float bulletForce = 1750;
    private float bulletDespawnTime = 4f;


    //LOCK CURSOR STATE OF GAME
    bool wantCursorLock = true;

    public CinemachineCamera vc;
    public int priorityValue = 4;

    public GameManager gameManager;
    
    // Start is called before the first frame update
    void Start()
    {   
        playerController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Confined;
        gameManager = FindAnyObjectByType<GameManager>();
    }
    // Update is called once per frame
    void Update()
    {
        // check if the player is the owner of the object
        // makes sure the script is only executed on the owners 
        // not on the other prefabs 
        if (!IsOwner) return;
        
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

        /*Vector3 moveDirection = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection.z = +1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection.z = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection.x = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection.x = +1f;
        }
        transform.position += moveDirection * speed * Time.deltaTime;*/


        // if I is pressed spawn the object 
        // if J is pressed destroy the object
        if (Input.GetKeyDown(KeyCode.I))
        {
            //instantiate the object
            instantiatedPrefab = Instantiate(spawnedPrefab);
            // spawn it on the scene
            instantiatedPrefab.GetComponent<NetworkObject>().Spawn(true);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            //despawn the object
            instantiatedPrefab.GetComponent<NetworkObject>().Despawn(true);
            // destroy the object
            Destroy(instantiatedPrefab);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            // call the BulletSpawningServerRpc method
            // as client can not spawn objects
            //AimingBullet();
            BulletSpawningServerRpc(cannon.transform.position, cannon.transform.rotation);
        }

        //ADJUST ROTATE SPEED
        if (Input.GetKeyDown(KeyCode.Q))
        {
            rotateSpeed -= 0.5f;
            if (rotateSpeed == 0f)
            {
                rotateSpeed = 0.5f;
                
            }
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            rotateSpeed += 0.5f;
        }

        //UNLOCK FROM SCREEN
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            wantCursorLock = !wantCursorLock;
            if (wantCursorLock)
            {
                Cursor.lockState = CursorLockMode.Confined;
            } else
            {
                Cursor.lockState = CursorLockMode.None;
            }
                
        }
    }

    // this method is called when the object is spawned
    // we will change the color of the objects
    public override void OnNetworkSpawn()
    {
        GetComponent<MeshRenderer>().material.color = colors[(int)OwnerClientId];
        
        
      
        // check if the player is the owner of the object
        if (IsOwner)
        {
            //PROPER CINNEMACHINE CAMERA PRIORITY - FOLLOWS CORRECT PLAYER ON SPAWN
            int priorityValueUse = FindAnyObjectByType<GameManager>().GetPriorityValue();
            vc.Priority = priorityValueUse;
            FindAnyObjectByType<GameManager>().SubtractPriorityValue();
            audioListener.enabled = true;
            playerCamera.enabled = true;
            mainCamera.enabled = true;
        } else
        {
            vc.Priority = 0;
            return;
        }
            //mainCamera = FindAnyObjectByType<Camera>();
            // if the player is the owner of the object
            // enable the camera and the audio listener
           
    }

    // need to add the [ServerRPC] attribute
    [ServerRpc]
    // method name must end with ServerRPC
    private void BulletSpawningServerRpc(Vector3 position, Quaternion rotation)
    {
        // call the BulletSpawningClientRpc method to locally create the bullet on all clients
        BulletSpawningClientRpc(position, rotation);
    }

    [ClientRpc]
    private void BulletSpawningClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject newBullet = Instantiate(bullet, position, rotation);
        newBullet.GetComponent<Rigidbody>().linearVelocity += Vector3.up * 2;
        //newBullet.GetComponent<Rigidbody>().AddForce(newBullet.transform.forward * 1500);
        newBullet.GetComponent<Rigidbody>().AddForce(newBullet.transform.forward * bulletForce);
        Destroy(newBullet, bulletDespawnTime);
        // newBullet.GetComponent<NetworkObject>().Spawn(true);
    }

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

    /*public void AimingBullet()
    {
        //SHOOTS RAY CAST TOWARDS MOUSE
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        //CHECK IF RAY CAST HITS OBJECT
        if (Physics.Raycast(ray, out hit))
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

    }*/
}