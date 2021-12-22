using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Drone : MonoBehaviour
{
    public Robot[] allRobots;
    public List<Robot> robotsSquad;
    public GameObject target; //the player when they're detected
    public int attack;
    public int level;
    public int speed;
    private float range = 200f;
    int destroyable; 
    int layer_mask;
    int layer_mask_wall;
    public GameObject ground;
    public GameObject center;
    public Vector3 groundSize;
    public float timerRotation;
    public GameObject HQ;
    public bool isPatrol;
    public bool hasDetectedPlayer;
    public bool hasInheritedZone;
    public RaycastHit[] hits;
    public float zoneMinX;
    public float zoneMaxX;
    public float zoneMinZ;
    public float zoneMaxZ;
    public List<string> tags;
    public List<Drone> drones = new List<Drone>();
    public Text log; //where the last communication is displayed
    public Text screenChange;
    public bool hasNotifiedSquad;
    public bool isMissionComplete;
    //public CanvasGroup visibility;


    void DetectionLaser()
    {
        Vector3 current_pos = transform.position;

        for(float i = -5f ; i <= 5f ; i=i+0.5f)
            {
                for(float j = -5f ; j <= 5f ; j=j+0.5f)
                {
                                //Vector3 down = transform.TransformDirection(range * Vector3.down);
                    var ray = new Ray(current_pos, transform.TransformDirection(new Vector3(i, this.transform.position.y * -1f, j)));
                    
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, range, layer_mask)) 
                    {
                        this.target = hit.transform.gameObject;
                        Player playerScript = target.GetComponent<Player>();
                        this.isPatrol = false;
                        if(!hasDetectedPlayer) //this way IntelligentRepartition is called only once
                            IntelligentRepartition();
                        if(!playerScript.hasBeenDetected)
                        {
                            playerScript.hasBeenDetected = true;
                            // playerScript.isCameraOnPlayer = false;
                            // playerScript.mainCamera.transform.position = new Vector3(0f, 87f, 0f);
                            // playerScript.mainCamera.transform.localRotation = Quaternion.Euler(90, 0f, 0f);
                            // playerScript.speed /= 2.5f;
                            // playerScript.isKeyWPressed = false;
                            // playerScript.isKeyLeftPressed = false;
                            // playerScript.isKeyRightPressed = false;
                            // playerScript.isKeyDownPressed = false;
                            // playerScript.isKeyUpPressed = false;
                            this.DisplayToUI("You've been detected: you can now switch the view by pressing 'C'.", false);
                        }
                        this.hasDetectedPlayer = true;
                    }
                }
            }
        

    }

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


    public void DisplayToUI(string message, bool isDroneTalking)
    {
        if(isDroneTalking)
            this.log.text += "[" + Time.time.ToString("N2") + "] - " + this.tag + ": " + message + "\n\n";
        else
        {
            this.screenChange.text = message + "\n\n";
        }   
    }


    // Start is called before the first frame update
    void Start()
    {
        // this.visibility = GetComponent<CanvasGroup>();
        // this.visibility.alpha = 0;
        // this.visibility.interactable = false;
        // this.visibility.blocksRaycasts = false;
        this.hasNotifiedSquad = false;
        this.hasDetectedPlayer = false;
        this.isPatrol = true;
        this.hasInheritedZone = false;
        this.isMissionComplete = false;
        this.allRobots = FindObjectsOfType(typeof(Robot)) as Robot[];
        foreach (var robot in allRobots)
        {
            if(robot.gameObject.tag == this.tag)
                robotsSquad.Add(robot);
        }
        this.HQ = GameObject.Find(WhichHQ());
        this.timerRotation = 0f;
        this.ground = GameObject.Find("Ground");
        this.center = GameObject.Find("Center");
        this.groundSize = ground.GetComponent<Renderer>().bounds.size;
        this.destroyable = 8;
        this.layer_mask = 1 << destroyable; 
        this.layer_mask_wall = 1 << 7;

        this.tags = new List<string>{"Drone1", "Drone2", "Drone3", "Drone4"};
        foreach(var droneTag in this.tags)
        {
            this.drones.Add(GameObject.Find(droneTag).GetComponent<Drone>());
        }
        float valueMin;
        float valueMax;
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
                print("wtf bro");
                break;

        }
        this.zoneMinX = valueMin * (float) (ground.transform.localScale.x / 2f);
        this.zoneMaxX = valueMax * (float) (ground.transform.localScale.x / 2f);
        this.zoneMinZ = -1f * (float) (ground.transform.localScale.x / 2f);
        this.zoneMaxZ = (float) (ground.transform.localScale.x) / 2f;
        
    }


    public void IntelligentRepartition()
    {
        string buffer1 = "Target has been detected. I have to lead my robots squad. ";
        string buffer2;
        Drone inheritor = null;
        if(this.tag == "Drone1")
        {
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
                buffer2 = "No drone found to inherit my patrol zone.";    
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
            DisplayToUI(buffer1 + buffer2, true);
            foreach(var drone in this.drones)
                drone.tags.Remove(this.tag);

            if(inheritor != null)
                inheritor.DisplayToUI("Roger.", true);
        }
    }


    public void AwakeRobots()
    {
        foreach (var robot in robotsSquad)
        {
            robot.layer_mask = this.layer_mask;
            robot.layer_mask_wall = this.layer_mask_wall;
            robot.isActive = true;
        }
    }

    public void OnBorder(float posX, float posZ)
    {
        float diffXMin = Mathf.Abs(posX - this.zoneMinX);
        float diffXMax = Mathf.Abs(this.zoneMaxX - posX);
        float diffZ = Mathf.Abs(Mathf.Abs(posZ) - groundSize.z/2f); 
        if(diffXMin < 1f || diffXMax < 1f || diffZ < 1f)
        {    
            transform.Rotate(0, 180, 0);
            //this.timePreviousRotation = (int) Mathf.Round(Time.time);
        }
    }

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
    // Update is called once per frame
    void Update()
    {
        bool isTargetDead = false;
        if(isPatrol)
        {
            if(this.timerRotation >= 2)
            {
                RotationDroneRandom();
            }
            
            OnBorder(transform.position.x, transform.position.z); //if a drone is close to the border it turns back and the previous loop is reset 
            transform.position += 0.30f * transform.forward;
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
                        isTargetDead = true; //in order to write only once the message
                    }
                }
                if(isTargetDead)
                    {
                        this.DisplayToUI("My squad has successfully brought down the target.", true);
                        isMissionComplete = true;
                    }
            }
        }
        timerRotation += Time.deltaTime;
    }


}