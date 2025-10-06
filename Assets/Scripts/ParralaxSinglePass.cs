using UnityEngine;

public class ParallaxSinglePass : MonoBehaviour
{
    [Header("References")]
    public Transform player;              // Player transform
    public Camera cam;                    // Main orthographic camera
    public Transform background;          // Transform holding your background sprite
    public SpriteRenderer bgSprite;       // The background's SpriteRenderer

    [Header("Level Bounds (world X)")]
    public float levelStartX;             // Player.X at the start of the level
    public float levelEndX;               // Player.X at the end of the level

    // Optional: if your camera is not perfectly centered on the player,
    // you can assign a separate cameraFollow transform. If left null,
    // we just use cam.transform.position.
    public Transform cameraFollow;

    float camHalfWidth;
    float bgHalfWidth;

    void Awake()
    {
        if (cam == null) cam = Camera.main;

        if (!cam.orthographic)
        {
            Debug.LogWarning("ParallaxSinglePass: Camera should be orthographic.");
        }

        if (bgSprite == null || background == null || player == null || cam == null)
        {
            Debug.LogError("ParallaxSinglePass: Assign all references.");
            enabled = false;
            return;
        }

        // Visible half-width of the camera in world units
        camHalfWidth = cam.orthographicSize * cam.aspect;

        // Half width of the background sprite in world units
        bgHalfWidth = bgSprite.bounds.extents.x;

        // Quick sanity check
        float viewWidth = camHalfWidth * 2f;
        float bgWidth = bgHalfWidth * 2f;
        if (bgWidth < viewWidth)
        {
            Debug.LogWarning("ParallaxSinglePass: Background is narrower than the camera view. You will see empty space at the edges.");
        }

        if (Mathf.Approximately(levelStartX, levelEndX))
        {
            Debug.LogWarning("ParallaxSinglePass: levelStartX == levelEndX; cannot compute progress.");
        }
    }

    void LateUpdate()
    {
        // Player progress 0..1
        float t = Mathf.InverseLerp(levelStartX, levelEndX, player.position.x);

        // Where is the camera? If you have a follow target, use it, otherwise use the actual camera position.
        Vector3 camPos = cameraFollow ? cameraFollow.position : cam.transform.position;

        // Camera edges in world space *for this frame*
        float camLeft = camPos.x - camHalfWidth;
        float camRight = camPos.x + camHalfWidth;

        // Compute the X positions that align background edges with camera edges:
        // At start: background left edge == camera left edge  -> bgX_start = camLeft + bgHalfWidth
        // At end:   background right edge == camera right edge -> bgX_end   = camRight - bgHalfWidth
        float bgX_start = camLeft + bgHalfWidth;
        float bgX_end = camRight - bgHalfWidth;

        // Interpolate background X to scroll exactly once across the level
        float targetX = Mathf.Lerp(bgX_start, bgX_end, t);

        Vector3 pos = background.position;
        pos.x = targetX;
        background.position = pos;
    }

#if UNITY_EDITOR
    // Helpful gizmos to see your bounds in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(levelStartX, -1000, 0), new Vector3(levelStartX, 1000, 0));
        Gizmos.DrawLine(new Vector3(levelEndX, -1000, 0), new Vector3(levelEndX, 1000, 0));
    }
#endif
}
