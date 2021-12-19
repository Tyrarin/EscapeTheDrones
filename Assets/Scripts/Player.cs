using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float gravity = 9.81f;
    public float speed = 0.001f;
    private float vertical;
    private float horizontal;
    private Rigidbody rigidBodyComponent;
    public int HP;
    public float sizeBonusSphere;
    public List<GameObject> bonusSpheres = new List<GameObject>();
    public float timerBonusSleep = 0f;
    public bool isBonusSleepActive = false;
    public List<Drone> drones = new List<Drone>{};
    public List<Robot> robots = new List<Robot>{};
    public GameObject ground;
    public GameObject finalDestination;
    public Camera mainCamera;
    public bool isCameraOnPlayer;
    public float lookSpeed = 2.0f; //for camera
    public float rotationY = 0f;
    public bool isKeyWPressed;
    public bool isKeyRightPressed;
    public bool isKeyLeftPressed;
    public bool isKeyUpPressed;
    public bool isKeyDownPressed;
    public bool hasBeenDetected = false;



    void CameraMove(){
        this.mainCamera.transform.position = this.transform.position;
        rotationY += -Input.GetAxis("Mouse X") * lookSpeed;
        mainCamera.transform.localRotation = Quaternion.Euler(0f, -1f * rotationY, 0f);
        this.transform.localRotation = this.mainCamera.transform.localRotation;
    }
    // Start is called before the first frame update
    void Start()
    {
        isKeyWPressed = false;
        isKeyLeftPressed = false;
        isKeyRightPressed = false;
        isKeyDownPressed = false;
        isKeyUpPressed = false;
        this.isCameraOnPlayer = true;
        float sizeGroundX = ground.transform.localScale.x;
        float sizeGroundY = ground.transform.localScale.y;
        float sizeGroundZ = ground.transform.localScale.z;
        this.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX-1)/2, (sizeGroundX-1)/2), 1.4f, Random.Range(-1f * (sizeGroundZ-1)/2, (sizeGroundZ-1)/2));
        this.finalDestination.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX-1)/2, (sizeGroundX-1)/2), sizeGroundY/2 + 0.01f, Random.Range(-1f * (sizeGroundZ-1)/2, (sizeGroundZ-1)/2));
        this.HP = 300;
        this.rigidBodyComponent = GetComponent<Rigidbody>();
        foreach(var bonusSphere in GameObject.FindGameObjectsWithTag("Bonus"))
        {
            this.bonusSpheres.Add(bonusSphere);
            if(bonusSphere.transform.rotation.x == 0f)
                bonusSphere.GetComponent<Renderer>().material.color = Color.blue;
            else 
                bonusSphere.GetComponent<Renderer>().material.color = Color.black;
            bonusSphere.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX-1)/2, (sizeGroundX-1)/2), 6f, Random.Range(-1f * (sizeGroundZ-1)/2, (sizeGroundZ-1)/2));
        }
        this.sizeBonusSphere = this.bonusSpheres[0].transform.localScale.x; 

        
        this.mainCamera = Camera.main;
        CameraMove();

    }

    void GameOver(){
        print("U dead");
        GameObject.Find("Player").SetActive(false);
        //Destroy(GameObject.Find("Player"));
    }

    // Update is called once per frame
    void Update()
    {
        this.vertical = Input.GetAxis("Vertical");
        this.horizontal = Input.GetAxis("Horizontal");
        Vector3 movement = new Vector3(horizontal, 0, vertical) * this.speed * Time.deltaTime;
        if(this.isCameraOnPlayer)
        {
            CameraMove();

            if(Input.GetKeyDown(KeyCode.W))
                this.isKeyWPressed = true;
            if(this.isKeyWPressed)
                this.transform.position += this.transform.forward * this.speed/10 * Time.deltaTime;

            if(Input.GetKeyUp(KeyCode.W))
                this.isKeyWPressed = false;
        }

        else
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
                this.isKeyUpPressed = true;
            if(this.isKeyUpPressed)
                this.transform.position += Vector3.forward * this.speed/10 * Time.deltaTime;
            if(Input.GetKeyUp(KeyCode.UpArrow))
                this.isKeyUpPressed = false;

            if(Input.GetKeyDown(KeyCode.DownArrow))
                this.isKeyDownPressed = true;
            if(this.isKeyDownPressed)
                this.transform.position += Vector3.back * this.speed/10 * Time.deltaTime;
            if(Input.GetKeyUp(KeyCode.DownArrow))
                this.isKeyDownPressed = false;

            if(Input.GetKeyDown(KeyCode.LeftArrow))
                this.isKeyLeftPressed = true;
            if(this.isKeyLeftPressed)
                this.transform.position += Vector3.left * this.speed/10 * Time.deltaTime;
            if(Input.GetKeyUp(KeyCode.LeftArrow))
                this.isKeyLeftPressed = false;

            if(Input.GetKeyDown(KeyCode.RightArrow))
                this.isKeyRightPressed = true;
            if(this.isKeyRightPressed)
                this.transform.position += Vector3.right * this.speed/10 * Time.deltaTime;
            if(Input.GetKeyUp(KeyCode.RightArrow))
                this.isKeyRightPressed = false;
        }
            this.rigidBodyComponent.MovePosition(this.transform.position + movement);
        if(this.HP <= 0)
            GameOver();

        foreach(var bonusSphere in this.bonusSpheres)
        {
            if((Mathf.Abs(this.transform.position.x - bonusSphere.transform.position.x) <= 4f * (this.sizeBonusSphere/2f)) && (Mathf.Abs(this.transform.position.z - bonusSphere.transform.position.z) <= 4f * (this.sizeBonusSphere/2)))
            {
                if(bonusSphere.GetComponent<Renderer>().material.color == Color.blue) //time bonus
                {
                    this.speed *= 1.5f; // +50% speed
                    print("Just took a blue bonus");
                }
                if(bonusSphere.GetComponent<Renderer>().material.color == Color.black) //sleep bonus
                {

                    foreach(Drone drone in Object.FindObjectsOfType<Drone>() as Drone[])
                    {
                        if(drone.isPatrol)
                        {
                            drone.isPatrol = false;
                            this.drones.Add(drone);
                        }     
                    }
                    foreach(Robot robot in Object.FindObjectsOfType<Robot>())
                    {
                        if(robot.isActive) //only the robots currently tracking down player
                        {
                            this.robots.Add(robot); 
                            robot.isActive = false;
                        }
                    }

                    this.timerBonusSleep = 0f;
                    this.isBonusSleepActive = true;
                    print("Just took a black bonus");
                }
                this.bonusSpheres.Remove(bonusSphere);
                Destroy(bonusSphere);
            }
        }
        
        if(isBonusSleepActive)
        {
            timerBonusSleep += Time.deltaTime;
            if(timerBonusSleep >= 5f)
            {
                foreach(var drone in this.drones)
                {
                    drone.isPatrol = true;
                }
                foreach(var robot in this.robots)
                {
                    robot.isActive = true;
                }
                this.isBonusSleepActive = false;
            }
        }

        if((Mathf.Abs(this.transform.position.x - finalDestination.transform.position.x) <= this.sizeBonusSphere/2f) && (Mathf.Abs(this.transform.position.z - finalDestination.transform.position.z) <= this.sizeBonusSphere/2))
        {
            print("you won !!");
        }
        if(Input.GetKeyDown(KeyCode.C) && hasBeenDetected)
        {
            this.isKeyWPressed = false;
            this.isKeyLeftPressed = false;
            this.isKeyRightPressed = false;
            this.isKeyDownPressed = false;
            this.isKeyUpPressed = false;
            
            if(this.isCameraOnPlayer)
            {
                this.speed /= 2f;
                this.isCameraOnPlayer = false;
                this.mainCamera.transform.position = new Vector3(0f, 87f, 0f);
                this.mainCamera.transform.localRotation = Quaternion.Euler(90, 0f, 0f);
            }
            else
            {
                this.isCameraOnPlayer = true;
                this.speed *= 2f;
            }
        }         
        
    }

}
