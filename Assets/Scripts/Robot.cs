using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public GameObject target; //GameObject of the player. Already in each robots' inspector
    public Player targetScript; //script of the target
    public bool isActive = false; //boolean that is true if the robot is active (awoken by its drone)
    public int baseDamage = 600; //damages calculations are based on this number
    public int layer_mask; //layer in which is the player
    public int layer_mask_wall; //layer in which are all the walls
    public float timer = 0f; //timer determining the frequency of the raycast shots
    public bool hasKilledTarget; //true if a robot make damages on the player while the latter is bellow 0HP


    /*
    Method that lets a robot throw a raycast with infinite range. It the way is clear between the robot and the player (no wall in between) it damages the player.
    The damages inflicted are inversely proportionnal to the distance between the robots and the player: the more the robots are close, the more damages they make.
    */
    void FireLaser(float distance)
    {
        Vector3 current_pos = transform.position;        
        var ray = new Ray(current_pos, target.transform.position - this.transform.position);
        RaycastHit hitTarget;
        RaycastHit hitWall;
        
        
        if (Physics.Raycast(ray, out hitTarget, Mathf.Infinity, layer_mask) && IsThereAWallInBetween(Physics.Raycast(ray, out hitWall, Mathf.Infinity, layer_mask_wall), hitWall, hitTarget, distance)) 
        {
            int totalDamage = (int) ( Mathf.Round( (float) this.baseDamage * (1f/distance) ) );
            this.targetScript.HP -= totalDamage;
            this.targetScript.SetHealth(this.targetScript.HP);
            print("Robot inflicted " + totalDamage.ToString() + " of damage.");
            if(targetScript.HP <= 0 && !targetScript.isPlayerDead)
            {
                this.targetScript.isPlayerDead = true; //this way only one squad can take credit of having killed the player
                this.hasKilledTarget = true;
                this.targetScript.speed = 0f;
            }
        }

        if(targetScript.HP <= 0)
        {
            this.isActive = false;
        }
        
    }

    public bool IsThereAWallInBetween(bool raycast, RaycastHit hitWall, RaycastHit hitTarget, float distTarget)
    {
        if(!raycast) //a wall has not been hit by the raycast so true is returned
            return true;
        //else the raycast got both the target and a wall so their distance to the robots is compared

        GameObject wall = hitWall.transform.gameObject;
        float distXWall = Mathf.Abs(wall.transform.position.x - this.transform.position.x);
        float distZWall = Mathf.Abs(wall.transform.position.z - this.transform.position.z);
        float distWall = Mathf.Sqrt(distZWall * distZWall + distXWall * distXWall);

        return distTarget < distWall ? true : false;
            
        
    }


    void Start()
    {
        this.hasKilledTarget = false;
        this.targetScript = target.GetComponent<Player>();
    }


    void Update()
    {
        if(this.isActive)
        {
            float dX = Mathf.Abs(this.transform.position.x - target.transform.position.x);
            float dZ = Mathf.Abs(this.transform.position.z - target.transform.position.z);
            float distance = Mathf.Round(Mathf.Sqrt(dX * dX + dZ * dZ)); //every iteration of Update() the distance between the robot and the player is updated
            this.transform.position = Vector3.MoveTowards(transform.position, this.target.transform.position, 3f * Time.deltaTime); //the robot moves towards the player in each iteration of Update()
            
            if(this.timer > 2)
            {
                FireLaser(distance);
                
                this.timer = 0;
            }
            else
            {
                timer += Time.deltaTime; //increments the timer
            }
            Debug.DrawRay(this.transform.position, target.transform.position - this.transform.position, Color.red);
        }
    }
}
