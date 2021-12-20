using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public GameObject startPanel;
    public GameObject logPanel;
    public GameObject gameOverPanel;
    public GameObject successPanel;
    public float timerGameOver = 0f;
    public Slider slider;
    public Color lowHealth;
    public Color highHealth;


    public void SetHealth(int currentValue)
    {
        slider.value = this.HP;
    }

    void CameraMove(){
        if(!this.startPanel.activeSelf)
        {
            this.mainCamera.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - this.gameObject.transform.localScale.y/1.5f, this.transform.position.z);
            rotationY += -Input.GetAxis("Mouse X") * lookSpeed;
            mainCamera.transform.localRotation = Quaternion.Euler(0f, -1f * rotationY, 0f);
            this.transform.localRotation = this.mainCamera.transform.localRotation;
        }
    }
    // Start is called before the first frame update

    public void StartGame()
    {
        Time.timeScale = 1f;
        this.startPanel.SetActive(false);
        this.logPanel.SetActive(true); 
        this.slider.gameObject.SetActive(true);
        this.SetHealth(this.HP);
        this.slider.maxValue = 300;
    }

    void Start()
    {
        this.startPanel = GameObject.Find("Start");
        this.logPanel = GameObject.Find("Queue");
        this.gameOverPanel = GameObject.Find("GameOver");
        this.successPanel = GameObject.Find("Success");
        this.startPanel.SetActive(true);
        this.logPanel.SetActive(false);
        this.gameOverPanel.SetActive(false);
        this.successPanel.SetActive(false);
        this.slider.gameObject.SetActive(false);
        Time.timeScale = 0f;
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
        float distX = Mathf.Abs(this.finalDestination.transform.position.x - this.transform.position.x);
        float distZ = Mathf.Abs(this.finalDestination.transform.position.z - this.transform.position.z);
        while(Mathf.Sqrt(distX * distX + distZ * distZ) < sizeGroundX/2.3) //to ensure that the green circle is not too close
        {
            this.finalDestination.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX-1)/2, (sizeGroundX-1)/2), sizeGroundY/2 + 0.01f, Random.Range(-1f * (sizeGroundZ-1)/2, (sizeGroundZ-1)/2));
            distX = Mathf.Abs(this.finalDestination.transform.position.x - this.transform.position.x);
            distZ = Mathf.Abs(this.finalDestination.transform.position.z - this.transform.position.z);        
        }
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

    void GameOver()
    {
        this.slider.gameObject.SetActive(false);
        Time.timeScale = 0f;
        this.logPanel.SetActive(false);
        this.gameOverPanel.SetActive(true);
        //Destroy(GameObject.Find("Player"));
    }

    void Success()
    {
        this.slider.gameObject.SetActive(false);
        Time.timeScale = 0f;
        //this.gameObject.SetActive(false);
        this.logPanel.SetActive(false);
        this.successPanel.SetActive(true);
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
        {
            if(this.timerGameOver > 2.2f)
                GameOver();
            else
                this.timerGameOver += Time.deltaTime;
        }
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

        if((Mathf.Abs(this.transform.position.x - finalDestination.transform.position.x) <= 1.5f * this.sizeBonusSphere/2f) && (Mathf.Abs(this.transform.position.z - finalDestination.transform.position.z) <= 1.5f * this.sizeBonusSphere/2))
        {
            this.Success();
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
                this.speed /= 2.5f;
                this.isCameraOnPlayer = false;
                this.mainCamera.transform.position = new Vector3(0f, 87f, 0f);
                this.mainCamera.transform.localRotation = Quaternion.Euler(90, 0f, 0f);
            }
            else
            {
                this.isCameraOnPlayer = true;
                this.speed *= 2.5f;
            }
        }
        if(this.startPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            this.StartGame();


        if((this.gameOverPanel.activeSelf || this.successPanel.activeSelf) && Input.GetKeyDown(KeyCode.G))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            StartGame();
        }
    }
    


}
