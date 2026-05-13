using UnityEngine;

public class WaypointCar : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    public float speed = 5f;
    public float rotationSpeed = 5f;

    private int currentWaypointIndex = 0;

    [Header("Traffic Light Settings")]
    public TrafficLightController trafficLight;
    public Transform stopPoint;
    public float stopDistance = 5f;

    private bool shouldStop = false;

    void Update()
    {
        CheckTrafficLight();

        if (!shouldStop)
        {
            MoveToWaypoint();
        }
    }

    void MoveToWaypoint()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];

        // Direction
        Vector3 direction = (target.position - transform.position).normalized;

        // Move
        transform.position += direction * speed * Time.deltaTime;

        // Smooth rotation
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            currentWaypointIndex++;

            // Loop path
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    void CheckTrafficLight()
    {
        if (trafficLight == null || stopPoint == null) return;

        float distance = Vector3.Distance(transform.position, stopPoint.position);

        if (distance < stopDistance)
        {
            if (trafficLight.currentState == TrafficLightController.LightState.Red ||
                trafficLight.currentState == TrafficLightController.LightState.Yellow)
            {
                shouldStop = true;
            }
            else
            {
                shouldStop = false;
            }
        }
        else
        {
            shouldStop = false;
        }
    }
}