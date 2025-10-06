using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraMovement : MonoBehaviour
{
    [System.Serializable]
    public class CameraAnchor
    {
        public string key;
        public Transform transform;
    }

    [System.Serializable]
    public class Route
    {
        [Tooltip("Start key (A)")]
        public string from;
        [Tooltip("End key (B)")]
        public string to;
        [Tooltip("Intermediate keys (e.g., [C, D])")]
        public List<string> via = new List<string>();
        [Tooltip("If true, the reverse path (B -> ...via reversed... -> A) is also valid.")]
        public bool bidirectional = true;
    }

    [Header("Anchors (define your named camera positions here)")]
    [SerializeField] private List<CameraAnchor> anchors = new();

    [Header("Optional Routes (only for pairs that need A -> via -> B)")]
    [SerializeField] private List<Route> routes = new();

    [Header("Motion")]
    [Tooltip("Seconds to move between each pair of waypoints.")]
    [SerializeField, Min(0.01f)] private float perLegDuration = 1.0f;
    [Tooltip("Easing curve for movement & rotation (0..1 time -> 0..1 progress).")]
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("If true, time is unaffected by timeScale (e.g., during pause menus).")]
    [SerializeField] private bool useUnscaledTime = false;
    [Tooltip("If true, snap to exact target at the end of each leg (avoid drift).")]
    [SerializeField] private bool snapAtEnd = true;

    [Header("Initial State")]
    [Tooltip("Optional: Key to move to on Start(). Leave empty to use current transform.")]
    [SerializeField] private string startAtKey = "Bed";

    // Runtime lookups
    private readonly Dictionary<string, Transform> _keyToTransform = new();
    private Coroutine _moveRoutine;
    private string _currentKey = null;

    void Awake()
    {
        // Build runtime dictionary; validate unique keys
        _keyToTransform.Clear();
        foreach (var a in anchors)
        {
            if (a == null || a.transform == null || string.IsNullOrWhiteSpace(a.key)) continue;
            if (_keyToTransform.ContainsKey(a.key))
                Debug.LogWarning($"Duplicate camera key '{a.key}'. Only the first will be used.");
            else
                _keyToTransform.Add(a.key, a.transform);
        }

        // Attempt to infer current key if we start at (or very near) a known anchor
        _currentKey = FindNearestKeyTo(transform.position, maxDist: 0.05f); // small tolerance
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(startAtKey) && HasCamera(startAtKey))
        {
            // Start by snapping (instant) so we begin from a known anchor/key.
            SnapTo(startAtKey);
        }
    }

    // ---------- Public API ----------

    /// <summary>Move to a single key (uses a route if one is defined for currentKey->key).</summary>
    public void MoveTo(string key)
    {
        if (!HasCamera(key))
        {
            Debug.LogWarning($"MoveTo: Unknown camera key '{key}'.");
            return;
        }

        // If we know our current key and there's a matching route, use it; else go direct
        var sequence = ResolveRoute(_currentKey, key);
        MoveThrough(sequence);
    }

    /// <summary>Move through an explicit sequence of keys, e.g., MoveThrough("A","C","B").</summary>
    public void MoveThrough(params string[] keys) => MoveThrough((IEnumerable<string>)keys);

    /// <summary>Move through an explicit sequence of keys given as IEnumerable.</summary>
    public void MoveThrough(IEnumerable<string> keys)
    {
        var path = new List<string>();
        foreach (var k in keys)
        {
            if (!HasCamera(k))
            {
                Debug.LogWarning($"MoveThrough: Unknown camera key '{k}'. Skipping.");
                continue;
            }
            path.Add(k);
        }

        if (path.Count == 0) return;

        // Interrupt any current movement
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MovePathRoutine(path));
    }

    /// <summary>Instantly snap to a key (no animation).</summary>
    public void SnapTo(string key)
    {
        if (!HasCamera(key)) return;
        var t = _keyToTransform[key];
        transform.SetPositionAndRotation(t.position, t.rotation);
        _currentKey = key;
    }

    /// <summary>Does a camera key exist?</summary>
    public bool HasCamera(string key) => !string.IsNullOrEmpty(key) && _keyToTransform.ContainsKey(key);

    /// <summary>Try-move wrapper (returns false if key is unknown).</summary>
    public bool TryMoveTo(string key)
    {
        if (!HasCamera(key)) return false;
        MoveTo(key);
        return true;
    }

    // ---------- Internals ----------

    private List<string> ResolveRoute(string fromKey, string toKey)
    {
        // If we don't know where we are, just go direct to the target
        if (string.IsNullOrEmpty(fromKey))
            return new List<string> { toKey };

        // Exact match route (A -> B)
        foreach (var r in routes)
        {
            if (r == null) continue;

            // Forward
            if (r.from == fromKey && r.to == toKey)
            {
                var seq = new List<string> { fromKey };
                if (r.via != null && r.via.Count > 0) seq.AddRange(r.via);
                seq.Add(toKey);
                // We return WITHOUT the starting key because MovePathRoutine expects target keys only
                seq.RemoveAt(0);
                return seq;
            }

            // Reverse (B -> A)
            if (r.bidirectional && r.from == toKey && r.to == fromKey)
            {
                var seq = new List<string> { fromKey }; // current
                if (r.via != null && r.via.Count > 0)
                {
                    // Reverse of via
                    for (int i = r.via.Count - 1; i >= 0; --i)
                        seq.Add(r.via[i]);
                }
                seq.Add(toKey);
                seq.RemoveAt(0);
                return seq;
            }
        }

        // No special route -> direct
        return new List<string> { toKey };
    }

    private IEnumerator MovePathRoutine(List<string> pathKeys)
    {
        foreach (var key in pathKeys)
        {
            var target = _keyToTransform[key];
            yield return SmoothMoveTo(target, perLegDuration);
            if (snapAtEnd) transform.SetPositionAndRotation(target.position, target.rotation);
            _currentKey = key;
        }
        _moveRoutine = null;
    }

    private IEnumerator SmoothMoveTo(Transform target, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float t = 0f;
        while (t < 1f)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float step = (duration <= 0f) ? 1f : dt / duration;
            t = Mathf.Min(1f, t + step);

            float eased = ease.Evaluate(t);
            transform.position = Vector3.LerpUnclamped(startPos, endPos, eased);
            transform.rotation = Quaternion.SlerpUnclamped(startRot, endRot, eased);

            yield return null;
        }
    }

    private string FindNearestKeyTo(Vector3 pos, float maxDist)
    {
        string bestKey = null;
        float bestSqr = maxDist * maxDist;
        foreach (var kvp in _keyToTransform)
        {
            float sqr = (kvp.Value.position - pos).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                bestKey = kvp.Key;
            }
        }
        return bestKey;
    }

    // ---------- Convenience methods analogous to your original example ----------
    public void GetBedOptions() => MoveTo("Bed");
}
