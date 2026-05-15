using UnityEngine;

public class Carcontroller : MonoBehaviour
{
    public float speed = 5f;

    public Transform stopPoint;
    public TrafficLightController trafficLight;

    public float stopDistance = 5f;

    [Header("Accident Settings")]
    public float minimumAccidentSpeed = 0.1f;
    public Vector3 playerHitBoxCenter = new Vector3(0f, 0.9f, 1.6f);
    public Vector3 playerHitBoxHalfExtents = new Vector3(0.9f, 0.8f, 0.7f);
    public LayerMask playerHitLayers = ~0;
    public bool onlyHitPlayerInFront = true;
    public bool useAutomaticPlayerHitScan = true;

    private bool shouldStop = false;
    private bool hasHitPlayer = false;
    private VehicleSoundController vehicleSounds;

    void Awake()
    {
        vehicleSounds = GetComponent<VehicleSoundController>();
        if (vehicleSounds == null)
        {
            vehicleSounds = gameObject.AddComponent<VehicleSoundController>();
        }
    }

    void Update()
    {
        CheckTrafficLight();
        vehicleSounds.UpdateVehicleSound(speed, !shouldStop && speed > minimumAccidentSpeed);

        if (!shouldStop)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            CheckForPlayerHit();
        }
    }

    void CheckTrafficLight()
    {
        float distance = Vector3.Distance(transform.position, stopPoint.position);

        if (distance < stopDistance)
        {
            if (trafficLight.currentState == TrafficLightController.LightState.Green ||
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
        HandlePlayerHit(collision.gameObject, false);
    }

    void OnTriggerEnter(Collider other)
    {
        HandlePlayerHit(other.gameObject, false);
    }

    void HandlePlayerHit(GameObject hitObject, bool requireAccidentZone)
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

        if (requireAccidentZone && !IsPlayerInAccidentZone(playerObject.transform.position))
        {
            return;
        }

        Debug.Log("Accident occurred with player!");

        hasHitPlayer = true;
        vehicleSounds.PlayCollisionSound();
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

    bool IsPlayerInAccidentZone(Vector3 playerPosition)
    {
        if (!onlyHitPlayerInFront)
        {
            return true;
        }

        Vector3 localPlayerPosition = transform.InverseTransformPoint(playerPosition);
        Vector3 safeHalfExtents = GetSafeHitBoxHalfExtents();
        float frontStart = playerHitBoxCenter.z - safeHalfExtents.z;
        float frontEnd = playerHitBoxCenter.z + safeHalfExtents.z;

        return localPlayerPosition.z >= frontStart &&
               localPlayerPosition.z <= frontEnd &&
               Mathf.Abs(localPlayerPosition.x - playerHitBoxCenter.x) <= safeHalfExtents.x &&
               Mathf.Abs(localPlayerPosition.y - playerHitBoxCenter.y) <= safeHalfExtents.y;
    }

    void CheckForPlayerHit()
    {
        if (speed <= minimumAccidentSpeed || hasHitPlayer)
        {
            return;
        }

        Vector3 hitBoxCenter = transform.TransformPoint(playerHitBoxCenter);
        Collider[] hits = Physics.OverlapBox(hitBoxCenter, GetSafeHitBoxHalfExtents(), transform.rotation, playerHitLayers, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            HandlePlayerHit(hit.gameObject, true);
            if (hasHitPlayer)
            {
                return;
            }
        }
    }

    Vector3 GetSafeHitBoxHalfExtents()
    {
        return new Vector3(
            Mathf.Min(playerHitBoxHalfExtents.x, 0.6f),
            Mathf.Min(playerHitBoxHalfExtents.y, 0.9f),
            Mathf.Min(playerHitBoxHalfExtents.z, 0.8f));
    }
}
