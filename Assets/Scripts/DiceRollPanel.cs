using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DiceRollPanel : MonoBehaviour
{
    public static DiceRollPanel Instance { get; private set; }

    private RectTransform cardContainer;
    private List<CardData> activeCards  = new List<CardData>();
    private Queue<CombatRequest> queue  = new Queue<CombatRequest>();
    private bool processing;

    private const int   MAX_CARDS = 7;
    private const float CARD_W    = 276f;
    private const float CARD_H    = 144f;
    private const float ROLL_DUR  = 0.55f;
    private const float PAUSE     = 0.30f;

    private static readonly Color C_PLAYER  = new Color(1.00f, 0.84f, 0.05f, 1f);
    private static readonly Color C_ENEMY   = new Color(0.52f, 0.20f, 0.84f, 1f);
    private static readonly Color C_BG      = new Color(0.06f, 0.06f, 0.12f, 0.96f);
    private static readonly Color C_SUCCESS = new Color(0.18f, 0.90f, 0.44f, 1f);
    private static readonly Color C_FAIL    = new Color(0.92f, 0.22f, 0.22f, 1f);
    private static readonly Color C_INFO    = new Color(0.35f, 0.70f, 1.00f, 1f);

    public struct CombatRequest
    {
        public UnitStats attacker;
        public UnitStats defender;
        public Health    defenderHealth;
        public bool      attackerIsPlayer;
        public bool      isMelee;
    }

    private class DiceView
    {
        public Image           bg;
        public TextMeshProUGUI num;
        public TextMeshProUGUI result;
        public int             roll;
    }

    private class CardData
    {
        public GameObject      root;
        public CanvasGroup     group;
        public TextMeshProUGUI title;
        public DiceView[]      dice = new DiceView[3];
        public TextMeshProUGUI final;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateContainer();
    }

    public static void EnsureExists()
    {
        if (Instance != null) return;
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[DiceRollPanel] Canvas не найден в сцене!"); return; }
        canvas.gameObject.AddComponent<DiceRollPanel>();
        Debug.Log("[DiceRollPanel] Авто-создан на Canvas.");
    }

    void CreateContainer()
    {
        GameObject go = new GameObject("_DiceCards");
        go.transform.SetParent(transform, false);
        go.AddComponent<CanvasRenderer>();

        RectTransform rt    = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(14f, -14f);
        rt.sizeDelta        = new Vector2(CARD_W, 0f);
        cardContainer       = rt;

        VerticalLayoutGroup vlg    = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing                = 9f;
        vlg.childAlignment         = TextAnchor.UpperLeft;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit       = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void RequestCombat(CombatRequest req)
    {
        queue.Enqueue(req);
        if (!processing) StartCoroutine(ProcessQueue());
    }

    public static void Request(CombatRequest req)
    {
        EnsureExists();
        if (Instance != null) Instance.RequestCombat(req);
    }

    IEnumerator ProcessQueue()
    {
        processing = true;
        while (queue.Count > 0)
            yield return StartCoroutine(ResolveOne(queue.Dequeue()));
        processing = false;
    }

    IEnumerator ResolveOne(CombatRequest req)
    {
        TrimCards();
        CardData cd = BuildCard(req);
        yield return StartCoroutine(FadeIn(cd));

        int hitSkill  = HitSkill(req);
        int strength  = Str(req);
        int toughness = Tough(req);
        int modSave   = ModSave(req);
        int dmg       = Dmg(req);

        yield return StartCoroutine(RollDie(cd.dice[0]));
        bool hit = cd.dice[0].roll >= hitSkill;
        FinishDie(cd.dice[0], hit);
        yield return new WaitForSeconds(PAUSE);

        if (!hit)
        {
            ShowFinal(cd, "ПРОМАХ", C_FAIL);
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(FadeOut(cd));
            yield break;
        }

        int woundNeed = WoundThreshold(strength, toughness);
        yield return StartCoroutine(RollDie(cd.dice[1]));
        bool wound = cd.dice[1].roll >= woundNeed;
        FinishDie(cd.dice[1], wound);
        yield return new WaitForSeconds(PAUSE);

        if (!wound)
        {
            ShowFinal(cd, "БЕЗ РАНЫ", C_INFO);
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(FadeOut(cd));
            yield break;
        }

        yield return StartCoroutine(RollDie(cd.dice[2]));
        bool saved = cd.dice[2].roll >= modSave;
        FinishDie(cd.dice[2], saved);
        yield return new WaitForSeconds(PAUSE);

        if (saved)
        {
            ShowFinal(cd, "СОХРАНЁН", C_INFO);
        }
        else
        {
            if (req.defenderHealth != null)
                req.defenderHealth.ApplyDamage(dmg);
            ShowFinal(cd, $"РАНА  \u2212{dmg}", C_FAIL);
        }

        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(FadeOut(cd));
    }

    int HitSkill(CombatRequest r)
    {
        if (r.attacker == null) return 4;
        return r.isMelee ? r.attacker.weaponSkill : r.attacker.ballisticSkill;
    }
    int Str(CombatRequest r)   => r.attacker != null ? r.attacker.strength : 4;
    int Tough(CombatRequest r) => r.defender != null ? r.defender.toughness : 4;
    int ModSave(CombatRequest r)
    {
        int sv = r.defender != null ? r.defender.save           : 5;
        int ap = r.attacker != null ? r.attacker.armorPenetration : 0;
        return sv + ap;
    }
    int Dmg(CombatRequest r) => r.attacker != null ? r.attacker.damage : 1;

    void TrimCards()
    {
        while (activeCards.Count >= MAX_CARDS)
        {
            CardData old = activeCards[0];
            activeCards.RemoveAt(0);
            if (old.root != null) Destroy(old.root);
        }
    }

    CardData BuildCard(CombatRequest req)
    {
        CardData cd     = new CardData();
        Color accentCol = req.attackerIsPlayer ? C_PLAYER : C_ENEMY;

        cd.root = new GameObject("DiceCard");
        cd.root.transform.SetParent(cardContainer, false);

        cd.group       = cd.root.AddComponent<CanvasGroup>();
        cd.group.alpha = 0f;
        cd.root.AddComponent<CanvasRenderer>();

        RectTransform rootRt = cd.root.AddComponent<RectTransform>();
        rootRt.sizeDelta     = new Vector2(CARD_W, CARD_H);

        LayoutElement le   = cd.root.AddComponent<LayoutElement>();
        le.preferredWidth  = CARD_W;
        le.preferredHeight = CARD_H;

        Image bg = cd.root.AddComponent<Image>();
        bg.color = C_BG;

        MkImg(cd.root.transform, "Accent", accentCol,
            new Vector2(0,0), new Vector2(0,1),
            new Vector2(0,.5f), Vector2.zero, new Vector2(5,0));

        cd.title = MkTxt(cd.root.transform, "Title", TitleStr(req), 12f, FontStyles.Bold, Color.white,
            new Vector2(0,1), new Vector2(1,1),
            new Vector2(.5f,1f), new Vector2(5f,-9f), new Vector2(-18f,22f));
        cd.title.alignment = TextAlignmentOptions.Left;

        MkImg(cd.root.transform, "Sep", new Color(1,1,1,.13f),
            new Vector2(.02f,1f), new Vector2(.98f,1f),
            new Vector2(.5f,1f), new Vector2(0,-33f), new Vector2(0,1f));

        float[] xs   = { CARD_W * .17f, CARD_W * .50f, CARD_W * .83f };
        string[] lbs = { "ПОПАДАНИЕ", "РАНА", "БРОНЯ" };
        bool[]   plr = { req.attackerIsPlayer, req.attackerIsPlayer, !req.attackerIsPlayer };

        for (int i = 0; i < 3; i++)
            cd.dice[i] = BuildDie(cd.root.transform, xs[i], lbs[i], plr[i]);

        cd.final = MkTxt(cd.root.transform, "Final", "...", 11f, FontStyles.Bold, new Color(1,1,1,.38f),
            new Vector2(0,0), new Vector2(1,0),
            new Vector2(.5f,0f), new Vector2(0,9f), new Vector2(-10f,20f));
        cd.final.alignment = TextAlignmentOptions.Center;

        activeCards.Add(cd);
        return cd;
    }

    DiceView BuildDie(Transform parent, float cx, string label, bool isPlayer)
    {
        DiceView dv = new DiceView();

        TextMeshProUGUI lbl = MkTxt(parent, "L_"+label, label, 8f, FontStyles.Bold,
            new Color(1,1,1,.45f),
            new Vector2(0,1), new Vector2(0,1),
            new Vector2(.5f,1f), new Vector2(cx,-37f), new Vector2(82f,14f));
        lbl.alignment = TextAlignmentOptions.Center;

        Color dieColor = isPlayer ? C_PLAYER : C_ENEMY;
        Image dieImg   = MkImg(parent, "D_"+label, dieColor,
            new Vector2(0,1), new Vector2(0,1),
            new Vector2(.5f,1f), new Vector2(cx,-53f), new Vector2(44f,44f));
        dv.bg = dieImg;

        Outline ol        = dieImg.gameObject.AddComponent<Outline>();
        ol.effectColor    = new Color(0,0,0,.75f);
        ol.effectDistance = new Vector2(2f,-2f);

        Color numColor = isPlayer ? new Color(.08f,.06f,0f) : Color.white;
        dv.num = MkTxt(dieImg.transform, "Num", "?", 22f, FontStyles.Bold, numColor,
            Vector2.zero, Vector2.one,
            new Vector2(.5f,.5f), Vector2.zero, Vector2.zero);
        dv.num.alignment = TextAlignmentOptions.Center;

        dv.result = MkTxt(parent, "R_"+label, "", 13f, FontStyles.Bold, Color.white,
            new Vector2(0,1), new Vector2(0,1),
            new Vector2(.5f,1f), new Vector2(cx,-100f), new Vector2(82f,16f));
        dv.result.alignment = TextAlignmentOptions.Center;

        return dv;
    }

    string TitleStr(CombatRequest req)
    {
        if (req.attackerIsPlayer) return req.isMelee ? "УДАР  ИГРОКА" : "ВЫСТРЕЛ  ИГРОКА";
        return req.isMelee ? "АТАКА  ВРАГА" : "ОБСТРЕЛ  ВРАГА";
    }

    IEnumerator FadeIn(CardData cd)
    {
        float t = 0f;
        while (t < 0.22f)
        {
            t += Time.deltaTime;
            if (cd.group) cd.group.alpha = Mathf.Clamp01(t / 0.22f);
            yield return null;
        }
        if (cd.group) cd.group.alpha = 1f;
    }

    IEnumerator FadeOut(CardData cd)
    {
        float t = 0f;
        while (t < 0.28f && cd.group)
        {
            t += Time.deltaTime;
            cd.group.alpha = 1f - Mathf.Clamp01(t / 0.28f);
            yield return null;
        }
        activeCards.Remove(cd);
        if (cd.root) Destroy(cd.root);
    }

    IEnumerator RollDie(DiceView dv)
    {
        dv.roll = Random.Range(1, 7);
        float e = 0f;
        while (e < ROLL_DUR)
        {
            if (dv.num) dv.num.text = Random.Range(1, 7).ToString();
            yield return new WaitForSeconds(0.05f);
            e += 0.05f;
        }
        if (dv.num) dv.num.text = dv.roll.ToString();
    }

    void FinishDie(DiceView dv, bool success)
    {
        if (dv.result)
        {
            dv.result.text  = success ? "\u2713" : "\u2717";
            dv.result.color = success ? C_SUCCESS : C_FAIL;
        }
        if (!success)
        {
            if (dv.bg)  { Color c = dv.bg.color;  dv.bg.color  = new Color(c.r,c.g,c.b,.20f); }
            if (dv.num) { Color n = dv.num.color; dv.num.color = new Color(n.r,n.g,n.b,.20f); }
        }
    }

    void ShowFinal(CardData cd, string text, Color color)
    {
        if (cd.final) { cd.final.text = text; cd.final.color = color; }
    }

    int WoundThreshold(int str, int tough)
    {
        if (str >= tough * 2) return 2;
        if (str > tough)      return 3;
        if (str == tough)     return 4;
        if (str * 2 <= tough) return 6;
        return 5;
    }

    Image MkImg(Transform parent, string name, Color color,
        Vector2 ancMin, Vector2 ancMax, Vector2 piv, Vector2 aPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        RectTransform rt    = go.AddComponent<RectTransform>();
        rt.anchorMin        = ancMin;
        rt.anchorMax        = ancMax;
        rt.pivot            = piv;
        rt.anchoredPosition = aPos;
        rt.sizeDelta        = size;
        Image img  = go.AddComponent<Image>();
        img.color  = color;
        return img;
    }

    TextMeshProUGUI MkTxt(Transform parent, string name, string text,
        float size, FontStyles style, Color color,
        Vector2 ancMin, Vector2 ancMax, Vector2 piv, Vector2 aPos, Vector2 sz)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        RectTransform rt    = go.AddComponent<RectTransform>();
        rt.anchorMin        = ancMin;
        rt.anchorMax        = ancMax;
        rt.pivot            = piv;
        rt.anchoredPosition = aPos;
        rt.sizeDelta        = sz;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = color;
        return tmp;
    }
}
