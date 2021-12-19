using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public GameObject target;
    public Player targetScript;
    public bool isActive = false;
    public int baseDamage = 600;
    public int layer_mask;
    public int layer_mask_wall;
    public float timer = 0f;

    void FireLaser(float distance)
    {
        //transform.TransformDirection(new Vector3(-0.6f, 0f, 0.4f))
        Vector3 current_pos = transform.position;        
        var ray = new Ray(current_pos, target.transform.position - this.transform.position);
        RaycastHit hitTarget;
        RaycastHit hitWall;
        
        // if (Physics.Raycast(ray, out hitTarget, Mathf.Infinity, layer_mask) && Physics.Raycast(ray, out hitWall, Mathf.Infinity, layer_mask_wall)) 
        // {
        //     float dX1 = Mathf.Abs(this.transform.position.x - hitWall.transform.position.x);
        //     float dZ1 = Mathf.Abs(this.transform.position.z - hitWall.transform.position.z);
        //     int distanceToWall = (int) Mathf.Round(Mathf.Sqrt(dX1 * dX1 + dZ1 * dZ1));
        //     float dX2 = Mathf.Abs(this.transform.position.x - hitTarget.transform.position.x);
        //     float dZ2 = Mathf.Abs(this.transform.position.z - hitTarget.transform.position.z);
        //     int distanceToTarget = (int) Mathf.Round(Mathf.Sqrt(dX2 * dX2 + dZ2 * dZ2));
        //     int timeToDestroyWall = (int) Mathf.Round(Time.time);
        //     if(distanceToWall < distanceToTarget)
        //     {
        //         float timer = 0;
        //         if(timer > 5)
        //             Destroy(hitWall.transform.gameObject);
        //         else
        //         {
        //             timer += Time.deltaTime;
        //             print("waiting");
        //         }
        //     }
            
        // }
        
        if (Physics.Raycast(ray, out hitTarget, Mathf.Infinity, layer_mask) && IsThereAWallBetween(Physics.Raycast(ray, out hitWall, Mathf.Infinity, layer_mask_wall), hitWall, hitTarget)) 
        {
            int totalDamage = (int) Mathf.Round((float) this.baseDamage * (1f/distance));
            this.targetScript.HP -= totalDamage;
            print("Robot inflicted " + totalDamage.ToString() + " of damage.");
        }

        if(targetScript.HP <= 0)
        {
            this.isActive = false;
        }
        
    }

    public bool IsThereAWallBetween(bool raycast, RaycastHit hitWall, RaycastHit hitTarget)
    {
        if(!raycast)
            return true;
        //else the raycast got both the target and a wall

        GameObject wall = hitWall.transform.gameObject;
        float distXWall = Mathf.Abs(wall.transform.position.x - this.transform.position.x);
        float distXTarget = Mathf.Abs(target.transform.position.x - this.transform.position.x);
        return distXTarget < distXWall ? true : false;
            
        
    }

    // Start is called before the first frame update
    void Start()
    {
        targetScript = target.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.isActive)
        {
            float dX = Mathf.Abs(this.transform.position.x - target.transform.position.x);
            float dZ = Mathf.Abs(this.transform.position.z - target.transform.position.z);
            int distance = (int) Mathf.Round(Mathf.Sqrt(dX * dX + dZ * dZ));
            this.transform.position = Vector3.MoveTowards(transform.position, this.target.transform.position, 3f * Time.deltaTime);
            var lookRotation = Quaternion.LookRotation (new Vector3(target.transform.position.x, 0, target.transform.position.z));
            this.transform.rotation = Quaternion.Slerp (this.transform.rotation, lookRotation, 3f * Time.deltaTime);
            
            //print(distance);
            if(this.timer > 2)
            {
                FireLaser(distance);
                
                this.timer = 0;
                // Destroy(hitWall.transform.gameObject);
            }
            else
            {
                timer += Time.deltaTime;
                //print("waiting");
            }
            Debug.DrawRay(this.transform.position, target.transform.position - this.transform.position, Color.red);
            
           // FireLaser(distance);
        }
    }
}
