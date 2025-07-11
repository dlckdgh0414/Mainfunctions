using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySee : MonoBehaviour
{
    Vector3 moveDirection;
    public Transform[] waypoints;
    int currentWaypointIndex = 0;
    float moveSpeed = 2f;
    Vector3 direction;
    Vector3 prevPos;

    private void Awake()
    {
        prevPos = transform.position;
    }

    void Update()
    {
        EnemySeeR();
    }

    private void EnemySeeR()
    {
        Vector3 direction = transform.position - prevPos;
        prevPos = transform.position;

        if(direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle - 90);
        }

        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }
}
