using UnityEngine;
using UnityEngine.UI;

public class CurseVignette : MonoBehaviour
{
    private Image[] borders = new Image[4];
    private float pulseTimer = 0f;

    private static readonly Color BORDER_COLOR = new Color(0.45f, 0.05f, 0.85f, 0f);
    private const float BORDER_SIZE = 40f;
    private const float MAX_ALPHA   = 0.55f;
    private const float MIN_ALPHA   = 0.30f;

    void Awake()
    {
        GameObject canvasGO = new GameObject("_CurseVignetteCanvas");
        DontDestroyOnLoad(canvasGO);

        Canvas cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 998;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        borders[0] = MakeBorder(canvasGO.transform, "L",
            new Vector2(0,0), new Vector2(0,1), new Vector2(0,0.5f),
            new Vector2(BORDER_SIZE, 0), Vector2.zero);

        borders[1] = MakeBorder(canvasGO.transform, "R",
            new Vector2(1,0), new Vector2(1,1), new Vector2(1,0.5f),
            new Vector2(BORDER_SIZE, 0), Vector2.zero);

        borders[2] = MakeBorder(canvasGO.transform, "T",
            new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1),
            new Vector2(0, BORDER_SIZE), Vector2.zero);

        borders[3] = MakeBorder(canvasGO.transform, "B",
            new Vector2(0,0), new Vector2(1,0), new Vector2(0.5f,0),
            new Vector2(0, BORDER_SIZE), Vector2.zero);
    }

    Image MakeBorder(Transform parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos)
    {
        GameObject go = new GameObject("Border_" + name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = new Color(BORDER_COLOR.r, BORDER_COLOR.g, BORDER_COLOR.b, 0f);
        img.raycastTarget = false;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = ancMin;
        rt.anchorMax        = ancMax;
        rt.pivot            = pivot;
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = anchoredPos;

        return img;
    }

    void Update()
    {
        bool active = PlayerCurseHandler.Instance != null && PlayerCurseHandler.Instance.AnyCurseActive;

        float targetAlpha;
        if (active)
        {
            pulseTimer += Time.deltaTime * 3f;
            targetAlpha = MIN_ALPHA + (Mathf.Sin(pulseTimer) * 0.5f + 0.5f) * (MAX_ALPHA - MIN_ALPHA);
        }
        else
        {
            pulseTimer  = 0f;
            targetAlpha = 0f;
        }

        for (int i = 0; i < borders.Length; i++)
        {
            if (borders[i] == null) continue;
            Color c = borders[i].color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 8f);
            borders[i].color = c;
        }
    }
}
