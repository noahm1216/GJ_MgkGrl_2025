using UnityEngine;

/// <summary>
/// Parallax driven by a cumulative 'distance' (world units),
/// kept perfectly looped within the camera view.
/// Works with orthographic (common for side scrollers) and perspective cameras.
/// </summary>
public class ParallaxFromDistanceCameraTiled : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera that defines what must be covered by tiles. If null, Camera.main is used.")]
    public Camera targetCamera;

    [Header("Distance Input")]
    [Min(0f)]
    [Tooltip("Optional exponential smoothing time (seconds) for the incoming distance if it changes jerkily.")]
    public float smoothingTime = 0f;

    [Tooltip("Typically -1 so increasing distance scrolls scenery to the left.")]
    public float directionFactor = -1f;

    [System.Serializable]
    public class Layer
    {
        [Header("Layer Setup")]
        [Tooltip("Tiling segments. Provide 3 or more. They will be reused each frame to cover the camera view.")]
        public Transform[] segments;

        [Tooltip("0 = far background, 1 = foreground. >1 for faster-than-world foreground accents.")]
        [Range(0f, 2f)]
        public float parallaxMultiplier = 0.3f;

        [Header("Dimensions")]
        [Tooltip("Width of ONE segment in world units. Leave 0 to auto-detect from SpriteRenderer.bounds.size.x on the first segment.")]
        public float segmentWidth = 0f;

        [Header("Locks")]
        [Tooltip("Keep Y and Z fixed for classic side-scroller behavior.")]
        public bool lockYAndZ = true;

        [HideInInspector] public float lockedY;
        [HideInInspector] public float lockedZ;
        [HideInInspector] public float anchorX;          // initial mid-x anchor
        [HideInInspector] public float smoothedDistance; // for optional smoothing
    }

    [Header("Layers (back-to-front)")]
    public Layer[] layers;

    [Header("Coverage")]
    [Tooltip("Extra horizontal padding in world units on both sides beyond the camera view (prevents subpixel gaps).")]
    public float horizontalPadding = 0.25f;

    [Tooltip("Minimum number of tiles to lay across the view (clamped by provided segment count). Usually leave as 0 to auto-compute.")]
    public int minTilesAcross = 0;

    // ---- PUSH MODEL API ----
    private float _distance;

    /// <summary>Push your cumulative distance here each frame.</summary>
    public void SetDistance(float distance)
    {
        _distance = distance;
    }

    void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        foreach (var layer in layers)
        {
            if (layer.segments == null || layer.segments.Length < 3)
            {
                Debug.LogError($"{nameof(ParallaxFromDistanceCameraTiled)}: Each layer needs at least 3 segments.");
                enabled = false;
                return;
            }

            // Sort by X so we can grab a sensible middle at start
            System.Array.Sort(layer.segments, (a, b) => a.position.x.CompareTo(b.position.x));
            var mid = layer.segments[layer.segments.Length / 2];

            // Detect width if not provided
            if (layer.segmentWidth <= 0f)
            {
                float w = DetectWidthFromSprite(mid);
                if (w <= 0f)
                {
                    Debug.LogWarning($"{nameof(ParallaxFromDistanceCameraTiled)}: Could not auto-detect segment width for {mid.name}. Please set it manually.");
                }
                else layer.segmentWidth = w;
            }

            // Lock Y/Z
            if (layer.lockYAndZ)
            {
                layer.lockedY = mid.position.y;
                layer.lockedZ = mid.position.z;
                foreach (var seg in layer.segments)
                {
                    var p = seg.position;
                    seg.position = new Vector3(p.x, layer.lockedY, layer.lockedZ);
                }
            }

            // Use current mid X as anchor
            layer.anchorX = mid.position.x;
            layer.smoothedDistance = _distance;
        }
    }

    void LateUpdate()
    {
        _distance = Manager_GameState.Instance.distanceOfCurrentGameRun * -1;
        Debug.Log(_distance);
        
        if (targetCamera == null) return;

        float dt = Time.deltaTime;
        float alpha = (smoothingTime > 0f) ? 1f - Mathf.Exp(-dt / smoothingTime) : 1f;

        // Camera visible width in world units
        float viewWidth = GetCameraViewWidth(targetCamera);

        foreach (var layer in layers)
        {
            // Smooth (optional)
            layer.smoothedDistance = (smoothingTime > 0f)
                ? Mathf.Lerp(layer.smoothedDistance, _distance, alpha)
                : _distance;

            float w = Mathf.Max(0.0001f, layer.segmentWidth);

            // Signed scroll offset for this layer
            float s = directionFactor * (layer.smoothedDistance * layer.parallaxMultiplier);

            // The "center" of the infinite tiling before snapping to tiles
            float center = layer.anchorX - s;

            // Determine how many tiles we need to cover the camera width + padding
            float neededWidth = viewWidth + horizontalPadding * 2f;
            int tilesNeeded = Mathf.CeilToInt(neededWidth / w) + 1; // +1 guard
            if (minTilesAcross > 0) tilesNeeded = Mathf.Max(tilesNeeded, minTilesAcross);

            // We'll place up to segments.Length tiles; warn if not enough to fully cover
            if (tilesNeeded > layer.segments.Length)
            {
                // Not fatal—we'll still place as many as we have
                //Debug.LogWarningOnce($"{nameof(ParallaxFromDistanceCameraTiled)}: Layer '{(layer.segments[0] != null ? layer.segments[0].parent?.name : "Layer")}' needs {tilesNeeded} tiles to fully cover view, but only {layer.segments.Length} provided.");
                tilesNeeded = layer.segments.Length;
            }

            // Find the leftmost tile index such that tiles cover view
            // Choose k so that tile at (center + k*w) is the center tile near camera center; then we extend left/right.
            int k = Mathf.FloorToInt(center / w);
            // Find camera center x at layer depth
            float camCenterX = GetCameraCenterX(targetCamera, layer.lockYAndZ ? layer.lockedZ : layer.segments[0].position.z);

            // Snap the mid tile to be near camera center
            float midX = center + k * w;
            // shift k so the mid tile is closest to cam center
            int midIndex = Mathf.RoundToInt((camCenterX - midX) / w);
            int firstTileIndex = (k + midIndex) - tilesNeeded / 2;

            // Compute leftmost tile world X (index firstTileIndex)
            float firstX = center + firstTileIndex * w;

            // Place tiles in order across the view
            for (int i = 0; i < tilesNeeded; i++)
            {
                Transform seg = layer.segments[i];
                Vector3 p = seg.position;
                p.x = firstX + i * w;

                if (layer.lockYAndZ)
                {
                    p.y = layer.lockedY;
                    p.z = layer.lockedZ;
                }

                seg.position = p;
            }

            // If we provided more segments than needed, park the extras just outside the view to avoid overlap
            for (int i = tilesNeeded; i < layer.segments.Length; i++)
            {
                Transform seg = layer.segments[i];
                Vector3 p = seg.position;
                p.x = firstX + (i * w); // still aligned, just beyond coverage
                if (layer.lockYAndZ)
                {
                    p.y = layer.lockedY;
                    p.z = layer.lockedZ;
                }
                seg.position = p;
            }
        }
    }

    // --- Helpers ---

    private static float DetectWidthFromSprite(Transform t)
    {
        var sr = t.GetComponent<SpriteRenderer>();
        return (sr && sr.sprite) ? sr.bounds.size.x : 0f;
    }

    private static float GetCameraViewWidth(Camera cam)
    {
        if (cam.orthographic)
            return 2f * cam.orthographicSize * cam.aspect;

        // Perspective: derive width at camera center plane (z at camera)
        // Safer: measure via two viewport points at mid-height
        Vector3 a = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, cam.nearClipPlane + 1f));
        Vector3 b = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, cam.nearClipPlane + 1f));
        return Mathf.Abs(b.x - a.x);
    }

    private static float GetCameraCenterX(Camera cam, float layerZ)
    {
        if (cam.orthographic)
            return cam.transform.position.x;

        // For perspective, project viewport center to world at layerZ plane.
        // We can raycast from camera to a plane Z=layerZ.
        Vector3 origin = cam.transform.position;
        Vector3 dir = cam.transform.forward;
        // Plane parallel to XY at Z=layerZ
        float t = (layerZ - origin.z) / (Mathf.Abs(dir.z) < 1e-6f ? 1e-6f : dir.z);
        return origin.x + dir.x * t;
    }
}
