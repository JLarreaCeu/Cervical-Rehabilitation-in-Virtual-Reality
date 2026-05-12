using UnityEngine;

public class TunnelBuilder : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Tunnel Visual Settings")]
    public float tunnelRadius  = 0.55f;
    public Material tunnelMaterial;
    public Material earthMaterial;
    public Material rootMaterial;

    [Header("Wall")]
    public float wallWidth  = 30f;
    public float wallHeight = 12f;

    [Header("Seed")]
    public int randomSeed = 42;

    void Start()
    {
        BuildWall();
        if (waypoints != null && waypoints.Length > 1)
            BuildTunnel();
        AddRoots();
    }

    void BuildWall()
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        wall.name = "EarthWall";
        wall.transform.SetParent(transform);
        wall.transform.localPosition = new Vector3(0f, 0f, 0.6f);
        wall.transform.localScale    = new Vector3(wallWidth, wallHeight, 1f);
        Destroy(wall.GetComponent<MeshCollider>());
        if (earthMaterial != null)
            wall.GetComponent<Renderer>().sharedMaterial = earthMaterial;
    }

    void BuildTunnel()
    {

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 a = waypoints[i].position;
            Vector3 b = waypoints[i + 1].position;
            Vector3 mid = (a + b) * 0.5f;
            float   len = Vector3.Distance(a, b);

            var seg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            seg.name = "TunnelSeg_" + i;
            seg.transform.SetParent(transform);
            seg.transform.position   = mid;
            seg.transform.up         = (b - a).normalized;
            seg.transform.localScale = new Vector3(tunnelRadius * 2f, len * 0.5f, tunnelRadius * 2f);


            Destroy(seg.GetComponent<CapsuleCollider>());

            if (tunnelMaterial != null)
                seg.GetComponent<Renderer>().sharedMaterial = tunnelMaterial;
        }
    }

    void AddRoots()
    {
        if (rootMaterial == null) return;
        var rng = new System.Random(randomSeed);


        for (int i = 0; i < 18; i++)
        {
            float x = (float)(rng.NextDouble() * wallWidth - wallWidth * 0.5f);
            float y = (float)(rng.NextDouble() * wallHeight - wallHeight * 0.5f);
            float angle = (float)(rng.NextDouble() * 180f);
            float len   = (float)(rng.NextDouble() * 1.5f + 0.4f);

            var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            root.name = "Root_" + i;
            root.transform.SetParent(transform);
            root.transform.position     = new Vector3(x, y, 0.55f);
            root.transform.eulerAngles  = new Vector3(angle, 0f, 0f);
            root.transform.localScale   = new Vector3(0.04f, len, 0.04f);
            Destroy(root.GetComponent<CapsuleCollider>());
            root.GetComponent<Renderer>().sharedMaterial = rootMaterial;
        }
    }
}
