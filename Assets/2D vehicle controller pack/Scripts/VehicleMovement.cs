using System.Collections;
using UnityEngine;

public class VehicleMovement : MonoBehaviour
{
    [Header("Chassis & Wheels")]
    public Rigidbody2D chassisRb;
    public WheelJoint2D frontWheelJoint;
    public WheelJoint2D rearWheelJoint;

    [Header("Settings")]
    public float motorSpeed = 1500f;
    public float reverseSpeed = 1000f;
    public float tiltTorque = 150f;

    // ---------------- ENGINE AUDIO ----------------
    [Header("Engine Audio - Clips")]
    [Tooltip("Plays when W is NOT held")]
    public AudioClip engineIdleClip;
    [Tooltip("Plays when W IS held")]
    public AudioClip engineDriveClip;

    [Header("Engine Audio - Volumes")]
    [Range(0f, 1f)] public float idleVolume = 0.45f;
    [Range(0f, 1f)] public float driveVolume = 0.6f;

    [Header("Engine Audio - Pitch (center + jitter)")]
    public float baseIdlePitch = 1.0f;
    public float baseDrivePitch = 1.0f;
    [Range(0f, 0.5f)] public float idlePitchJitter = 0.05f;
    [Range(0f, 0.5f)] public float drivePitchJitter = 0.08f;

    [Header("Engine Audio - Fades")]
    [Tooltip("Seconds for fade-in when a layer becomes active")]
    public float fadeInDuration = 0.25f;
    [Tooltip("Seconds for fade-out when a layer deactivates")]
    public float fadeOutDuration = 0.25f;

    [Header("Engine Audio - Offsets (skip clicky parts)")]
    [Tooltip("Start playback from this offset (seconds) for idle loop")]
    public float idleStartOffset = 0.0f;
    [Tooltip("Start playback from this offset (seconds) for drive loop")]
    public float driveStartOffset = 0.0f;
    [Tooltip("Stop/warp this many seconds BEFORE the end (seconds)")]
    public float idleEndOffset = 0.0f;
    [Tooltip("Stop/warp this many seconds BEFORE the end (seconds)")]
    public float driveEndOffset = 0.0f;

    private AudioSource idleSrc;
    private AudioSource driveSrc;
    private Coroutine idleFadeCo;
    private Coroutine driveFadeCo;

    private JointMotor2D motor;

    // ---------------- UNITY LIFECYCLE ----------------
    void Start()
    {
        // Motor defaults
        motor.maxMotorTorque = 100000000f;
        motor.motorSpeed = 0f;

        // Create audio sources
        if (engineIdleClip != null)
        {
            idleSrc = gameObject.AddComponent<AudioSource>();
            idleSrc.clip = engineIdleClip;
            idleSrc.loop = true;
            idleSrc.playOnAwake = false;
            idleSrc.volume = 0f; // will be faded in
        }

        if (engineDriveClip != null)
        {
            driveSrc = gameObject.AddComponent<AudioSource>();
            driveSrc.clip = engineDriveClip;
            driveSrc.loop = true;
            driveSrc.playOnAwake = false;
            driveSrc.volume = 0f; // will be faded in
        }
    }

    void Update()
    {
        HandleMotor();
        HandleTilt();
        MaintainLoopTrim(idleSrc, idleStartOffset, idleEndOffset);
        MaintainLoopTrim(driveSrc, driveStartOffset, driveEndOffset);
    }

    // ---------------- MOTOR / INPUT ----------------
    private void HandleMotor()
    {
        bool accelerate = Input.GetKey(KeyCode.W);
        bool reverse = Input.GetKey(KeyCode.S);

        // AUDIO STATE:
        // When W is held -> crossfade to DRIVE
        // When W is not held -> crossfade to IDLE
        // (If you want reverse 'S' to also use DRIVE, replace 'accelerate' with: bool driving = accelerate || reverse;)
        if (accelerate)
        {
            Crossfade(
                fadeOut: idleSrc, outDur: fadeOutDuration,
                fadeIn: driveSrc, inDur: fadeInDuration, targetInVol: driveVolume,
                startOffset: driveStartOffset, pitchBase: baseDrivePitch, pitchJitter: drivePitchJitter
            );
        }
        else
        {
            Crossfade(
                fadeOut: driveSrc, outDur: fadeOutDuration,
                fadeIn: idleSrc, inDur: fadeInDuration, targetInVol: idleVolume,
                startOffset: idleStartOffset, pitchBase: baseIdlePitch, pitchJitter: idlePitchJitter
            );
        }

        // PHYSICS / JOINT MOTOR:
        if (accelerate)
        {
            motor.motorSpeed = -motorSpeed;
            ApplyMotor(true);
        }
        else if (reverse)
        {
            motor.motorSpeed = reverseSpeed;
            ApplyMotor(true);
        }
        else
        {
            ApplyMotor(false);
        }
    }

    private void ApplyMotor(bool active)
    {
        if (frontWheelJoint != null) frontWheelJoint.useMotor = active;
        if (rearWheelJoint != null) rearWheelJoint.useMotor = active;

        if (active)
        {
            if (frontWheelJoint != null) frontWheelJoint.motor = motor;
            if (rearWheelJoint != null) rearWheelJoint.motor = motor;
        }
    }

    private void HandleTilt()
    {
        float tiltInput = 0f;

        if (Input.GetKey(KeyCode.A)) tiltInput = 1f;
        else if (Input.GetKey(KeyCode.D)) tiltInput = -1f;

        if (chassisRb != null)
            chassisRb.AddTorque(tiltInput * tiltTorque * Time.deltaTime, ForceMode2D.Force);
    }

    // ---------------- AUDIO: XFADE / FADES ----------------
    private void Crossfade(
        AudioSource fadeOut, float outDur,
        AudioSource fadeIn, float inDur, float targetInVol,
        float startOffset, float pitchBase, float pitchJitter)
    {
        // Fade IN target
        if (fadeIn != null && fadeIn.clip != null)
        {
            // Slight pitch change each (re)start to avoid repetition
            fadeIn.pitch = pitchBase + Random.Range(-pitchJitter, pitchJitter);

            // If not already playing, start from offset
            if (!fadeIn.isPlaying)
            {
                if (startOffset > 0f)
                    fadeIn.time = Mathf.Clamp(startOffset, 0f, Mathf.Max(0f, fadeIn.clip.length - 0.001f));
                fadeIn.Play();
            }

            // Cancel existing fade and start new one to desired volume
            if (fadeIn == idleSrc && idleFadeCo != null) StopCoroutine(idleFadeCo);
            if (fadeIn == driveSrc && driveFadeCo != null) StopCoroutine(driveFadeCo);

            var coIn = StartCoroutine(FadeVolume(fadeIn, fadeIn.volume, targetInVol, inDur));
            if (fadeIn == idleSrc) idleFadeCo = coIn; else driveFadeCo = coIn;
        }

        // Fade OUT the other
        if (fadeOut != null && fadeOut.isPlaying)
        {
            if (fadeOut == idleSrc && idleFadeCo != null) StopCoroutine(idleFadeCo);
            if (fadeOut == driveSrc && driveFadeCo != null) StopCoroutine(driveFadeCo);

            var coOut = StartCoroutine(FadeOutThenStop(fadeOut, outDur));
            if (fadeOut == idleSrc) idleFadeCo = coOut; else driveFadeCo = coOut;
        }
    }

    private IEnumerator FadeVolume(AudioSource src, float from, float to, float duration)
    {
        float t = 0f;
        if (duration <= 0f)
        {
            src.volume = to;
            yield break;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            src.volume = Mathf.Lerp(from, to, k);
            yield return null;
        }
        src.volume = to;
    }

    private IEnumerator FadeOutThenStop(AudioSource src, float duration)
    {
        float startVol = src.volume;
        float t = 0f;

        if (duration <= 0f)
        {
            src.volume = 0f;
            src.Stop();
            yield break;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            src.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }

        src.volume = 0f;
        src.Stop();
    }

    // ---------------- AUDIO: LOOP TAIL TRIMMING ----------------
    /// <summary>
    /// If endOffset > 0, avoid the last 'endOffset' seconds of the clip by warping
    /// back to 'startOffset' when we reach the cutoff. Keeps loop seamless.
    /// </summary>
    private void MaintainLoopTrim(AudioSource src, float startOffset, float endOffset)
    {
        if (src == null || !src.isPlaying || src.clip == null || endOffset <= 0f) return;

        float cutoff = Mathf.Max(0f, src.clip.length - endOffset);
        if (src.time >= cutoff)
        {
            // Wrap to startOffset to skip the tail
            src.time = Mathf.Clamp(startOffset, 0f, Mathf.Max(0f, src.clip.length - 0.001f));
        }
    }
}
