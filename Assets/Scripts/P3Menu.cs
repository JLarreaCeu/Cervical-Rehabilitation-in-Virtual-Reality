using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class P3Menu : MonoBehaviour
{


    [Serializable]
    public class MenuItem
    {
        public RectTransform   root;
        public Image           highlight;
        public Image           shadow;
        public Image           glow;
        public TextMeshProUGUI darkLabel;
        public TextMeshProUGUI brightLabel;
        public float           fontSize   = 68f;
        public Vector2         offset     = Vector2.zero;
        public float           skewX      = 0f;
        public float           skewY      = 0f;
        public string          label      = "ITEM";
        public string          page       = "";
    }

    [SerializeField] MenuItem[] items;
    [SerializeField] CanvasGroup hintGroup;


    static readonly Color CyanDefault    = new Color(0.235f, 0.886f, 1f);
    static readonly Color CyanHover      = new Color(0f,     0.851f, 1f);
    static readonly Color DarkActive     = new Color(0.42f,  0f,     0.063f);
    static readonly Color RedBright      = new Color(1f,     0.165f, 0.165f);
    static readonly Color ShadowColor    = new Color(0.922f, 0.314f, 0.471f, 0.85f);
    static readonly Color HighlightColor = Color.white;


    int  _active  = 0;
    bool _mounted = false;


    Coroutine[] _highlightCo;
    Coroutine[] _glowCo;
    Coroutine[] _shadowCo;
    Coroutine[] _opacityCo;



    static float EaseOutExpo(float t) =>
        t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);


    static float EaseSpring(float t)
    {
        const float c4 = (2f * Mathf.PI) / 3f;
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }



    void Awake()
    {
        _highlightCo = new Coroutine[items.Length];
        _glowCo      = new Coroutine[items.Length];
        _shadowCo    = new Coroutine[items.Length];
        _opacityCo   = new Coroutine[items.Length];
    }

    void Start()
    {
        InitialState();
        StartCoroutine(MountSequence());
    }

    void Update()
    {
        HandleInput();
    }



    void InitialState()
    {
        for (int i = 0; i < items.Length; i++)
        {
            var it = items[i];


            it.darkLabel.text   = it.label;
            it.brightLabel.text = it.label;
            it.darkLabel.fontSize   = it.fontSize;
            it.brightLabel.fontSize = it.fontSize;


            it.root.anchoredPosition = it.offset;


            it.root.localRotation = Quaternion.Euler(0f, 0f, -it.skewX * 0.5f);


            var cg = GetOrAddCanvasGroup(it.root);
            cg.alpha = 0f;
            it.root.anchoredPosition += new Vector2(36f, 0f);


            SetHighlightScale(it, 0f);


            it.shadow.color = new Color(ShadowColor.r, ShadowColor.g, ShadowColor.b, 0f);


            it.glow.color = new Color(1f, 1f, 1f, 0f);


            it.darkLabel.color   = CyanDefault;
            it.brightLabel.color = new Color(RedBright.r, RedBright.g, RedBright.b, 0f);
        }

        if (hintGroup) hintGroup.alpha = 0f;
    }

    IEnumerator MountSequence()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < items.Length; i++)
        {
            int idx = i;
            StartCoroutine(EntryAnim(idx, i * 0.08f));
        }


        if (hintGroup)
            StartCoroutine(FadeCanvasGroup(hintGroup, 0f, 1f, 0.5f, 0.9f));

        yield return new WaitForSeconds(items.Length * 0.08f + 0.38f);

        _mounted = true;
        Activate(0);
    }

    IEnumerator EntryAnim(int idx, float delay)
    {
        yield return new WaitForSeconds(delay);

        var it  = items[idx];
        var cg  = GetOrAddCanvasGroup(it.root);
        var startPos = it.root.anchoredPosition;
        var endPos   = startPos - new Vector2(36f, 0f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / 0.38f;
            float e = EaseOutExpo(Mathf.Clamp01(t));
            cg.alpha = Mathf.Lerp(0f, 1f, e);
            it.root.anchoredPosition = Vector2.Lerp(startPos, endPos, e);
            yield return null;
        }

        cg.alpha = 1f;
        it.root.anchoredPosition = endPos;
    }



    void HandleInput()
    {
        if (!_mounted) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.upArrowKey.wasPressedThisFrame)
            Activate(Mathf.Max(0, _active - 1));

        if (kb.downArrowKey.wasPressedThisFrame)
            Activate(Mathf.Min(items.Length - 1, _active + 1));

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            OnConfirm();
    }


    public void OnHover(int idx) { if (_mounted) Activate(idx); }


    public void OnClick(int idx) { if (_mounted) OnConfirm(); }

    void OnConfirm()
    {
        Debug.Log($"[P3Menu] Confirmed: {items[_active].page}");


    }



    void Activate(int idx)
    {
        int prev = _active;
        _active  = idx;

        for (int i = 0; i < items.Length; i++)
            UpdateItemVisual(i, prev);
    }

    void UpdateItemVisual(int i, int prev)
    {
        bool isActive = (i == _active);
        int  dist     = Mathf.Abs(i - _active);
        var  it       = items[i];


        float targetAlpha = isActive ? 1f : Mathf.Max(0.5f, 1f - dist * 0.2f);
        RestartCoroutine(ref _opacityCo[i], FadeLabel(it, targetAlpha, 0.12f));


        float targetScale = isActive ? 1f : 0f;
        RestartCoroutine(ref _highlightCo[i], AnimHighlight(it, targetScale, 0.22f));


        float targetGlow = isActive ? 1f : 0f;
        RestartCoroutine(ref _glowCo[i], FadeImage(it.glow, targetGlow, 0.3f));


        if (isActive)
            RestartCoroutine(ref _shadowCo[i], ShadowPop(it));
        else
            RestartCoroutine(ref _shadowCo[i], FadeImage(it.shadow, 0f, 0.1f));


        RestartCoroutine(ref _highlightCo[i], AnimHighlight(it, targetScale, 0.22f));
        StartCoroutine(AnimLabelColor(it, isActive, 0.12f));
    }



    IEnumerator AnimHighlight(MenuItem it, float targetScaleX, float duration)
    {
        var rect = it.highlight.rectTransform;
        float startX = rect.localScale.x;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float e = EaseOutExpo(Mathf.Clamp01(t));
            float sx = Mathf.Lerp(startX, targetScaleX, e);
            rect.localScale = new Vector3(sx, rect.localScale.y, 1f);
            yield return null;
        }

        rect.localScale = new Vector3(targetScaleX, rect.localScale.y, 1f);
    }

    IEnumerator ShadowPop(MenuItem it)
    {
        it.shadow.color = ShadowColor;
        var rect = it.shadow.rectTransform;


        (float t, Vector3 scale)[] keys =
        {
            (0.00f, new Vector3(0f,    1f,    1f)),
            (0.55f, new Vector3(1.22f, 1.18f, 1f)),
            (0.75f, new Vector3(0.96f, 0.97f, 1f)),
            (1.00f, new Vector3(1f,    1f,    1f)),
        };

        float dur = 0.28f;
        float t   = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float tc = Mathf.Clamp01(t);
            rect.localScale = EvalKeyframes(keys, tc);
            yield return null;
        }

        rect.localScale = Vector3.one;
    }

    IEnumerator FadeImage(Image img, float targetAlpha, float duration)
    {
        float startA = img.color.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Lerp(startA, targetAlpha, Mathf.Clamp01(t));
            var c = img.color;
            img.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        var fc = img.color;
        img.color = new Color(fc.r, fc.g, fc.b, targetAlpha);
    }

    IEnumerator FadeLabel(MenuItem it, float targetAlpha, float duration)
    {
        float startA = it.darkLabel.color.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Lerp(startA, targetAlpha, Mathf.Clamp01(t));
            var dc = it.darkLabel.color;
            it.darkLabel.color = new Color(dc.r, dc.g, dc.b, a);
            yield return null;
        }
    }

    IEnumerator AnimLabelColor(MenuItem it, bool active, float duration)
    {
        Color darkTarget   = active ? DarkActive : CyanDefault;
        Color brightTarget = active ? new Color(RedBright.r, RedBright.g, RedBright.b, 1f)
                                    : new Color(RedBright.r, RedBright.g, RedBright.b, 0f);
        Color darkStart   = it.darkLabel.color;
        Color brightStart = it.brightLabel.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float e = Mathf.Clamp01(t);
            it.darkLabel.color   = Color.Lerp(darkStart,   darkTarget,   e);
            it.brightLabel.color = Color.Lerp(brightStart, brightTarget, e);
            yield return null;
        }

        it.darkLabel.color   = darkTarget;
        it.brightLabel.color = brightTarget;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, float delay = 0f)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }

        cg.alpha = to;
    }



    void RestartCoroutine(ref Coroutine co, IEnumerator routine)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(routine);
    }

    static void SetHighlightScale(MenuItem it, float sx)
    {
        var r = it.highlight.rectTransform;
        r.localScale = new Vector3(sx, r.localScale.y, 1f);
    }

    static CanvasGroup GetOrAddCanvasGroup(RectTransform rt)
    {
        var cg = rt.GetComponent<CanvasGroup>();
        if (!cg) cg = rt.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }

    static Vector3 EvalKeyframes((float t, Vector3 scale)[] keys, float t)
    {
        for (int i = 0; i < keys.Length - 1; i++)
        {
            if (t >= keys[i].t && t <= keys[i + 1].t)
            {
                float span   = keys[i + 1].t - keys[i].t;
                float local  = (t - keys[i].t) / span;
                float eased  = EaseSpring(local);
                return Vector3.Lerp(keys[i].scale, keys[i + 1].scale, eased);
            }
        }

        return keys[keys.Length - 1].scale;
    }
}
