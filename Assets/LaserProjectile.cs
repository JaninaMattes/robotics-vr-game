﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class LaserProjectile : MonoBehaviour
{

    Rigidbody rb;
    [HideInInspector]
    public float speed=1;
    [HideInInspector]
    public GameObject explosionPrefab;

    public float timeTillAutoDestruction=10;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.AddRelativeForce(Vector3.up * speed);
        StartCoroutine("WaitForDestruction");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Scanner" && other.tag != "Munition" && other.tag != "SonarSensor_2" && other.tag != "LidarSensor" && other.tag != "CameraSensor" && other.tag != "RadarSensor")
        {
            if (other.GetComponent<EnemyHit>())
            {
                other.GetComponent<EnemyHit>().Hit(true);
            }
            if (other.GetComponent<ChangeSceneOnHit>())
            {
                other.GetComponent<ChangeSceneOnHit>().InvokeLoadScene();
            }
            if (other.GetComponent<StartGameOnHit>())
            {
                other.GetComponent<StartGameOnHit>().StartGameAndScore();
            }
            Hit();
            Debug.Log("Hit Object: " + other.name + " with Tag: "+ other.tag);
        }
    }

    void Hit()
    {
        //Debug.Log("Boom!");
        Instantiate(explosionPrefab, gameObject.transform.position-rb.velocity.normalized*0.5f, Quaternion.identity);
      
        Destroy(gameObject);
    }

    IEnumerator WaitForDestruction()
    {
        float counter = 0;
        while (counter < timeTillAutoDestruction)
        {
            //Debug.Log(counter);

            counter += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Hit();
        //Destroy(gameObject);
    }


}
