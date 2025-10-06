using System.Collections.Generic;
using UnityEngine;

public class CheckpointPose2D : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerRoot;   // your 'motorbike' GameObject
    [SerializeField] private Rigidbody2D chassis;    // the moving body

    [Header("Options")]
    [SerializeField] private bool isFirstCheckpoint = false;
    [SerializeField] private Transform spawnPoint;   // optional exact chassis spawn pose (else uses this transform)

    // Saved spawn (chassis world pose) + per-part local offsets relative to chassis
    private static bool s_has;
    private static Vector2 s_spawnChassisPos;
    private static float s_spawnChassisRotZ;
    private static Dictionary<string, (Vector2 localPos, float localRotZ)> s_localByName;

    private void Awake()
    {
        if (!spawnPoint) spawnPoint = transform;
        if (isFirstCheckpoint)
            Capture();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (BelongsToPlayer(other))
            Capture();
    }

    private void Capture()
    {
        if (!playerRoot || !chassis)
        {
            Debug.LogWarning("CheckpointPose2D: assign playerRoot + chassis.");
            return;
        }

        // Where to spawn the chassis
        s_spawnChassisPos = (Vector2)spawnPoint.position;
        s_spawnChassisRotZ = spawnPoint.eulerAngles.z;

        // Save every RB2D's local pose relative to the chassis (works even if they are siblings)
        s_localByName = new Dictionary<string, (Vector2, float)>(16);
        var bodies = playerRoot.GetComponentsInChildren<Rigidbody2D>(true);
        var chT = chassis.transform;

        foreach (var rb in bodies)
        {
            var t = rb.transform;
            Vector2 localPos = chT.InverseTransformPoint(t.position);
            float localRot = Mathf.DeltaAngle(chT.eulerAngles.z, t.eulerAngles.z);
            s_localByName[rb.gameObject.name] = (localPos, localRot);
        }

        s_has = true;
        // Debug.Log("Checkpoint captured.");
    }

    public static bool HasPose() => s_has;
    public static Vector2 GetSpawnPos() => s_spawnChassisPos;
    public static float GetSpawnRotZ() => s_spawnChassisRotZ;
    public static IReadOnlyDictionary<string, (Vector2 localPos, float localRotZ)> GetLocalMap() => s_localByName;

    private bool BelongsToPlayer(Collider2D col)
    {
        if (!playerRoot) return false;
        if (col.transform == playerRoot) return true;
        if (col.attachedRigidbody && col.attachedRigidbody.transform.IsChildOf(playerRoot)) return true;
        return col.transform.IsChildOf(playerRoot);
    }
}
