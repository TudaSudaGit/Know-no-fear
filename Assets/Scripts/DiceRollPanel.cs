// =====================================================
// DiceRollPanel.cs
// =====================================================
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
    private const int MAX_CARDS = 7;
    private const float CARD_W = 276f, CARD_H = 144f, CARD_GAP = 9f, CARD_X = 14f, CARD_Y = 14f, SLIDE_DUR = 0.16f, ROLL_DUR = 0.50f, PAUSE = 0.28f;

    // Оригинальные цвета и константы из предоставленного дизайна
    private static readonly Color C_PLAYER = new Color(1.00f, 0.84f, 0.05f, 1f); // Желтый акцент игрока
    private static readonly Color C_ENEMY = new Color(0.52f, 0.20f, 0.84f, 1f);  // Фиолетовый акцент врага
    private static readonly Color C_BG = new Color(0.06f, 0.06f, 0.12f, 0.96f);     // Темный фон карточки
    private static readonly Color C_SUCCESS = new Color(0.18f, 0.90f, 0.44f, 1f);  // Зеленый успех
    private static readonly Color C_FAIL = new Color(0.92f, 0.22f, 0.22f, 1f);     // Красная неудача
    private static readonly Color C_INFO = new Color(0.35f, 0.70f, 1.00f, 1f);

    // Счётчики для корректировки рандома игрока
    private static int consecutiveFailures = 0;
    private static int consecutiveWoundFailures = 0;

    public struct CombatRequest { public UnitStats attacker, defender; public Health defenderHealth; public bool attackerIsPlayer, isMelee; }
    private class DiceView { public Image bg; public TextMeshProUGUI num, result; public int roll; }
    private class CardData { public GameObject root; public CanvasGroup group; public TextMeshProUGUI title, final; public DiceView[] dice = new DiceView[3]; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GameObject go = new GameObject("_DiceCanvas");
        Canvas cv = go.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 999;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        go.AddComponent<GraphicRaycaster>();
        panelRoot = go.transform;
    }

    public static void EnsureExists() { if (Instance != null) return; new GameObject("DiceRollPanel").AddComponent<DiceRollPanel>(); }
    public static void Request(CombatRequest req) { EnsureExists(); if (Instance != null) Instance.StartCoroutine(Instance.ResolveOne(req)); }
    public void RequestCombat(CombatRequest req) { StartCoroutine(ResolveOne(req)); }

    private int RollDie(bool isPlayerRoll)
    {
        // Бросок врага всегда честный
        if (!isPlayerRoll) return Random.Range(1, 7);

        GameSettings.LoadOptions();
        int difficulty = GameSettings.Difficulty;

        // Честный бросок 1D6 для игрока (по 16.6% на каждую грань)
        int roll = Random.Range(1, 7);

        // Легкая сложность: 50% шанс автоматически перебросить выпавшую единицу
        if (difficulty == 0 && roll == 1 && Random.value > 0.5f)
        {
            roll = Random.Range(2, 7);
        }
        // Высокая сложность: 50% шанс, что игра заставит перебросить шестерку
        else if (difficulty == 2 && roll == 6 && Random.value > 0.5f)
        {
            roll = Random.Range(1, 6);
        }

        // Мягкая помощь при серии неудач:
        // Если игрок промахнулся 2+ раза подряд и выкинул мало, слегка подталкиваем результат (+1)
        if (consecutiveFailures >= 2 && roll <= 3)
        {
            roll += 1;
        }

        return Mathf.Clamp(roll, 1, 6);
    }

    IEnumerator ResolveOne(CombatRequest req)
    {
        int hitSkill = HitSkill(req), strength = Str(req), toughness = Tough(req), modSave = Mathf.Clamp(ModSave(req), 2, 7), dmg = Dmg(req);

        int hitRoll = RollDie(req.attackerIsPlayer);
        int woundRoll = RollDie(req.attackerIsPlayer);

        // Гарантированное 4-е пробитие для игрока (Pity Timer)
        if (req.attackerIsPlayer && consecutiveWoundFailures >= 3)
        {
            woundRoll = 6;
        }

        int saveRoll = Random.Range(1, 7);

        bool hit = hitRoll >= hitSkill;
        bool wound = hit && woundRoll >= WoundThreshold(strength, toughness);
        bool saved = wound && modSave <= 6 && saveRoll >= modSave;
        bool dmgDone = wound && !saved;

        // МГНОВЕННОЕ ВЛИЯНИЕ НА ИГРУ (до запуска анимаций карточек)
        if (dmgDone && req.defender != null)
        {
            req.defender.TakeDamage(dmg);
            if (req.defenderHealth != null) req.defenderHealth.ApplyDamage(dmg);
        }

        // Корректировка счетчиков неудач игрока
        if (req.attackerIsPlayer)
        {
            if (dmgDone) consecutiveFailures = 0;
            else consecutiveFailures++;

            if (hit)
            {
                if (wound) consecutiveWoundFailures = 0;
                else consecutiveWoundFailures++;
            }
        }

        bool hitGood = hit == req.attackerIsPlayer, woundGood = wound == req.attackerIsPlayer, saveGood = saved != req.attackerIsPlayer;
        CardData cd = SpawnCard(req);

        yield return StartCoroutine(FadeIn(cd));

        // 1. Попадание
        yield return StartCoroutine(AnimateDie(cd.dice[0], hitRoll));
        FinishDie(cd.dice[0], hit, hitGood);
        yield return new WaitForSeconds(PAUSE);
        if (!hit) { ShowFinal(cd, "ПРОМАХ", req.attackerIsPlayer ? C_FAIL : C_SUCCESS); yield return new WaitForSeconds(1.8f); yield return StartCoroutine(FadeOut(cd)); yield break; }

        // 2. Рана (Пробитие)
        yield return StartCoroutine(AnimateDie(cd.dice[1], woundRoll));
        FinishDie(cd.dice[1], wound, woundGood);
        yield return new WaitForSeconds(PAUSE);
        if (!wound) { ShowFinal(cd, "БЕЗ РАНЫ", req.attackerIsPlayer ? C_FAIL : C_SUCCESS); yield return new WaitForSeconds(1.8f); yield return StartCoroutine(FadeOut(cd)); yield break; }

        // 3. Броня (Сейв)
        yield return StartCoroutine(AnimateDie(cd.dice[2], saveRoll));
        FinishDie(cd.dice[2], saved, saveGood);
        yield return new WaitForSeconds(PAUSE);

        if (saved) ShowFinal(cd, "СОХРАНЁН", req.attackerIsPlayer ? C_FAIL : C_SUCCESS);
        else ShowFinal(cd, $"РАНА -{dmg}", req.attackerIsPlayer ? C_SUCCESS : C_FAIL);

        yield return new WaitForSeconds(2.2f);
        yield return StartCoroutine(FadeOut(cd));
    }

    CardData SpawnCard(CombatRequest req)
    {
        if (activeCards.Count >= MAX_CARDS) { CardData old = activeCards[activeCards.Count - 1]; activeCards.RemoveAt(activeCards.Count - 1); if (old.root != null) Destroy(old.root); }
        float step = CARD_H + CARD_GAP;
        for (int i = 0; i < activeCards.Count; i++)
        {
            if (activeCards[i].root == null) continue;
            RectTransform ert = activeCards[i].root.GetComponent<RectTransform>();
            Vector2 tgt = new Vector2(CARD_X, -(CARD_Y + (i + 1) * step));
            StartCoroutine(SlideCard(ert, tgt));
        }
        CardData cd = BuildCard(req);
        RectTransform rt = cd.root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(CARD_X, -CARD_Y); rt.sizeDelta = new Vector2(CARD_W, CARD_H);
        activeCards.Insert(0, cd);
        return cd;
    }

    CardData BuildCard(CombatRequest req)
    {
        CardData cd = new CardData();
        Color accentCol = req.attackerIsPlayer ? C_PLAYER : C_ENEMY;
        cd.root = new GameObject("DiceCard");
        cd.root.transform.SetParent(panelRoot, false);
        cd.root.AddComponent<Image>().color = C_BG;
        cd.group = cd.root.AddComponent<CanvasGroup>();
        cd.group.alpha = 0f;
        RectTransform rootRt = cd.root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(CARD_W, CARD_H);

        MkImg(cd.root.transform, "Accent", accentCol, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, .5f), Vector2.zero, new Vector2(5, 0));
        cd.title = MkTxt(cd.root.transform, "Title", TitleStr(req), 12f, FontStyles.Bold, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(.5f, 1f), new Vector2(5f, -9f), new Vector2(-18f, 22f));
        cd.title.alignment = TextAlignmentOptions.Left;

        MkImg(cd.root.transform, "Sep", new Color(1, 1, 1, .12f), new Vector2(.02f, 1f), new Vector2(.98f, 1f), new Vector2(.5f, 1f), new Vector2(0, -33f), new Vector2(0, 1f));

        float[] xs = { CARD_W * .17f, CARD_W * .50f, CARD_W * .83f };
        string[] lbs = { "ПОПАДАНИЕ", "РАНА", "БРОНЯ" };
        bool[] plr = { req.attackerIsPlayer, req.attackerIsPlayer, !req.attackerIsPlayer };
        for (int i = 0; i < 3; i++) cd.dice[i] = BuildDie(cd.root.transform, xs[i], lbs[i], plr[i]);

        cd.final = MkTxt(cd.root.transform, "Final", "...", 11f, FontStyles.Bold, new Color(1, 1, 1, .38f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(.5f, 0f), new Vector2(0, 9f), new Vector2(-10f, 20f));
        cd.final.alignment = TextAlignmentOptions.Center;
        return cd;
    }

    DiceView BuildDie(Transform parent, float cx, string label, bool isPlayer)
    {
        DiceView dv = new DiceView();
        TextMeshProUGUI lbl = MkTxt(parent, "L_" + label, label, 8f, FontStyles.Bold, new Color(1, 1, 1, .45f), new Vector2(0, 1), new Vector2(0, 1), new Vector2(.5f, 1f), new Vector2(cx, -37f), new Vector2(82f, 14f));
        lbl.alignment = TextAlignmentOptions.Center;

        Image dieImg = MkImg(parent, "D_" + label, isPlayer ? C_PLAYER : C_ENEMY, new Vector2(0, 1), new Vector2(0, 1), new Vector2(.5f, 1f), new Vector2(cx, -53f), new Vector2(44f, 44f));
        dv.bg = dieImg;

        Outline ol = dieImg.gameObject.AddComponent<Outline>();
        ol.effectColor = new Color(0, 0, 0, .75f);
        ol.effectDistance = new Vector2(2f, -2f);

        Color numColor = isPlayer ? new Color(.08f, .06f, 0f) : Color.white;
        dv.num = MkTxt(dieImg.transform, "Num", "?", 22f, FontStyles.Bold, numColor, Vector2.zero, Vector2.one, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
        dv.num.alignment = TextAlignmentOptions.Center;

        dv.result = MkTxt(parent, "R_" + label, "", 11f, FontStyles.Bold, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(.5f, 1f), new Vector2(cx, -100f), new Vector2(82f, 16f));
        dv.result.alignment = TextAlignmentOptions.Center;
        return dv;
    }

    string TitleStr(CombatRequest req)
    {
        string t = req.attackerIsPlayer ? (req.isMelee ? "УДАР ИГРОКА" : "ВЫСТРЕЛ ИГРОКА") : (req.isMelee ? "АТАКА ВРАГА" : "ОБСТРЕЛ ВРАГА");
        if (req.defender != null && req.defender.armorPoints > 0) t += $" (Броня: {req.defender.armorPoints})";
        else if (req.defender != null) t += $" (HP: {req.defender.wounds})";
        return t;
    }

    IEnumerator SlideCard(RectTransform rt, Vector2 target)
    { if (rt == null) yield break; Vector2 start = rt.anchoredPosition; float t = 0f; while (t < SLIDE_DUR && rt != null) { t += Time.deltaTime; rt.anchoredPosition = Vector2.Lerp(start, target, t / SLIDE_DUR); yield return null; } if (rt) rt.anchoredPosition = target; }

    IEnumerator FadeIn(CardData cd) { float t = 0f; while (t < 0.20f) { t += Time.deltaTime; if (cd.group) cd.group.alpha = Mathf.Clamp01(t / 0.20f); yield return null; } if (cd.group) cd.group.alpha = 1f; }

    IEnumerator FadeOut(CardData cd)
    {
        float t = 0f;
        while (t < 0.25f && cd.group != null) { t += Time.deltaTime; if (cd.group) cd.group.alpha = 1f - Mathf.Clamp01(t / 0.25f); yield return null; }
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

    IEnumerator AnimateDie(DiceView dv, int finalRoll) { dv.roll = finalRoll; float e = 0f; while (e < ROLL_DUR) { if (dv.num) dv.num.text = Random.Range(1, 7).ToString(); yield return new WaitForSeconds(0.05f); e += 0.05f; } if (dv.num) dv.num.text = finalRoll.ToString(); }

    void FinishDie(DiceView dv, bool rollSucceeded, bool isGood)
    {
        if (dv.result) { dv.result.text = rollSucceeded ? "OK" : "--"; dv.result.color = isGood ? C_SUCCESS : C_FAIL; }
        if (!isGood)
        {
            if (dv.bg) { Color c = dv.bg.color; dv.bg.color = new Color(c.r, c.g, c.b, .20f); }
            if (dv.num) { Color n = dv.num.color; dv.num.color = new Color(n.r, n.g, n.b, .20f); }
        }
    }

    void ShowFinal(CardData cd, string text, Color color) { if (cd.final) { cd.final.text = text; cd.final.color = color; } }

    int HitSkill(CombatRequest r)
    {
        // 1. Берем базовый навык (ближний или дальний бой)
        int baseSkill = r.attacker == null ? 4 : (r.isMelee ? r.attacker.weaponSkill : r.attacker.ballisticSkill);

        // 2. Если атакует ВРАГ, усложняем ему бросок за счет Неуловимости игрока
        if (!r.attackerIsPlayer && PlayerXP.Instance != null)
        {
            baseSkill += PlayerXP.Instance.elusiveness;
        }

        // 3. Возвращаем результат, ограничивая его от 2 до 7 
        // (если навык станет 7, враг физически не выкинет столько на D6 и всегда будет мазать)
        return Mathf.Clamp(baseSkill, 2, 7);
    }
    int Str(CombatRequest r) => r.attacker != null ? r.attacker.strength : 4;
    int Tough(CombatRequest r) => r.defender != null ? r.defender.toughness : 4;
    int Dmg(CombatRequest r) => r.attacker != null ? r.attacker.damage : 1;
    int ModSave(CombatRequest r) { int baseSave = r.defender != null ? r.defender.save : 5; int ap = r.attacker != null ? Mathf.Abs(r.attacker.armorPenetration) : 0; return baseSave + ap; }
    int WoundThreshold(int str, int tough) { if (str >= tough * 2) return 2; if (str > tough) return 3; if (str == tough) return 4; if (tough >= str * 2) return 6; return 5; }

    Image MkImg(Transform parent, string name, Color color, Vector2 ancMin, Vector2 ancMax, Vector2 piv, Vector2 aPos, Vector2 size)
    { GameObject go = new GameObject(name); go.transform.SetParent(parent, false); Image img = go.AddComponent<Image>(); img.color = color; RectTransform rt = go.GetComponent<RectTransform>(); rt.anchorMin = ancMin; rt.anchorMax = ancMax; rt.pivot = piv; rt.anchoredPosition = aPos; rt.sizeDelta = size; return img; }

    TextMeshProUGUI MkTxt(Transform parent, string name, string text, float size, FontStyles style, Color color, Vector2 ancMin, Vector2 ancMax, Vector2 piv, Vector2 aPos, Vector2 sz)
    { GameObject go = new GameObject(name); go.transform.SetParent(parent, false); TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style; tmp.color = color; RectTransform rt = go.GetComponent<RectTransform>(); rt.anchorMin = ancMin; rt.anchorMax = ancMax; rt.pivot = piv; rt.anchoredPosition = aPos; rt.sizeDelta = sz; return tmp; }
}