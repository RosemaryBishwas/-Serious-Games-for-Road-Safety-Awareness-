using UnityEngine;

public class VehicleSoundController : MonoBehaviour
{
    [Header("Engine")]
    public bool enableVehicleLoop = true;
    public float maxExpectedSpeed = 8f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.35f;

    [Header("Horn")]
    public bool enableAutomaticHorn = true;
    public string playerTag = "Player";
    public float hornDistance = 7f;
    public float hornCooldown = 3f;
    public Vector3 hornCheckCenter = new Vector3(0f, 1f, 3f);
    public Vector3 hornCheckHalfExtents = new Vector3(1.2f, 1f, 3f);
    public LayerMask hornLayers = ~0;

    private AudioSource engineSource;
    private float nextHornTime;

    private void Awake()
    {
        engineSource = GetComponent<AudioSource>();
        if (engineSource == null)
        {
            engineSource = gameObject.AddComponent<AudioSource>();
        }

        engineSource.playOnAwake = false;
        engineSource.loop = true;
        engineSource.spatialBlend = 1f;
        engineSource.minDistance = 3f;
        engineSource.maxDistance = 45f;
        engineSource.rolloffMode = AudioRolloffMode.Linear;
    }

    public void UpdateVehicleSound(float currentSpeed, bool isMoving)
    {
        UpdateEngineLoop(currentSpeed, isMoving);

        if (isMoving)
        {
            TryPlayHornForNearbyPlayer();
        }
    }

    public void PlayCollisionSound()
    {
        RoadSafetyAudio.PlayCollision(transform.position);
        StopEngineLoop();
    }

    private void UpdateEngineLoop(float currentSpeed, bool isMoving)
    {
        if (!enableVehicleLoop || engineSource == null)
        {
            return;
        }

        if (!isMoving || currentSpeed <= 0.05f)
        {
            StopEngineLoop();
            return;
        }

        if (engineSource.clip == null)
        {
            engineSource.clip = RoadSafetyAudio.VehicleLoopClip;
        }

        float speedAmount = Mathf.Clamp01(currentSpeed / Mathf.Max(0.01f, maxExpectedSpeed));
        engineSource.volume = RoadSafetyAudio.VehicleVolume * Mathf.Lerp(0.45f, 1f, speedAmount);
        engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedAmount);

        if (!engineSource.isPlaying)
        {
            engineSource.Play();
        }
    }

    private void StopEngineLoop()
    {
        if (engineSource != null && engineSource.isPlaying)
        {
            engineSource.Stop();
        }
    }

    private void TryPlayHornForNearbyPlayer()
    {
        if (!enableAutomaticHorn || Time.time < nextHornTime)
        {
            return;
        }

        Vector3 checkCenter = transform.TransformPoint(hornCheckCenter);
        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            hornCheckHalfExtents,
            transform.rotation,
            hornLayers,
            QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (!IsPlayer(hit.gameObject))
            {
                continue;
            }

            if (Vector3.Distance(transform.position, hit.transform.position) <= hornDistance)
            {
                RoadSafetyAudio.PlayHorn(transform.position);
                nextHornTime = Time.time + hornCooldown;
                return;
            }
        }
    }

    private bool IsPlayer(GameObject hitObject)
    {
        if (hitObject.CompareTag(playerTag))
        {
            return true;
        }

        Transform root = hitObject.transform.root;
        return root != null && root.CompareTag(playerTag);
    }
}
