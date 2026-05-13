using UnityEngine;
using UnityEngine.UI;

public class GazeProgressIndicator : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    CanvasGroup _group;

    void Awake()
    {
        // Use a CanvasGroup on this GameObject to fade the whole ring in/out.
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;

        // Replace sprites with guaranteed-circular ones generated at runtime.
        // Sprite assets can compress/alias and render as a square; programmatic circles always have clean alpha.
        if (fillImage != null)
        {
            fillImage.sprite        = MakeCircleSprite(128);
            fillImage.type          = Image.Type.Filled;
            fillImage.fillMethod    = Image.FillMethod.Radial360;
            fillImage.fillOrigin    = (int)Image.Origin360.Top;
            fillImage.fillClockwise = true;
            fillImage.fillAmount    = 0f;
        }

        // Fix RingInner (the center cap) — same asset, same square problem.
        var innerT = transform.Find("RingInner");
        if (innerT != null)
        {
            var innerImg = innerT.GetComponent<Image>();
            if (innerImg != null)
                innerImg.sprite = MakeCircleSprite(64);
        }

        // Fix RingBG (the track ring) — use a donut sprite so only the ring band shows.
        var bgT = transform.Find("RingBG");
        if (bgT != null)
        {
            var bgImg = bgT.GetComponent<Image>();
            if (bgImg != null)
                bgImg.sprite = MakeRingSprite(128, 0.55f);
        }
    }

    public void SetProgress(float t)
    {
        if (fillImage != null) fillImage.fillAmount = t;
    }

    public void Hide()
    {
        SetProgress(0f);
        if (_group != null) _group.alpha = 0f;
    }

    public void Show()
    {
        if (_group != null) _group.alpha = 1f;
    }

    static Sprite MakeCircleSprite(int size)
    {
        var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float r    = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float a  = Mathf.Clamp01((r - Mathf.Sqrt(dx * dx + dy * dy)) / 1.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // Donut/ring sprite — opaque in the band between innerFraction and outer edge.
    static Sprite MakeRingSprite(int size, float innerFraction)
    {
        var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float r     = size * 0.5f;
        float rInner = r * innerFraction;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx   = x - r + 0.5f, dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float aOuter = Mathf.Clamp01((r      - dist) / 1.5f);
                float aInner = Mathf.Clamp01((dist - rInner) / 1.5f);
                float a = aOuter * aInner;
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
