using UnityEngine;

public class MeteorSightLinesManager : MonoBehaviour
{
    [Header("Scene refs")]
    public Transform meteor; // Meteor object
    public Transform earth; // Earth object
    public Transform northPoint; // North Point of the Meter
    public Transform southPoint; // South point of the Meteor

    [Header("Rendering")]
    public float centerWidth = 0.2f; // The line from the Meteor itself
    public float edgeWidth = 0.06f; // The lines from North and South points
    //public Color centerColor = Color.blue;
    public Color centerColor = new Color(0f, 0f, 1f, 0.1f);
    public Color edgeColor = new Color(1f, 0f, 0f, 0.1f);
    public float maxDistance = 5000f;

    [Header("Raycast")]
    public LayerMask earthMask; // Set to only the Earth layer

    private LineRenderer lrCenter, lrNorth, lrSouth;

    void Awake()
    {
        // If edge points aren’t assigned, try to find them under the meteor
        if (meteor != null)
        {
            if (northPoint == null) { var t = meteor.Find("NorthPoint"); if (t) northPoint = t; }
            if (southPoint == null) { var t = meteor.Find("SouthPoint"); if (t) southPoint = t; }
        }

        // Create renderers as children of the parent
        lrCenter = CreateLR("LOS_Center", centerWidth, centerColor, this.transform);
        lrNorth = CreateLR("LOS_North", edgeWidth, edgeColor, this.transform);
        lrSouth = CreateLR("LOS_South", edgeWidth, edgeColor, this.transform);
    }

    void Update()
    {
        if (meteor == null || earth == null) return;

        // Center line: from meteor to Earth
        UpdateLine(meteor, lrCenter);

        // North and South points
        if (northPoint != null) UpdateLine(northPoint, lrNorth);
        if (southPoint != null) UpdateLine(southPoint, lrSouth);
    }

    // Public so you can swap meteors at runtime if needed
    public void SetMeteor(Transform newMeteor, Transform newNorth = null, Transform newSouth = null)
    {
        meteor = newMeteor;
        northPoint = newNorth != null ? newNorth : (newMeteor ? newMeteor.Find("NorthPoint") : null);
        southPoint = newSouth != null ? newSouth : (newMeteor ? newMeteor.Find("SouthPoint") : null);
    }

    LineRenderer CreateLR(string name, float width, Color col, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = lr.endWidth = width;

        // --- Make a transparent URP material so alpha works ---
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        // Switch surface type to Transparent and set proper blending
        mat.SetFloat("_Surface", 1f); // 0=Opaque, 1=Transparent
        mat.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        mat.color = col;
        lr.material = mat;

        lr.startColor = col;
        lr.endColor = col;

        lr.numCornerVertices = 4;
        lr.numCapVertices = 4;
        return lr;
    }

    void UpdateLine(Transform origin, LineRenderer lr)
    {
        Vector3 start = origin.position;
        Vector3 dir = (earth.position - start).normalized;

        Vector3 end = start + dir * maxDistance;

        if (Physics.Raycast(start, dir, out RaycastHit hit, maxDistance, earthMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point; // stop at Earth surface
        }

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}
