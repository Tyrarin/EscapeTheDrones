using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Drone : MonoBehaviour
{
    public Robot[] allRobots; //array that contains all robots
    public List<Robot> robotsSquad; //list that contains all robots that have the same tag as the drone
    public GameObject target; //the player when they're detected
    public float range = 200f; //max range of the detection raycasts
    public float speed; 
    public int detection; //the layer the player is in
    public int layer_mask; //the layer the drone has to search to find the player
    public int layer_mask_wall; //the layer the drone will send to its robots
    public GameObject ground; //GameObject of the cube that hosts every other physical objets
    public Vector3 groundSize; //size of the ground object
    public float timerRotation; //timer for the next random rotation
    public GameObject HQ; //headquarter of the squad where all the robots from the drone's squad are. The drone has to stay there to control its squad
    public bool isPatrol; //boolean that is true if the drone is searching the player and false otherwise
    public bool hasDetectedPlayer; //boolean that is set to true when the drone's detection raycasts have found the player
    public bool hasInheritedZone; //boolean that is set to true only when the drone's patrol zone has been changed by another drone
    public float zoneMinX; //minimum value the drone can take on the x axis
    public float zoneMaxX; //maximum value the drone can take on the x axis
    public float zoneMinZ; //minimum value the drone can take on the z axis
    public float zoneMaxZ; //maximum value the drone can take on the z axis
    public List<string> tags; //list that contains all the tags of the drones
    public List<Drone> drones = new List<Drone>(); //list that contains all the drones (including the drone the script is attached to)
    public Text log; //where all the communications are displayed
    public Text screenChange; //notification that the player can change the view
    public bool hasNotifiedSquad; //boolean that is true when a drone a detected the player and notified another drone (if there are any that is still patrolling)
    public bool isMissionComplete; //boolean that is true when a robot from the drone's squad has brought down the player
    public bool isTargetDead;


    /*
    Method that lets the drone emit raycasts to detect the player.
    */
    void DetectionLaser()
    {
        Vector3 current_pos = transform.position;
        //the detection area is a square 10m x 10m
        for(float i = -5f ; i <= 5f ; i=i+0.5f) 
            {
                for(float j = -5f ; j <= 5f ; j=j+0.5f)
                {
                    var ray = new Ray(current_pos, transform.TransformDirection(new Vector3(i, this.transform.position.y * -1f, j))); //each ray has to begin from the drone's current position and has to be directed towards the ground in a way that the intersection is a square
                    
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, range, layer_mask)) //the ray is thrown on the layer_mask with the range that was initialized in the attributes section. The value of the Raycast method is true if the raycast hit the and false otherwise
                    {
                        this.target = hit.transform.gameObject; 
                        Player playerScript = target.GetComponent<Player>(); //the Player script hence is accessible only if the raycast hit the player
                        this.isPatrol = false; //the drone stops patrolling
                        
                        if(!hasDetectedPlayer) //this way IntelligentRepartition is called only once
                            IntelligentRepartition(); //the drone

                        if(!playerScript.hasBeenDetected)
                        {
                            playerScript.hasBeenDetected = true;
                            this.screenChange.gameObject.SetActive(true);
                        }
                        this.hasDetectedPlayer = true;
                    }
                }
            }
        

    }


    /*
    Method that is called in Start(). Lets the drone know which HQ it has to go to awake its squad depending on its tag
    */
    public string WhichHQ()
    {
        if(this.tag == "Drone1")
            return "HQSquad1";
        if(this.tag == "Drone2")
            return "HQSquad2";
        if(this.tag == "Drone3")
            return "HQSquad3";
        if(this.tag == "Drone4")
            return "HQSquad4";
        return "error";
    }

    
    /*
    Method that lets the drone write its communications on the log panel.
    */
    public void DisplayToUI(string message)
    {
        this.log.text += "[" + Time.timeSinceLevelLoad.ToString("N2") + "] - " + this.tag + ": " + message + "\n\n";
    }


    void Start()
    {
        this.isTargetDead = false;
        this.screenChange.gameObject.SetActive(false); //the notification that the view can be changed is not visible since the player isn't detected yet 
        this.hasNotifiedSquad = false;
        this.hasDetectedPlayer = false;
        this.isPatrol = true;
        this.hasInheritedZone = false;
        this.isMissionComplete = false;
        this.allRobots = FindObjectsOfType(typeof(Robot)) as Robot[];
        this.speed = 0.30f;
        foreach (var robot in allRobots)
        {
            if(robot.gameObject.tag == this.tag)
                robotsSquad.Add(robot); //only the robots from the drone's squad are added in the list
        }
        this.HQ = GameObject.Find(WhichHQ()); //here WhichHQ() returns the name of the HQ specific the each drone
        this.timerRotation = 0f;
        this.ground = GameObject.Find("Ground");
        this.groundSize = ground.transform.localScale;
        this.detection = 8; //since the player is the layer 8
        this.layer_mask = 1 << detection; //the mask is determined this way
        this.layer_mask_wall = 1 << 7; //this mask is made for the robots squad in order to detect the walls which are in the layer 7

        this.tags = new List<string>{"Drone1", "Drone2", "Drone3", "Drone4"}; //every drone has its tag in this list at the beginning
        foreach(var droneTag in this.tags)
        {
            this.drones.Add(GameObject.Find(droneTag).GetComponent<Drone>()); //the tags list can also be used to find all the corresponding drone and put them in the drones list
        }
        float valueMin; //factor that will be used to determine the limit bounds
        float valueMax; //factor that will be used to determine the limit bounds
        switch(this.tag)
        {
            case "Drone1":
                valueMin = -1f;
                valueMax = -0.5f;
                break;
            case "Drone2":
                valueMin = -0.5f;
                valueMax = 0f;
                break;
            case "Drone3":
                valueMin = 0f;
                valueMax = 0.5f;
                break;
            case "Drone4":
                valueMin = 0.5f;
                valueMax = 1f;
                break;
            default:
                valueMin = 0;
                valueMax = 0;
                print("This isn't normal");
                break;

        }
        this.zoneMinX = valueMin * (ground.transform.localScale.x / 2f);
        this.zoneMaxX = valueMax * (ground.transform.localScale.x / 2f);
        this.zoneMinZ = -1f * (ground.transform.localScale.x / 2f);
        this.zoneMaxZ = (ground.transform.localScale.x) / 2f;
        
    }


    /*
    Method that is called when a drone detects the player. Since it has to go the its HQ, it asks another drone to patrol in its zone too.
    */
    public void IntelligentRepartition()
    {
        string buffer1 = "Target has been detected. I have to lead my robots squad. ";
        string buffer2;
        Drone inheritor = null; //the drone that will have its patrol zone increased by another drone
        if(this.tag == "Drone1") //the drone that has to give away its patrol zone to another in order to awake its squad
        {
            //all these verifications are in priority order: if drone 1 has to give its zone it will prioritize drone 2, or drone 3 if drone 2 isn't patrolling, or drone 4 if drones 2 and 3 can't patrol, or nothing if there's no drone patrolling left
            if(this.tags.Contains("Drone2"))
            {
                this.drones[1].zoneMinX = this.zoneMinX;
                this.drones[1].hasInheritedZone = true;
                buffer2 = "Drone2, keep watch in my stead.";
                inheritor = this.drones[1];
            }
            else if(this.tags.Contains("Drone3"))
            {    
                this.drones[2].zoneMinX = this.zoneMinX;
                this.drones[2].hasInheritedZone = true;
                buffer2 = "Drone3, keep watch in my stead.";
                inheritor = this.drones[2];
            }
            else if(this.tags.Contains("Drone4"))
            {    
                this.drones[3].zoneMinX = this.zoneMinX;
                this.drones[3].hasInheritedZone = true;
                buffer2 = "Drone4, keep watch in my stead.";
                inheritor = this.drones[3];
            }
            else
                buffer2 = "No drone found to inherit my patrol zone."; //there's neither drone 2, drone 3 or drone 4 to retrieve drone 1's patrol zone   
        }
        
        else if(this.tag == "Drone2")
        {
            if(this.tags.Contains("Drone1"))
            {
                this.drones[0].zoneMaxX = this.zoneMaxX;
                this.drones[0].hasInheritedZone = true;
                buffer2 = "Drone1, keep watch in my stead.";
                inheritor = this.drones[0];
            }
            else if(this.tags.Contains("Drone3"))
            {
                this.drones[2].zoneMinX = this.zoneMinX;
                this.drones[2].hasInheritedZone = true;
                buffer2 = "Drone3, keep watch in my stead.";
                inheritor = this.drones[2];
            }
            else if(this.tags.Contains("Drone4"))
            {    
                this.drones[3].zoneMinX = this.zoneMinX;
                this.drones[3].hasInheritedZone = true;
                buffer2 = "Drone4, keep watch in my stead.";
                inheritor = this.drones[3];
            }
            else
                buffer2 = "No drone found to inherit my patrol zone.";
        }

        else if(this.tag == "Drone3")
        {
            if(this.tags.Contains("Drone4"))
            {
                this.drones[3].zoneMinX = this.zoneMinX;
                this.drones[3].hasInheritedZone = true;
                buffer2 = "Drone4, keep watch in my stead.";
                inheritor = this.drones[3];
            }
            else if(this.tags.Contains("Drone2"))
            {
                this.drones[1].zoneMaxX = this.zoneMaxX;
                this.drones[1].hasInheritedZone = true;
                buffer2 = "Drone2, keep watch in my stead.";
                inheritor = this.drones[1];
            }
            else if(this.tags.Contains("Drone1"))
            {    
                this.drones[0].zoneMaxX = this.zoneMaxX;
                this.drones[0].hasInheritedZone = true;
                buffer2 = "Drone1, keep watch in my stead.";
                inheritor = this.drones[0];
            }
            else
                buffer2 = "No drone found to inherit my patrol zone.";
        }

        else if(this.tag == "Drone4")
        {
            if(this.tags.Contains("Drone3"))
            {
                this.drones[2].zoneMaxX = this.zoneMaxX;
                this.drones[2].hasInheritedZone = true;
                buffer2 = "Drone3, keep watch in my stead.";
                inheritor = this.drones[2];
            }
            else if(this.tags.Contains("Drone2"))
            {    
                this.drones[1].zoneMaxX = this.zoneMaxX;
                this.drones[1].hasInheritedZone = true;
                buffer2 = "Drone2, keep watch in my stead.";
                inheritor = this.drones[1];
            }
            else if(this.tags.Contains("Drone1"))
            {
                this.drones[0].zoneMaxX = this.zoneMaxX;
                this.drones[0].hasInheritedZone = true;
                buffer2 = "Drone1, keep watch in my stead.";
                inheritor = this.drones[0];
            }
            else
                buffer2 = "No drone found to inherit my patrol zone.";
        }
        else
            buffer2 = "Wait, what am I exactly ?";
        if(!hasNotifiedSquad)
        {
            DisplayToUI(buffer1 + buffer2);
            foreach(var drone in this.drones)
                drone.tags.Remove(this.tag);

            if(inheritor != null)
            {
                inheritor.DisplayToUI("Roger.");
                inheritor.speed *= 1.5f; //since it has to cover a larger patrol zone it has its speed increased by 50%
            }
        }
    }


    /*
    Method that is called when a drone arrives to its headquart after having given its patrol zone to another drone. 
    It awakes every robot in its list of robots and gives the player layer and the walls layer to each of them.
    */
    public void AwakeRobots()
    {
        foreach (var robot in robotsSquad)
        {
            robot.layer_mask = this.layer_mask;
            robot.layer_mask_wall = this.layer_mask_wall;
            robot.isActive = true;
        }
    }


    /*
    Method that is called every iteration of the Update() method. It checks if the drone is too close the the bounds of its zone.
    If it is, the drone turns 180 degrees and continues in the opposite direction.
    */
    public void OnBorder(float posX, float posZ)
    {
        float diffXMin = Mathf.Abs(posX - this.zoneMinX);
        float diffXMax = Mathf.Abs(this.zoneMaxX - posX);
        float diffZ = Mathf.Abs(Mathf.Abs(posZ) - groundSize.z/2f); 
        if(diffXMin < 1f || diffXMax < 1f || diffZ < 1f)
        {    
            transform.Rotate(0, 180, 0);
        }
    }


    /*
    Method that is called everytime timerRotation is greater or equal to 5f. It means that it's called every 5 seconds.
    It randomly select a new direction are rotates the drone accordingly before resetting timeRotation to 0.
    If the drone is moving along the z axis and has not yet inherited another drone's patrol zone, a rotation of 0 degree (i.e no rotation) has a lot more chance of being selected.
    */
    public void RotationDroneRandom()
    {
        List<float> rotationList;
        if((this.transform.forward == Vector3.forward || this.transform.forward == -1f * Vector3.forward) && !this.hasInheritedZone) //drone is on the z axis
        {
            rotationList = new List<float>{0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 90f, 180f, 270f}; //high probability of continuying forward (75%)
        }
        else
            rotationList = new List<float>{0f, 90f, 180f, 270f};

        int selectIndex = Random.Range(0, rotationList.Count);
        transform.Rotate(0, rotationList[selectIndex], 0);
        this.timerRotation = 0f;
    }
    

    void Update()
    {
        
        if(isPatrol)
        {
            if(this.timerRotation >= 2)
            {
                RotationDroneRandom();
            }
            
            OnBorder(transform.position.x, transform.position.z); //if a drone is close to the border it turns back and the previous loop is reset 
            transform.position += this.speed * transform.forward;
            Vector3 current_pos = transform.position;
            for(float i = -5f ; i <= 5f ; i=i+0.5f)
            {
                for(float j = -5f ; j <= 5f ; j=j+0.5f)
                {
                    Vector3 drawDown = transform.TransformDirection(new Vector3(i, this.transform.position.y * -1f, j));
                    Debug.DrawRay(current_pos, drawDown, Color.blue);
                }
            }
            DetectionLaser();
        }
        else if(hasDetectedPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, HQ.transform.position, 20f * Time.deltaTime);
            float dX = Mathf.Abs(this.transform.position.x - HQ.transform.position.x);
            float dZ = Mathf.Abs(this.transform.position.z - HQ.transform.position.z);
            if(!this.hasNotifiedSquad)
            {
                if(dX <= 0.1f && dZ <= 0.1f)
                {
                    AwakeRobots();
                    this.hasNotifiedSquad = true;
                    // while(Mathf.Abs(this.transform.position.y - this.ground.transform.localScale.y/2) >= 0.3f)
                    //     this.transform.position -= new Vector3(this.transform.position.x, this.transform.position.y - 0.5f, this.transform.position.z);
                }
            }
            if(!isMissionComplete)
            {
                foreach(var robot in this.robotsSquad)
                {
                    if(robot.hasKilledTarget)
                    {
                        isTargetDead = true; //so that a drone writes only once the message
                    }
                }
                if(isTargetDead)
                    {
                        this.DisplayToUI("My squad has successfully brought down the target.");
                        isMissionComplete = true;
                    }
            }
        }
        timerRotation += Time.deltaTime; //the timer is incremented every at every iteration of Update() 
    }


}