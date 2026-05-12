using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    public float spawnZ        = 25f;
    public float lateralRange  = 2.5f;
    public float verticalMin   = 1.5f;
    public float verticalMax   = 5.5f;

    [Header("Coin")]
    public int bonusPoints = 5;

    private float _timer;

    void Start()
    {
        if (DifficultyConfig.Instance != null && DifficultyConfig.Instance.Current != null)
            spawnInterval = DifficultyConfig.Instance.Current.obstacleSpawnInterval * 1.8f;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < spawnInterval) return;
        _timer = 0f;
        DoSpawn();
    }

    void DoSpawn()
    {
        float x = Random.Range(-lateralRange, lateralRange);
        float y = Random.Range(verticalMin, verticalMax);
        MakeCoin(new Vector3(x, y, transform.position.z + spawnZ), bonusPoints);
    }

    public static void MakeCoin(Vector3 pos, int bonus)
    {
        var root = new GameObject("Coin");
        root.transform.position = pos;

        var shader = Shader.Find("Universal Render Pipeline/Lit")
                  ?? Shader.Find("Universal Render Pipeline/Unlit")
                  ?? Shader.Find("Standard");
        var mat = new Material(shader) { name = "CoinGold" };
        mat.color = new Color(1f, 0.82f, 0.08f);
        if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic",   0.9f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.8f);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.6f, 0.45f, 0f) * 0.6f);
        }


        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(root.transform);
        sphere.transform.localScale = Vector3.one * 0.38f;
        sphere.GetComponent<Renderer>().material = mat;
        Object.Destroy(sphere.GetComponent<SphereCollider>());


        for (int i = 0; i < 2; i++)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.transform.SetParent(root.transform);
            disc.transform.localScale    = new Vector3(0.7f, 0.02f, 0.7f);
            disc.transform.localRotation = Quaternion.Euler(0f, 45f * i, 0f);
            disc.GetComponent<Renderer>().material = mat;
            Object.Destroy(disc.GetComponent<CapsuleCollider>());
        }

        var col    = root.AddComponent<SphereCollider>();
        col.radius    = 0.5f;
        col.isTrigger = true;

        var cc = root.AddComponent<CoinCollectible>();
        cc.bonusPoints = bonus;
    }
}
