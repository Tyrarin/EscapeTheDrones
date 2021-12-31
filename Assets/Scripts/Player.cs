using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float speed = 0.001f; //speed at which the player can move
    private Rigidbody rigidBodyComponent;
    public int HP; //total amount of health point
    public float sizeBonusSphere; //diameter of all bonus spheres
    public List<GameObject> bonusSpheres = new List<GameObject>(); //list of all the bonus spheres on the map
    public float timerBonusSleep = 0f; //timer used when isBonusSleepActive is true
    public bool isBonusSleepActive = false; //boolean that is set to true when the player has consummed a blue sphere
    public bool isPlayerDead;
    public List<Drone> drones = new List<Drone>{}; //list of all the drones that is used when the player has consummed a black sphere
    public List<Robot> robots = new List<Robot>{}; //list of all the robots that is used when the player has consummed a black sphere
    public GameObject ground; //GameObject of the cube that hosts every other physical objets
    public GameObject finalDestination; //GameObject of the green circle the player has to get to.
    public Camera mainCamera; //the camera. Actually there's only one in this game
    public bool isCameraOnPlayer; //boolean that is set to true if the camera is set to first person view
    public float lookSpeed = 2.0f; //speed at which the camera can rotate on the y axis 
    public float rotationY = 0f; //float the record how much the mouse moves (left/right)
    public bool isKeyWPressed; //boolean that is set to true when the 'W' key is pressed (Input.GetKeyDown) and to false when the key isn't pressed anymore (Input.GetKeyUp)
    public bool isKeyRightPressed; //boolean that is set to true when the right arrow key is pressed (Input.GetKeyDown) and to false when the key isn't pressed anymore (Input.GetKeyUp)
    public bool isKeyLeftPressed; //boolean that is set to true when the left arrow key is pressed (Input.GetKeyDown) and to false when the key isn't pressed anymore (Input.GetKeyUp)
    public bool isKeyUpPressed; //boolean that is set to true when the up arrow key is pressed (Input.GetKeyDown) and to false when the key isn't pressed anymore (Input.GetKeyUp)
    public bool isKeyDownPressed; //boolean that is set to true when the down arrow key is pressed (Input.GetKeyDown) and to false when the key isn't pressed anymore (Input.GetKeyUp)
    public bool hasBeenDetected = false; //boolean that is set to true when the played is detected by a raycast from a drone
    public GameObject startPanel; //panel that explains the rules and that is shown when the game starts
    public GameObject logPanel; //panel that shows the communications between the drones
    public GameObject gameOverPanel; //panel that is shown when the player dies
    public GameObject successPanel; //panel that is shown when the player wins
    public float timerGameOver = 0f; //timer that is used to slightly delay the gameOver panel in order to display the final communications from the drones
    public Slider slider; //health bar that decreases along with the player's health points
    public GameObject[] walls; //list of all the walls that is used to place and rotate them randomly in the Start() method
    public float timerTextBonus;
    public Text textBonus;


    /*
    Method called from Robot.cs when an offensive raycast damages the player in order to accordingly update the health bar. 
    */
    public void SetHealth(int currentValue)
    {
        slider.value = this.HP;
    }

    /*
    Method called at every iteration of Update() if isCameraOnPlayer is set to true in order to synchronize the direction of the camera with the player.
    */
    void CameraMove(){
        if(!this.startPanel.activeSelf)
        {
            this.mainCamera.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - this.gameObject.transform.localScale.y/1.5f, this.transform.position.z);
            rotationY += -Input.GetAxis("Mouse X") * lookSpeed;
            mainCamera.transform.localRotation = Quaternion.Euler(0f, -1f * rotationY, 0f);
            this.transform.localRotation = this.mainCamera.transform.localRotation;
        }
    }


    /*
    Method called when the Esc key is pressed while startPanel is active. Lets the game truly start by activating the health bar and the log panel and by letting time flow normally. 
    */
    public void StartGame()
    {
        Time.timeScale = 1f;
        this.startPanel.SetActive(false);
        this.logPanel.SetActive(true); 
        this.slider.gameObject.SetActive(true);
        this.slider.maxValue = 300;
    }

    void Start()
    {
        this.isPlayerDead = false;
        this.startPanel = GameObject.Find("Start");
        this.logPanel = GameObject.Find("Log");
        this.gameOverPanel = GameObject.Find("GameOver");
        this.successPanel = GameObject.Find("Success");
        this.startPanel.SetActive(true);
        this.logPanel.SetActive(false);
        this.gameOverPanel.SetActive(false);
        this.successPanel.SetActive(false);
        this.slider.gameObject.SetActive(false);
        Time.timeScale = 0f; //time is null while the player reads the rules
        isKeyWPressed = false;
        isKeyLeftPressed = false;
        isKeyRightPressed = false;
        isKeyDownPressed = false;
        isKeyUpPressed = false;
        this.isCameraOnPlayer = true;
        float sizeGroundX = ground.transform.localScale.x;
        float sizeGroundY = ground.transform.localScale.y;
        float sizeGroundZ = ground.transform.localScale.z;

        this.walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach(var wall in this.walls)
        {
            //each wall is randomly placed on the map with a random orientation as well.
            wall.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX/2 - wall.transform.localScale.z/2), (sizeGroundX/2 - wall.transform.localScale.z/2)), 2.65f, Random.Range(-1f * (sizeGroundZ/2 - wall.transform.localScale.z/2), (sizeGroundZ/2 - wall.transform.localScale.z/2)));
            wall.transform.Rotate(0, Random.Range(0, 180), 0);
        }

        this.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX-1)/2, (sizeGroundX-1)/2), 1.4f, Random.Range(-1f * (sizeGroundZ-1)/2, (sizeGroundZ-1)/2)); //the player is placed randomly on the map
        this.finalDestination.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX-1)/2, (sizeGroundX-1)/2), sizeGroundY/2 + 0.01f, Random.Range(-1f * (sizeGroundZ-1)/2, (sizeGroundZ-1)/2)); //the green circle is also placed randomly
        float distX = Mathf.Abs(this.finalDestination.transform.position.x - this.transform.position.x);
        float distZ = Mathf.Abs(this.finalDestination.transform.position.z - this.transform.position.z);
        while(Mathf.Sqrt(distX * distX + distZ * distZ) < sizeGroundX/1.9) //with this loop I ensure that the green circle is not too close to the player
        {
            this.finalDestination.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX-1)/2, (sizeGroundX-1)/2), sizeGroundY/2 + 0.01f, Random.Range(-1f * (sizeGroundZ-1)/2, (sizeGroundZ-1)/2)); //if necessary we calculate another random location for the green circle 
            distX = Mathf.Abs(this.finalDestination.transform.position.x - this.transform.position.x);
            distZ = Mathf.Abs(this.finalDestination.transform.position.z - this.transform.position.z);        
        }
        this.HP =300; //max amount of health points
        this.rigidBodyComponent = GetComponent<Rigidbody>();
        foreach(var bonusSphere in GameObject.FindGameObjectsWithTag("Bonus"))
        {
            this.bonusSpheres.Add(bonusSphere); //each GameObject with the tag "Bonus" is added to the bonusSpheres list
            if(bonusSphere.transform.rotation.x == 0f) //a way I found to differentiate blue spheres from black spheres: in the inspector I put a 0 in the transform.rotation.x for blue spheres and a 1 for black one
                bonusSphere.GetComponent<Renderer>().material.color = Color.blue; //this way I let the script itself color the spheres so that there's no difference between my own blue (that I would give) and Color.blue 
            else 
                bonusSphere.GetComponent<Renderer>().material.color = Color.black; //same for Color.black and my own black
            bonusSphere.transform.position = new Vector3(Random.Range(-1f * (sizeGroundX-1)/2, (sizeGroundX-1)/2), 6f, Random.Range(-1f * (sizeGroundZ-1)/2, (sizeGroundZ-1)/2)); //then each sphere is randomly placed on the map but with a certain height so that it's easier for the player to see them
        }
        this.sizeBonusSphere = this.bonusSpheres[0].transform.localScale.x; 

        this.GetComponent<MeshRenderer>().enabled = false; //player is invisible in the first person view
        this.mainCamera = Camera.main;
        CameraMove();
    }


    /*
    Method called when the player's health points hit 0 (and the timerGameOver has completed its role). Lets the player eventually try again.
    */
    void GameOver()
    {
        this.slider.gameObject.SetActive(false);
        Time.timeScale = 0f;
        this.logPanel.SetActive(false);
        this.gameOverPanel.SetActive(true);
        //Destroy(GameObject.Find("Player"));
    }


    /*
    Method called when the player has found the green circle. Lets the player eventually try again.
    */
    void Success()
    {
        this.slider.gameObject.SetActive(false);
        Time.timeScale = 0f;
        //this.gameObject.SetActive(false);
        this.logPanel.SetActive(false);
        this.successPanel.SetActive(true);
    }


    void Update()
    {
        if(this.isCameraOnPlayer)
        {
            CameraMove(); //is called to update the camera orientation and position to match the player in fist person view

            if(Input.GetKeyDown(KeyCode.W)) //in first person view the 'W' key is used to make the player move forward
                this.isKeyWPressed = true;
            if(this.isKeyWPressed)
                this.transform.position += this.transform.forward * this.speed/10 * Time.deltaTime;

            if(Input.GetKeyUp(KeyCode.W))
                this.isKeyWPressed = false; //when the 'W' is not pressed anymore the player's position no longer changes 
        }

        else
        {
            //in aerial view the arrow keys are used to move the player instead
            if(Input.GetKeyDown(KeyCode.UpArrow)) //to move forward (from the camera's view it would be up)
                this.isKeyUpPressed = true;
            if(this.isKeyUpPressed)
                this.transform.position += Vector3.forward * this.speed/10 * Time.deltaTime;
            if(Input.GetKeyUp(KeyCode.UpArrow))
                this.isKeyUpPressed = false;

            if(Input.GetKeyDown(KeyCode.DownArrow)) //to move back (from the camera's view it would be down)
                this.isKeyDownPressed = true;
            if(this.isKeyDownPressed)
                this.transform.position += Vector3.back * this.speed/10 * Time.deltaTime;
            if(Input.GetKeyUp(KeyCode.DownArrow))
                this.isKeyDownPressed = false;

            if(Input.GetKeyDown(KeyCode.LeftArrow)) //to move left (from the camera's view it would be left)
                this.isKeyLeftPressed = true;
            if(this.isKeyLeftPressed)
                this.transform.position += Vector3.left * this.speed/10 * Time.deltaTime;
            if(Input.GetKeyUp(KeyCode.LeftArrow))
                this.isKeyLeftPressed = false;

            if(Input.GetKeyDown(KeyCode.RightArrow)) //to move right (from the camera's view it would be right)
                this.isKeyRightPressed = true;
            if(this.isKeyRightPressed)
                this.transform.position += Vector3.right * this.speed/10 * Time.deltaTime;
            if(Input.GetKeyUp(KeyCode.RightArrow))
                this.isKeyRightPressed = false;
        }
        if(this.HP <= 0) //player is dead
        {
            if(this.timerGameOver > 2.2f) //we wait 2.2 seconds before showing the gameOver panel in order to see the last communication
                GameOver();
            else
                this.timerGameOver += Time.deltaTime; //increments the timer
        }

        foreach(var bonusSphere in this.bonusSpheres)
        {
            //I compare the distance between the player and each bonus sphere on the x and z axis and I allow a certain between them
            if((Mathf.Abs(this.transform.position.x - bonusSphere.transform.position.x) <= 4f * (this.sizeBonusSphere/2f)) && (Mathf.Abs(this.transform.position.z - bonusSphere.transform.position.z) <= 4f * (this.sizeBonusSphere/2)))
            {
                if(bonusSphere.GetComponent<Renderer>().material.color == Color.blue) //speed bonus
                {
                    this.speed *= 1.5f; // +50% speed
                    this.textBonus.text = "Player speed + 50%";
                    this.textBonus.gameObject.SetActive(true);
                    this.timerTextBonus = 0f;
                }
                if(bonusSphere.GetComponent<Renderer>().material.color == Color.black) //sleep bonus
                {
                    this.textBonus.text = "All drones and robots are freezed for 5 seconds";
                    this.textBonus.gameObject.SetActive(true);
                    this.timerTextBonus = 0f;
                    foreach(Drone drone in Object.FindObjectsOfType<Drone>() as Drone[])
                    {
                        if(drone.isPatrol)
                        {
                            drone.isPatrol = false; //if a drone is currently patroling, it stops
                            this.drones.Add(drone); //every drone patroling is added to the list drones
                        }     
                    }
                    foreach(Robot robot in Object.FindObjectsOfType<Robot>())
                    {
                        if(robot.isActive) //only the robots currently tracking down player
                        {
                            this.robots.Add(robot); //each active robot is added the the list robots 
                            robot.isActive = false; //if a robot is currently active, it stops
                        }
                    }

                    this.timerBonusSleep = 0f; //timer is reset each time a black sphere takes effect
                    this.isBonusSleepActive = true;
                }
                this.bonusSpheres.Remove(bonusSphere); //if the player consumes a bonus sphere it's removed from the list
                Destroy(bonusSphere); //and then it's destroyed
            }
        }
        
        if(isBonusSleepActive) //the boolean is set to true when a black sphere is consumed
        {
            timerBonusSleep += Time.deltaTime; //increments the timer
            if(timerBonusSleep >= 5f) //time is up for the effects of the black sphere
            {
                foreach(var drone in this.drones)
                {
                    drone.isPatrol = true; //each drone that was patroling before being freezed is patroling again
                    this.drones.Remove(drone); //then it's removed from the list
                }
                foreach(var robot in this.robots)
                {
                    robot.isActive = true; //each robot that was active before being freezed is active again
                    this.robots.Remove(robot); //then it's removed from the list
                }
                this.isBonusSleepActive = false; 
            }
        }

        if((Mathf.Abs(this.transform.position.x - finalDestination.transform.position.x) <= 1.5f * this.sizeBonusSphere/2f) && (Mathf.Abs(this.transform.position.z - finalDestination.transform.position.z) <= 1.5f * this.sizeBonusSphere/2))
        {
            this.Success(); //if the player is close enough to the green circle the game is finished and the player won
        }
        if(Input.GetKeyDown(KeyCode.C) && hasBeenDetected) //pressing the 'C' key lets the player change the view but only when it has been detected because otherwise it would be too easy
        {
            //reset of the booleans
            this.isKeyWPressed = false;
            this.isKeyLeftPressed = false;
            this.isKeyRightPressed = false;
            this.isKeyDownPressed = false;
            this.isKeyUpPressed = false;
            
            if(this.isCameraOnPlayer) //in the case the player is switching from first person view to aerial view 
            {
                this.GetComponent<MeshRenderer>().enabled = true;
                this.speed /= 2.5f; //the player speed is way lower in aerial view because otherwise it would be too easy
                this.isCameraOnPlayer = false;
                this.mainCamera.transform.position = new Vector3(0f, 113f, 0f);
                this.mainCamera.transform.localRotation = Quaternion.Euler(90, 0f, 0f);
            }
            else //in the case the player is switching from aerial view to first person view
            {
                this.GetComponent<MeshRenderer>().enabled = false;
                this.isCameraOnPlayer = true;
                this.speed *= 2.5f;
            }
        }
        if(this.startPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            this.StartGame(); //when the player presses the Esc key at the very start after having read the rules


        if((this.gameOverPanel.activeSelf || this.successPanel.activeSelf) && Input.GetKeyDown(KeyCode.G)) //the player pressed the 'G' while having either lost or won
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); //restarts the whole scene to play a new party
            StartGame();
        }

        if(this.textBonus.gameObject.activeSelf)
        {
            if(this.timerTextBonus > 2f)
            {
                this.textBonus.text = "";
                this.textBonus.gameObject.SetActive(false);
            }
            else
                this.timerTextBonus += Time.deltaTime;
        }
    }
    


}
