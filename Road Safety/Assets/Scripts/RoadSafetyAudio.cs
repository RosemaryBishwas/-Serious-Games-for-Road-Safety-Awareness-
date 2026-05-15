using UnityEngine;

public class RoadSafetyAudio : MonoBehaviour
{
    private const int SampleRate = 44100;

    private static RoadSafetyAudio instance;
    private static AudioClip generatedHornClip;
    private static AudioClip generatedCollisionClip;
    private static AudioClip generatedSuccessClip;
    private static AudioClip generatedVehicleLoopClip;

    [Header("Optional Custom Clips")]
    public AudioClip hornClip;
    public AudioClip collisionClip;
    public AudioClip successClip;
    public AudioClip vehicleLoopClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float hornVolume = 0.9f;
    [Range(0f, 1f)] public float collisionVolume = 1f;
    [Range(0f, 1f)] public float successVolume = 0.9f;
    [Range(0f, 1f)] public float vehicleVolume = 0.35f;

    public static RoadSafetyAudio Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<RoadSafetyAudio>();
            }

            if (instance == null)
            {
                GameObject audioObject = new GameObject("Road Safety Audio");
                instance = audioObject.AddComponent<RoadSafetyAudio>();
                DontDestroyOnLoad(audioObject);
            }

            return instance;
        }
    }

    public static AudioClip VehicleLoopClip
    {
        get { return Instance.GetVehicleLoopClip(); }
    }

    public static float VehicleVolume
    {
        get { return Instance.vehicleVolume; }
    }

    public static void PlayHorn(Vector3 position)
    {
        RoadSafetyAudio audio = Instance;
        audio.PlayOneShotAt(position, audio.GetHornClip(), audio.hornVolume);
    }

    public static void PlayCollision(Vector3 position)
    {
        RoadSafetyAudio audio = Instance;
        audio.PlayOneShotAt(position, audio.GetCollisionClip(), audio.collisionVolume);
    }

    public static void PlaySuccess(Vector3 position)
    {
        RoadSafetyAudio audio = Instance;
        audio.PlayOneShotAt(position, audio.GetSuccessClip(), audio.successVolume);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private AudioClip GetHornClip()
    {
        if (hornClip != null)
        {
            return hornClip;
        }

        if (generatedHornClip == null)
        {
            generatedHornClip = CreateHornClip();
        }

        return generatedHornClip;
    }

    private AudioClip GetCollisionClip()
    {
        if (collisionClip != null)
        {
            return collisionClip;
        }

        if (generatedCollisionClip == null)
        {
            generatedCollisionClip = CreateCollisionClip();
        }

        return generatedCollisionClip;
    }

    private AudioClip GetSuccessClip()
    {
        if (successClip != null)
        {
            return successClip;
        }

        if (generatedSuccessClip == null)
        {
            generatedSuccessClip = CreateSuccessClip();
        }

        return generatedSuccessClip;
    }

    private AudioClip GetVehicleLoopClip()
    {
        if (vehicleLoopClip != null)
        {
            return vehicleLoopClip;
        }

        if (generatedVehicleLoopClip == null)
        {
            generatedVehicleLoopClip = CreateVehicleLoopClip();
        }

        return generatedVehicleLoopClip;
    }

    private void PlayOneShotAt(Vector3 position, AudioClip clip, float volume)
    {
        if (clip == null || volume <= 0f)
        {
            return;
        }

        GameObject soundObject = new GameObject("Road Safety One Shot Audio");
        soundObject.transform.position = position;

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.minDistance = 2f;
        source.maxDistance = 35f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.Play();

        Destroy(soundObject, clip.length + 0.25f);
    }

    private static AudioClip CreateHornClip()
    {
        float duration = 0.55f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            float envelope = Mathf.Clamp01(time / 0.04f) * Mathf.Clamp01((duration - time) / 0.08f);
            float toneA = Mathf.Sin(2f * Mathf.PI * 440f * time);
            float toneB = Mathf.Sin(2f * Mathf.PI * 530f * time);
            samples[i] = (toneA * 0.6f + toneB * 0.4f) * envelope * 0.45f;
        }

        return CreateClip("Generated Horn", samples);
    }

    private static AudioClip CreateCollisionClip()
    {
        float duration = 0.45f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        uint random = 123456789u;

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            float envelope = Mathf.Exp(-time * 9f);
            random = random * 1664525u + 1013904223u;
            float noise = ((random >> 16) / 32768f) - 1f;
            float thump = Mathf.Sin(2f * Mathf.PI * 85f * time) * Mathf.Exp(-time * 13f);
            samples[i] = Mathf.Clamp((noise * 0.5f + thump) * envelope, -1f, 1f) * 0.75f;
        }

        return CreateClip("Generated Collision", samples);
    }

    private static AudioClip CreateSuccessClip()
    {
        float duration = 0.9f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float[] notes = { 523.25f, 659.25f, 783.99f };
        float noteDuration = duration / notes.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            int noteIndex = Mathf.Min(Mathf.FloorToInt(time / noteDuration), notes.Length - 1);
            float noteTime = time - noteIndex * noteDuration;
            float envelope = Mathf.Clamp01(noteTime / 0.03f) * Mathf.Clamp01((noteDuration - noteTime) / 0.08f);
            float tone = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * time);
            samples[i] = tone * envelope * 0.45f;
        }

        return CreateClip("Generated Success", samples);
    }

    private static AudioClip CreateVehicleLoopClip()
    {
        float duration = 1f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            float engine = Mathf.Sin(2f * Mathf.PI * 95f * time) * 0.55f;
            float rumble = Mathf.Sin(2f * Mathf.PI * 47f * time) * 0.3f;
            float texture = Mathf.Sin(2f * Mathf.PI * 190f * time) * 0.15f;
            samples[i] = (engine + rumble + texture) * 0.25f;
        }

        return CreateClip("Generated Vehicle Loop", samples);
    }

    private static AudioClip CreateClip(string clipName, float[] samples)
    {
        AudioClip clip = AudioClip.Create(clipName, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
