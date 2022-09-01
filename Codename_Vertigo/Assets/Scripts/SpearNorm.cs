﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearNorm : MonoBehaviour
{
    Rigidbody2D rb;
    bool isFalling;
    public bool onWall;
    BoxCollider2D mainCollider;
    CircleCollider2D triggerArea;

    [SerializeField]
    Vector2 lastRightTransform;

    // Start is called before the first frame update
    void Start()
    {
        isFalling = true;
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<BoxCollider2D>();
        triggerArea = GetComponent<CircleCollider2D>();

        Invoke("EnableCollider", .1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isFalling)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            lastRightTransform = transform.right;
            Debug.Log(lastRightTransform.y + " is the y direction!");
            Debug.Log(lastRightTransform.x + " is the x direction!");
            
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //Used to check if the object hit implements the damage interface
        IDamageInterface objectToDamage = other.gameObject.GetComponent<IDamageInterface>();

        //If the object implements the damage interface AND it isn't the player character (Prevents the player from damaging themselves by throwing the spear at themselves)
        if (objectToDamage != null && !other.gameObject.GetComponent<PlayerController>())
        {
            //Damage the target
            objectToDamage.Damage(20f);
            //Briefly disable the collider if hitting an enemy so the enemy isn't continuously damaged
            if (other.gameObject.GetComponent<EnemyAIBase>())
            {
                DisableOnHit();
                Invoke("EnableCollider", .1f);
            }
            else
            {
                //If colliding with a wall that can be broken, for now, stick the spear to it (Will tweak so that the spear drops if the wall is destroyed)
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                rb.velocity = Vector2.zero;
                isFalling = false;
            }
            
        }
        //If the target is a wall
        else if (other.gameObject.GetComponent<Wall>())
        {
            //First, freeze rotation and movement constraints to prevent the spear from being moved by objects
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.velocity = Vector2.zero;
            //Set is falling to false, prevents angle adjustment
            isFalling = false;
            //Used to store the collided wall to get the corners/edges of the wall (Doesn't work right now, needs tweaking)
            Wall latchedWall = other.gameObject.GetComponent<Wall>();
            //Used to test the hit-stop system. Will remove at a later date
            FindObjectOfType<HitStopController>().StopTime(.2f);
            //Set the rotation + position of the spear based on where the spear made contact
            if(lastRightTransform.x > 0.7f && lastRightTransform.y < .7f)
            {

                Vector2 wallLeftCorner = latchedWall.GetCorner("UpperLeft");
                
                transform.rotation = Quaternion.identity;
                transform.position = new Vector2(transform.position.x + .1f, transform.position.y);


            } else if(lastRightTransform.x < -0.7f && lastRightTransform.y < .7f)
            {
                Debug.Log("FROM THE RIGHT");
                Vector2 wallRightCorner = latchedWall.GetCorner("UpperRight");
                transform.rotation = new Quaternion(0, 0, 180, 0);
                transform.position = new Vector2(transform.position.x - .1f, transform.position.y);
            } else if(lastRightTransform.y > .7f)
            {
                gameObject.layer = LayerMask.NameToLayer("WallClimbObj");
                transform.rotation = Quaternion.Euler(0, 0, 90);
                transform.position = new Vector2(transform.position.x, transform.position.y + .1f);
                gameObject.AddComponent<Wall>();
                
                
            }
            onWall = true;
        }
        else
        {
            if (!onWall)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                Debug.Log("No wall!");
                rb.velocity = Vector2.zero;
                isFalling = false;
                mainCollider.enabled = false;
                triggerArea.enabled = true;
            }
        }

    }

    public void ReturnSpear()
    {
        Destroy(this.gameObject);
    }

    public void DisableOnHit()
    {
        mainCollider.enabled = false;
    }

    public void EnableCollider()
    {
        mainCollider.enabled = true;
        
    }
}