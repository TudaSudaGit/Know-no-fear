using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DiceRollPanel : MonoBehaviour
{
    public static DiceRollPanel Instance { get; private set; }

    private Transform panelRoot;
    private List<CardData> activeCards = new List<CardData>();

    private const int   MAX_CARDS = 7;
    private const float CARD_W    = 276f;
    private const float CARD_H    = 144f;
    private const float CARD_GAP  = 9f;
    private const float CARD_X    = 14f;
    private const float CARD_Y    = 14f;
    private const float SLIDE_DUR = 0.16f;
    private const float ROLL_DUR  = 0.50f;
    private const float PAUSE     = 0.28f;

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

        GameObject go = new GameObject("_DiceCanvas");
        Canvas cv = go.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 999;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        go.AddComponent<GraphicRaycaster>();
        panelRoot = go.transform;
    }

    public static void EnsureExists()
    {
        if (Instance != null) return;
        new GameObject("DiceRollPanel").AddComponent<DiceRollPanel>();
    }

    public static void Request(CombatRequest req)
    {
        EnsureExists();
        if (Instance != null) Instance.StartCoroutine(Instance.ResolveOne(req));
    }

    public void RequestCombat(CombatRequest req)
    {
        StartCoroutine(ResolveOne(req));
    }

    IEnumerator ResolveOne(CombatRequest req)
    {
        int hitSkill  = HitSkill(req);
        int strength  = Str(req);
        int toughness = Tough(req);
        int modSave   = Mathf.Clamp(ModSave(req), 2, 7);
        int dmg       = Dmg(req);

        int hitRoll   = Random.Range(1, 7);
        int woundRoll = Random.Range(1, 7);
        int saveRoll  = Random.Range(1, 7);

        bool hit      = hitRoll   >= hitSkill;
        bool wound    = hit   && woundRoll >= WoundThreshold(strength, toughness);
        bool saved    = wound && modSave <= 6 && saveRoll >= modSave;
        bool dmgDone  = wound && !saved;

        Debug.Log($"[Dice] hit={hit}({hitRoll}>={hitSkill})  wound={wound}({woundRoll}>={WoundThreshold(strength,toughness)})  save={saved}({saveRoll}>={modSave})  dmg={dmgDone}");

        if (dmgDone && req.defenderHealth != null)
        {
            req.defenderHealth.ApplyDamage(dmg);
            Debug.Log($"[Dice] ApplyDamage({dmg}) -> {req.defenderHealth.gameObject.name}");
        }
        else if (dmgDone)
        {
            Debug.LogWarning("[Dice] dmgDone=true но defenderHealth == null!");
        }

        CardData cd = SpawnCard(req);
        yield return StartCoroutine(FadeIn(cd));

        yield return StartCoroutine(AnimateDie(cd.dice[0], hitRoll));
        FinishDie(cd.dice[0], hit);
        yield return new WaitForSeconds(PAUSE);

        if (!hit)
        {
            ShowFinal(cd, "ПРОМАХ", C_FAIL);
            yield return new WaitForSeconds(1.8f);
            yield return StartCoroutine(FadeOut(cd));
            yield break;
        }

        yield return StartCoroutine(AnimateDie(cd.dice[1], woundRoll));
        FinishDie(cd.dice[1], wound);
        yield return new WaitForSeconds(PAUSE);

        if (!wound)
        {
            ShowFinal(cd, "БЕЗ РАНЫ", C_INFO);
            yield return new WaitForSeconds(1.8f);
            yield return StartCoroutine(FadeOut(cd));
            yield break;
        }

        yield return StartCoroutine(AnimateDie(cd.dice[2], saveRoll));
        FinishDie(cd.dice[2], saved);
        yield return new WaitForSeconds(PAUSE);

        ShowFinal(cd, saved ? "СОХРАНЁН" : $"РАНА -{dmg}", saved ? C_INFO : C_FAIL);

        yield return new WaitForSeconds(2.2f);
        yield return StartCoroutine(FadeOut(cd));
    }

    CardData SpawnCard(CombatRequest req)
    {
        if (activeCards.Count >= MAX_CARDS)
        {
            CardData old = activeCards[activeCards.Count - 1];
            activeCards.RemoveAt(activeCards.Count - 1);
            if (old.root != null) Destroy(old.root);
        }

        float step = CARD_H + CARD_GAP;
        for (int i = 0; i < activeCards.Count; i++)
        {
            if (activeCards[i].root == null) continue;
            RectTransform ert = activeCards[i].root.GetComponent<RectTransform>();
            Vector2 tgt = new Vector2(CARD_X, -(CARD_Y + (i + 1) * step));
            StartCoroutine(SlideCard(ert, tgt));
        }

        CardData cd      = BuildCard(req);
        RectTransform rt = cd.root.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(0, 1);
        rt.pivot            = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(CARD_X, -CARD_Y);
        rt.sizeDelta        = new Vector2(CARD_W, CARD_H);

        activeCards.Insert(0, cd);
        return cd;
    }

    CardData BuildCard(CombatRequest req)
    {
        CardData cd     = new CardData();
        Color accentCol = req.attackerIsPlayer ? C_PLAYER : C_ENEMY;

        cd.root = new GameObject("DiceCard");
        cd.root.transform.SetParent(panelRoot, false);

        cd.root.AddComponent<Image>().color = C_BG;
        cd.group       = cd.root.AddComponent<CanvasGroup>();
        cd.group.alpha = 0f;

        RectTransform rootRt = cd.root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(CARD_W, CARD_H);

        MkImg(cd.root.transform, "Accent", accentCol,
            new Vector2(0,0), new Vector2(0,1),
            new Vector2(0,.5f), Vector2.zero, new Vector2(5,0));

        cd.title = MkTxt(cd.root.transform, "Title", TitleStr(req), 12f, FontStyles.Bold, Color.white,
            new Vector2(0,1), new Vector2(1,1),
            new Vector2(.5f,1f), new Vector2(5f,-9f), new Vector2(-18f,22f));
        cd.title.alignment = TextAlignmentOptions.Left;

        MkImg(cd.root.transform, "Sep", new Color(1,1,1,.12f),
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

        Image dieImg = MkImg(parent, "D_"+label, isPlayer ? C_PLAYER : C_ENEMY,
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

        dv.result = MkTxt(parent, "R_"+label, "", 11f, FontStyles.Bold, Color.white,
            new Vector2(0,1), new Vector2(0,1),
            new Vector2(.5f,1f), new Vector2(cx,-100f), new Vector2(82f,16f));
        dv.result.alignment = TextAlignmentOptions.Center;

        return dv;
    }

    string TitleStr(CombatRequest req)
    {
        if (req.attackerIsPlayer) return req.isMelee ? "УДАР ИГРОКА" : "ВЫСТРЕЛ ИГРОКА";
        return req.isMelee ? "АТАКА ВРАГА" : "ОБСТРЕЛ ВРАГА";
    }

    IEnumerator SlideCard(RectTransform rt, Vector2 target)
    {
        if (rt == null) yield break;
        Vector2 start = rt.anchoredPosition;
        float t = 0f;
        while (t < SLIDE_DUR && rt != null)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, target, t / SLIDE_DUR);
            yield return null;
        }
        if (rt) rt.anchoredPosition = target;
    }

    IEnumerator FadeIn(CardData cd)
    {
        float t = 0f;
        while (t < 0.20f)
        {
            t += Time.deltaTime;
            if (cd.group) cd.group.alpha = Mathf.Clamp01(t / 0.20f);
            yield return null;
        }
        if (cd.group) cd.group.alpha = 1f;
    }

    IEnumerator FadeOut(CardData cd)
    {
        float t = 0f;
        while (t < 0.25f && cd.group != null)
        {
            t += Time.deltaTime;
            if (cd.group) cd.group.alpha = 1f - Mathf.Clamp01(t / 0.25f);
            yield return null;
        }
        activeCards.Remove(cd);
        for (int i = 0; i < activeCards.Count; i++)
        {
            if (activeCards[i].root == null) continue;
            RectTransform ert = activeCards[i].root.GetComponent<RectTransform>();
            Vector2 tgt = new Vector2(CARD_X, -(CARD_Y + i * (CARD_H + CARD_GAP)));
            StartCoroutine(SlideCard(ert, tgt));
        }
        if (cd.root) Destroy(cd.root);
    }

    IEnumerator AnimateDie(DiceView dv, int finalRoll)
    {
        dv.roll = finalRoll;
        float e = 0f;
        while (e < ROLL_DUR)
        {
            if (dv.num) dv.num.text = Random.Range(1, 7).ToString();
            yield return new WaitForSeconds(0.05f);
            e += 0.05f;
        }
        if (dv.num) dv.num.text = finalRoll.ToString();
    }

    void FinishDie(DiceView dv, bool success)
    {
        if (dv.result) { dv.result.text = success ? "OK" : "--"; dv.result.color = success ? C_SUCCESS : C_FAIL; }
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

    int HitSkill(CombatRequest r) { if (r.attacker == null) return 4; return r.isMelee ? r.attacker.weaponSkill : r.attacker.ballisticSkill; }
    int Str(CombatRequest r)      => r.attacker != null ? r.attacker.strength         : 4;
    int Tough(CombatRequest r)    => r.defender != null ? r.defender.toughness         : 4;
    int ModSave(CombatRequest r)  => (r.defender != null ? r.defender.save : 5) + (r.attacker != null ? r.attacker.armorPenetration : 0);
    int Dmg(CombatRequest r)      => r.attacker != null ? r.attacker.damage            : 1;

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
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt    = go.GetComponent<RectTransform>();
        rt.anchorMin        = ancMin;
        rt.anchorMax        = ancMax;
        rt.pivot            = piv;
        rt.anchoredPosition = aPos;
        rt.sizeDelta        = size;
        return img;
    }

    TextMeshProUGUI MkTxt(Transform parent, string name, string text,
        float size, FontStyles style, Color color,
        Vector2 ancMin, Vector2 ancMax, Vector2 piv, Vector2 aPos, Vector2 sz)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = color;
        RectTransform rt    = go.GetComponent<RectTransform>();
        rt.anchorMin        = ancMin;
        rt.anchorMax        = ancMax;
        rt.pivot            = piv;
        rt.anchoredPosition = aPos;
        rt.sizeDelta        = sz;
        return tmp;
    }
}
