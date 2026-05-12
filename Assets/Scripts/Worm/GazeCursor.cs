using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    public Camera cam;
    public float cursorRadius = 0.35f;
    public Color offColor = new Color(1f, 1f, 1f, 0.7f);
    public Color onColor  = new Color(0.2f, 1f, 0.2f, 0.9f);

    public bool OnTarget { get; private set; }

    private LineRenderer _ring;
    private Material _ringMat;
    private const int Segments = 32;

    void Awake()
    {
        _ring = gameObject.AddComponent<LineRenderer>();
        _ring.loop = true;
        _ring.positionCount = Segments;
        _ring.startWidth = 0.07f;
        _ring.endWidth   = 0.07f;
        _ring.useWorldSpace  = true;
        _ring.numCapVertices = 4;

        _ringMat = new Material(Shader.Find("Sprites/Default"));
        _ringMat.renderQueue = 4000;
        _ringMat.SetColor("_Color", offColor);
        _ring.sharedMaterial = _ringMat;
    }

    void Update()
    {
        if (cam == null) return;



        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        OnTarget = false;
        Vector3 ringCenter;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            ringCenter = hit.point - cam.transform.forward * 0.08f;
            if (hit.collider.GetComponentInParent<WormHead>() != null)
                OnTarget = true;
        }
        else
        {

            ringCenter = cam.transform.position + cam.transform.forward * 5f;
        }

        transform.position = ringCenter;



        Vector3 right = Vector3.Cross(cam.transform.forward, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.001f) right = Vector3.right;
        Vector3 up = Vector3.Cross(right, cam.transform.forward).normalized;

        for (int i = 0; i < Segments; i++)
        {
            float angle = i / (float)Segments * Mathf.PI * 2f;
            _ring.SetPosition(i, ringCenter
                + right * (Mathf.Cos(angle) * cursorRadius)
                + up    * (Mathf.Sin(angle) * cursorRadius));
        }

        _ringMat.SetColor("_Color", OnTarget ? onColor : offColor);
    }
}
