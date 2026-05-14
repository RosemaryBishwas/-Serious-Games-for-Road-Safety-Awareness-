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

    [Header("Accident Settings")]
    public float minimumAccidentSpeed = 0.1f;
    public Vector3 playerHitBoxCenter = new Vector3(0f, 0.9f, 1.6f);
    public Vector3 playerHitBoxHalfExtents = new Vector3(0.9f, 0.8f, 0.7f);
    public LayerMask playerHitLayers = ~0;

    private bool shouldStop = false;
    private bool hasHitPlayer = false;

    void Update()
    {
        CheckTrafficLight();

        if (!shouldStop)
        {
            MoveToWaypoint();
            CheckForPlayerHit();
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

    void OnCollisionEnter(Collision collision)
    {
        HandlePlayerHit(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        HandlePlayerHit(other.gameObject);
    }

    void HandlePlayerHit(GameObject hitObject)
    {
        if (hasHitPlayer || speed <= minimumAccidentSpeed)
        {
            return;
        }

        GameObject playerObject = GetPlayerObject(hitObject);
        if (playerObject == null)
        {
            return;
        }

        Debug.Log("Accident occurred with player!");

        hasHitPlayer = true;
        speed = 0f;

        PlayerAccident playerAccident = playerObject.GetComponent<PlayerAccident>();
        if (playerAccident == null)
        {
            playerAccident = playerObject.AddComponent<PlayerAccident>();
        }

        playerAccident.TriggerAccident(playerObject.transform.position - transform.position);
    }

    GameObject GetPlayerObject(GameObject hitObject)
    {
        if (hitObject.CompareTag("Player"))
        {
            return hitObject;
        }

        Transform root = hitObject.transform.root;
        return root.CompareTag("Player") ? root.gameObject : null;
    }

    void CheckForPlayerHit()
    {
        if (speed <= minimumAccidentSpeed || hasHitPlayer)
        {
            return;
        }

        Vector3 hitBoxCenter = transform.TransformPoint(playerHitBoxCenter);
        Collider[] hits = Physics.OverlapBox(hitBoxCenter, playerHitBoxHalfExtents, transform.rotation, playerHitLayers, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            HandlePlayerHit(hit.gameObject);
            if (hasHitPlayer)
            {
                return;
            }
        }
    }
}
