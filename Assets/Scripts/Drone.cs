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
    public Vector3 HQ; //headquarter of the squad where all the robots from the drone's squad are. The drone has to stay there to control its squad
    public bool isPatrol; //boolean that is true if the drone is searching the player and false otherwise
    public bool hasDetectedPlayer; //boolean that is set to true when the drone's detection raycasts have found the player
    public bool hasInheritedZone; //boolean that is set to true only when the drone's patrol zone has been changed by another drone
    public float zoneMinX; //minimum value the drone can take on the x axis
    public float zoneMaxX; //maximum value the drone can take on the x axis
    public float zoneMinZ; //minimum value the drone can take on the z axis
    public float zoneMaxZ; //maximum value the drone can take on the z axis
    public List<string> tags = new List<string>(); //list that contains all the tags of the drones
    public List<Drone> drones = new List<Drone>(); //list that contains all the drones (including the drone the script is attached to)
    public Text log; //where all the communications are displayed
    public Text screenChange; //notification that the player can change the view
    public bool hasNotifiedSquad; //boolean that is true when a drone a detected the player and notified another drone (if there are any that is still patrolling)
    public bool isMissionComplete; //boolean that is true when a robot from the drone's squad has brought down the player
    public bool isTargetDead;
    public bool toFixPosition;
    public Vector3 lastPosition;
    public int nbIterationsToRecoverPosition;
    public float timerRecover;
    public bool isAligned;


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
                            AdvancedRepartition(); //the drone

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
    Method that lets the drone write its communications on the log panel. 
    The second parameter, by default 2, is the number of lines to be skipped after the message is sent on log.
    */
    public void DisplayToUI(string message, int jumpLines = 2)
    {
        string buffer = "";
        for(int i = 0 ; i < jumpLines ; i++)
            buffer += "\n";
        
        this.log.text += "[" + Time.timeSinceLevelLoad.ToString("N2") + "] - " + this.tag + ": " + message + buffer;
    }


    void Start()
    {
        this.isAligned = false;
        this.timerRecover = 0f;
        this.nbIterationsToRecoverPosition = 0;
        this.lastPosition = this.transform.position;
        this.toFixPosition = false;
        this.isTargetDead = false;
        this.screenChange.gameObject.SetActive(false); //the notification that the view can be changed is not visible since the player isn't detected yet 
        this.hasNotifiedSquad = false;
        this.hasDetectedPlayer = false;
        this.isPatrol = true;
        this.hasInheritedZone = false;
        this.isMissionComplete = false;
        this.allRobots = FindObjectsOfType(typeof(Robot)) as Robot[];
        this.speed = 20f;
        float posXRobotsSum = 0f;
        float posZRobotsSum = 0f;
        foreach (Robot robot in FindObjectsOfType(typeof(Robot)) as Robot[])
        {
            if(robot.tag == this.tag)
            {
                this.robotsSquad.Add(robot); //only the robots from the drone's squad are added in the list
                posXRobotsSum += robot.gameObject.transform.position.x;
                posZRobotsSum += robot.gameObject.transform.position.z;
            }
        }
        this.HQ = new Vector3((posXRobotsSum / this.robotsSquad.Count), this.gameObject.transform.position.y, (posZRobotsSum / this.robotsSquad.Count)); //the coordinates of the headquarters are just the average of the squad coordinates
        
        this.timerRotation = 0f;
        this.ground = GameObject.Find("Ground");
        this.groundSize = ground.transform.localScale;
        this.detection = 8; //since the player is the layer 8
        this.layer_mask = 1 << detection; //the mask is determined this way
        this.layer_mask_wall = 1 << 7; //this mask is made for the robots squad in order to detect the walls which are in the layer 7

        foreach(Drone drone in FindObjectsOfType(typeof(Drone)) as Drone[])
        {
            if(!this.drones.Contains(drone) && !this.tags.Contains(drone.tag))
            {
                this.tags.Add(drone.tag);
                this.drones.Add(drone);
            }
        }


        InitZoneLimit();

        foreach(Drone drone in this.drones)
        {
            drone.transform.position = new Vector3(Random.Range(drone.zoneMinX + 1f, drone.zoneMaxX - 1f), drone.gameObject.transform.position.y, Random.Range(drone.zoneMinZ + 1, drone.zoneMaxZ - 1f));
        }
        
    }


    /*
    Method that is called when the different zones of the map have to be shared equally among the drones currently patrolling.
    */
    public void InitZoneLimit()
    {
        float size = - 1f * ground.transform.localScale.x/2;
        float cpt = ground.transform.localScale.x / this.drones.Count; //it will be the increment for the borders 
        for(int i = 0 ; i < this.drones.Count ; i++) //just some C nostalgy
        {
            this.drones[i].zoneMinX = size;
            size += cpt;
            this.drones[i].zoneMaxX = size; //this way the size of the border (on the x axis) for each drone is ground.tranform.localScale.x / this.drones.Count 
            this.drones[i].zoneMinZ = -1f * ground.transform.localScale.z / 2f;
            this.drones[i].zoneMaxZ = ground.transform.localScale.z / 2f;
            if(this.drones[i].transform.position.x < this.drones[i].zoneMinX || this.drones[i].transform.position.x > this.drones[i].zoneMaxX) // something went wrong, specifically a drone was already near its own bounds when InitZoneLimit() was called
            {
                this.drones[i].toFixPosition = true; //something went wrong, specifically a drone was already near its own bounds when InitZoneLimit() was called
                this.drones[i].isPatrol = false; //temporarily
            }
        }

    }


    /*
    Method that is called when a drone has detected the player. It then actualizes the lists, calls InitZoneLimit() and make the drones communicate.
    */
    public void AdvancedRepartition()
    {
        string buffer1 = "Target has been detected. I have to lead my robots squad.";
        string buffer2 = "";
        if(this.drones.Count == 1)
            buffer2 = " Whiskey Tango Foxtrot am I the only one patrolling?";
        List<string> droneResponse = new List<string>{"Roger.", "Copy that.", "Roger that.", "Transmission received.", "Got it.", "We got you.", "Then we will share your patrol zone."}; //to bring more variety
    
        if(!hasNotifiedSquad) //without this boolean sometimes a drone can call the method twice in a row, thus spamming the log panel
        {
            DisplayToUI(buffer1 + buffer2);
            int cpt = 0;
            foreach(var drone in this.drones)
            {
                if(drone.tag == this.tag)
                    continue;
                drone.tags.Remove(this.tag);
                drone.drones.Remove(this);

                string response = droneResponse[Random.Range(0, droneResponse.Count)];
                if(cpt == this.drones.Count - 2) //since cpt in not incremented when it's the drone which is giving its patrol zone, it will be this.drones.Count - 2 for the last drone about to send its response
                    drone.DisplayToUI(response); //the last drone that answers will skip 2 lines instead of 1 in the logPanel
                else
                    drone.DisplayToUI(response, 1);

                droneResponse.Remove(response); //so that each drone gives a different answer.
                cpt++;
            }
            this.drones.Remove(this);
            this.InitZoneLimit();

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
    If it is, the drone does a rotation in a random direction.
    */
    public void OnBorder(float posX, float posZ)
    {

        posX = Mathf.Clamp(posX, this.zoneMinX + 3f, this.zoneMaxX - 3f);
        posZ = Mathf.Clamp(posZ, this.zoneMinZ + 3f, this.zoneMaxZ - 3f); 
        if((posX != this.transform.position.x || posZ != this.transform.position.z) && !this.toFixPosition)
        {
            this.transform.position = new Vector3(posX, this.transform.position.y, posZ); //if a drone was within its bounds the position stays the same and if it was outside its zone its position if modified 
            RotationDroneRandom(false); //25% of continuying forward (in which case RotationDroneRandom will be called again) instead of 50%
        }

    }


    /*
    Method that is called everytime timerRotation is greater or equal to 5f. It means that it's called every 5 seconds.
    It randomly select a new direction are rotates the drone accordingly before resetting timeRotation to 0.
    If the drone is moving along the z axis and has not yet inherited another drone's patrol zone, a rotation of 0 degree (i.e no rotation) has a more chance of being selected.
    The parameter should be true if the method was called in Update() when the timer reaches 2 secondes and false if it's called by OnBorder().
    */
    public void RotationDroneRandom(bool isCalledByTimer)
    {
        List<float> rotationList;
        if(((this.transform.forward == Vector3.forward || this.transform.forward == -1f * Vector3.forward) && isCalledByTimer)) //drone is on the z axis
        {
            rotationList = new List<float>{0f, 0f, 0f, 90f, 180f, 270f}; //there's a good chance of continuying forward (50%)
        }
        else
            rotationList = new List<float>{0f, 90f, 180f, 270f};

        int selectIndex = Random.Range(0, rotationList.Count);
        transform.Rotate(0, rotationList[selectIndex], 0);
        this.timerRotation = 0f;
    }
    

    void Update()
    {

        if(this.toFixPosition) //InitLimitZone made a mistake (the drone was near one of its bounds on the x axis just before the limit bounds changed)
        {

            if(this.transform.position.x < this.zoneMinX || this.transform.position.x > this.zoneMaxX)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(0.5f * (this.zoneMinX + this.zoneMaxX), this.transform.position.y, this.transform.position.z), 20f * Time.deltaTime);
            }
                        
            else
            {
                this.isPatrol = true;
                this.toFixPosition = false;
            }
        }
        if(isPatrol)
        {
            if(this.timerRotation >= 2)
            {
                RotationDroneRandom(true);
            }
            
            OnBorder(transform.position.x, transform.position.z); //if a drone is close to the border it turns back and the previous loop is reset 
            this.transform.position += this.transform.forward * this.speed * Time.deltaTime;
            
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
            transform.position = Vector3.MoveTowards(transform.position, this.HQ, 20f * Time.deltaTime);
            float dX = Mathf.Abs(this.transform.position.x - HQ.x);
            float dZ = Mathf.Abs(this.transform.position.z - HQ.z);
            if(!this.hasNotifiedSquad)
            {
                if(dX <= 0.1f && dZ <= 0.1f)
                {
                    AwakeRobots();
                    this.hasNotifiedSquad = true;
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
                        this.DisplayToUI("My squad has brought down the target with flying colors.");
                        isMissionComplete = true;
                    }
            }
        }

        timerRotation += Time.deltaTime; //the timer is incremented every at every iteration of Update() 
    }


}