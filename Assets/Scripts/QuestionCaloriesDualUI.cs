using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;

public class QuestionCaloriesDualUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card")]
    [SerializeField] private RectTransform cardTransform;
    [SerializeField] private float dragThreshold = 200f;

    [Header("UI Références")]
    [SerializeField] protected TextMeshProUGUI nameA;
    [SerializeField] protected TextMeshProUGUI nameB;
    [SerializeField] protected Image imageA;
    [SerializeField] protected Image imageB;

    [SerializeField] private RectTransform foodAContainer;
    [SerializeField] private RectTransform foodBContainer;
    [SerializeField] private Image bgA;
    [SerializeField] private Image bgB;

    [Header("Dial (facultatif)")]
    [SerializeField] private RectTransform dial; // sprite au milieu qui se déplace

    // ====== PARAMÈTRES FIGÉS (non sérialisés, non modifiables par d'autres scripts) ======
    private const float DIAL_TRAVEL = 220f;   // amplitude +-X
    private const float DIAL_TWEEN = 0.12f;  // lissage du dial

    // Dimensions
    private const float WIDTH_DESEL = 425f;
    private const float WIDTH_SEL = 600f;
    private const float WIDTH_NEUTRAL = 512f;

    // Tweens
    private const float WIDTH_TWEEN = 0.12f;
    private const float COLOR_TWEEN = 0.12f;
    private const float SCALE_TWEEN = 0.10f;

    // Couleurs
    [SerializeField] private Color COLOR_SELECTED = Color.white;
    [SerializeField] private Color COLOR_DESELECTED = new Color(1f, 1f, 1f, 0.6f);

    // Couleurs ON/OFF
    private const float COLOR_DEAD_ZONE = 0.04f; // [0..0.2]

    // Réponse Continue (mapping X → f)
    private const float DEAD_ZONE = 0.08f;  // [0..0.5]
    private const float RESPONSE_GAIN = 1.0f;   // [0..2]
    private const float RESPONSE_EXP = 1.35f;  // [0.25..3]
    private const float RESPONSE_OFFSET = 0f;     // [-0.25..0.25]
    private const float MAX_SCALE = 1.12f;
    private const float MIN_SCALE = 0.88f;
    // ======================================================================================

    // Données & état accessibles aux classes enfants (comme dans ton script original)
    protected FoodData foodA;
    protected FoodData foodB;

    protected bool hasAnswered = false;
    protected Vector2 originalPosition;
    protected Quaternion originalRotation;
    protected float screenWidth = 0;

    // Séquence liée au cardTransform (reset position/rotation)
    private Sequence cardSequence;


    public void Init(FoodData a, FoodData b, PortionSelection portionA, PortionSelection portionB)
    {
        screenWidth = Screen.width;
        foodA = a;
        foodB = b;


        nameA.text = PortionTextFormatter.ToDisplayWithFood(foodA, portionA);
        nameB.text = PortionTextFormatter.ToDisplayWithFood(foodB, portionB);

        imageA.sprite = SpriteLoader.LoadFoodSprite(foodA.Name);
        imageB.sprite = SpriteLoader.LoadFoodSprite(foodB.Name);

        InitVisual();
    }

    public void InitVisual()
    {
        cardTransform.localScale = Vector3.zero;
        cardTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        originalPosition = cardTransform.anchoredPosition;
        originalRotation = cardTransform.rotation;

        // État neutre
        ApplyContinuousVisual(0f, instant: true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (hasAnswered) return;

        KillFeedbackTweens();

        // Stoppe toute anim sur la carte (pos/rot/scale) pour éviter l'aspiration au centre
        cardSequence?.Kill();
        cardTransform.DOKill();

        // Reset visuel neutre (feedback uniquement)
        ApplyContinuousVisual(0f, instant: false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (hasAnswered) return;

        InteractionManager.Instance.TriggerLightVibration();

        if (cardTransform == null) return;
        cardTransform.anchoredPosition += eventData.delta;

        // Rotation carte (comportement existant conservé)
        float normalizedX = Mathf.Clamp(cardTransform.anchoredPosition.x / (screenWidth * 0.5f), -1f, 1f);
        float rotationAngle = -normalizedX * 25f;
        cardTransform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

        // ✅ Présélection : feedback basé UNIQUEMENT sur X
        UpdatePreselectByX();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (hasAnswered) return;

        float x = cardTransform.anchoredPosition.x;

        if (x < -dragThreshold)
        {
            AnimateAndSelect(0); // gauche
        }
        else if (x > dragThreshold)
        {
            AnimateAndSelect(1); // droite
        }
        else
        {
            ResetCardPosition();
        }
    }

    private void AnimateAndSelect(int index)
    {
        hasAnswered = true;

        // Sécurise : stoppe toute anim en cours sur la carte
        cardSequence?.Kill();
        cardTransform.DOKill();

        float offsetX = index == 0 ? -800f : 800f;
        float rotateZ = index == 0 ? 45f : -45f;

        RectTransform losingContainer = index == 0 ? foodBContainer : foodAContainer;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(cardTransform.DOAnchorPos(originalPosition + new Vector2(offsetX, 0), 0.2f).SetEase(Ease.InExpo));
        sequence.Join(cardTransform.DORotate(new Vector3(0, 0, rotateZ), 0.3f));
        sequence.Join(losingContainer.DOScale(0.8f, 0.3f).SetEase(Ease.InBack));

        sequence.OnComplete(() =>
        {
            GameManager.Instance.OnQuestionAnswered(index);
            Destroy(gameObject);
        });
    }

    private void ResetCardPosition()
    {
        cardSequence?.Kill();

        cardSequence = DOTween.Sequence();
        cardSequence.Append(cardTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.OutBack));
        cardSequence.Join(cardTransform.DORotateQuaternion(originalRotation, 0.2f));

        // Feedback → retour neutre
        KillFeedbackTweens();

        cardSequence.Join(foodAContainer.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        cardSequence.Join(foodBContainer.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        TweenWidth(foodAContainer, WIDTH_NEUTRAL, 0.2f);
        TweenWidth(foodBContainer, WIDTH_NEUTRAL, 0.2f);

        if (bgA) bgA.DOColor(COLOR_DESELECTED, 0.2f);
        if (bgB) bgB.DOColor(COLOR_DESELECTED, 0.2f);

        // Dial revient au centre
        if (dial)
        {
            dial.DOKill();
            dial.DOAnchorPosY(0f, DIAL_TWEEN).SetEase(Ease.OutQuad);
        }
    }

    private void Update()
    {
        if (hasAnswered) return;

        if (Keyboard.current?.leftArrowKey.wasPressedThisFrame == true)
        {
            AnimateAndSelect(0);
        }
        else if (Keyboard.current?.rightArrowKey.wasPressedThisFrame == true)
        {
            AnimateAndSelect(1);
        }
    }

    // ---------- Présélection : calcul uniquement basé sur X ----------
    private void UpdatePreselectByX()
    {
        // X normalisé → [-1..1]
        float nx = Mathf.Clamp(cardTransform.anchoredPosition.x / (screenWidth * 0.5f), -1f, 1f);

        // Offset éventuel
        nx = Mathf.Clamp(nx + RESPONSE_OFFSET, -1f, 1f);

        // Dead-zone + courbe
        float sign = Mathf.Sign(nx);
        float ax = Mathf.Abs(nx);
        float t = 0f;
        if (ax > DEAD_ZONE)
        {
            t = Mathf.InverseLerp(DEAD_ZONE, 0.75f, ax);
            t = Mathf.Pow(t, RESPONSE_EXP);
            t = Mathf.Clamp01(t * RESPONSE_GAIN);
        }

        float f = sign * t; // [-1..1] : -1 = A (gauche), +1 = B (droite)
        ApplyContinuousVisual(f, instant: false);
    }

    /// <summary>
    /// f ∈ [-1..1] : -1 = A, +1 = B, 0 = neutre.
    /// N’affecte QUE le feedback (échelle / largeur / couleurs / dial-X).
    /// </summary>
    private void ApplyContinuousVisual(float f, bool instant)
    {
        float t = Mathf.Abs(f); // intensité
        bool towardsB = f > 0f;

        // Kill des tweens feedback en cours (pas de refs stockées)
        foodAContainer.DOKill();
        foodBContainer.DOKill();
        bgA?.DOKill();
        bgB?.DOKill();
        dial?.DOKill();

        // Échelles (symétriques)
        float scaleBig = Mathf.Lerp(1f, MAX_SCALE, t);
        float scaleSmall = Mathf.Lerp(1f, MIN_SCALE, t);
        float targetScaleA = towardsB ? scaleSmall : scaleBig;
        float targetScaleB = towardsB ? scaleBig : scaleSmall;

        foodAContainer.DOScale(targetScaleA, instant ? 0f : SCALE_TWEEN).SetEase(Ease.OutQuad);
        foodBContainer.DOScale(targetScaleB, instant ? 0f : SCALE_TWEEN).SetEase(Ease.OutQuad);

        // Largeurs
        float widthBig = Mathf.Lerp(WIDTH_NEUTRAL, WIDTH_SEL, t);
        float widthSmall = Mathf.Lerp(WIDTH_NEUTRAL, WIDTH_DESEL, t);

        TweenWidth(foodAContainer, towardsB ? widthSmall : widthBig, instant ? 0f : WIDTH_TWEEN);
        TweenWidth(foodBContainer, towardsB ? widthBig : widthSmall, instant ? 0f : WIDTH_TWEEN);

        // Couleurs ON/OFF
        Color targetA, targetB;
        if (Mathf.Abs(f) <= COLOR_DEAD_ZONE)
        {
            targetA = COLOR_DESELECTED;
            targetB = COLOR_DESELECTED;
        }
        else if (towardsB)
        {
            targetA = COLOR_DESELECTED;
            targetB = COLOR_SELECTED;
        }
        else
        {
            targetA = COLOR_SELECTED;
            targetB = COLOR_DESELECTED;
        }

        if (bgA) bgA.DOColor(targetA, instant ? 0f : COLOR_TWEEN);
        if (bgB) bgB.DOColor(targetB, instant ? 0f : COLOR_TWEEN);

        // Dial (glisse en X)
        if (dial)
        {
            float targetY = f * DIAL_TRAVEL;
            dial.DOAnchorPosY(targetY, instant ? 0f : DIAL_TWEEN).SetEase(Ease.OutQuad);
        }
    }

    // ---------- Utilitaires ----------
    private void TweenWidth(RectTransform rt, float targetWidth, float duration)
    {
        var le = rt.GetComponent<LayoutElement>();
        if (le != null)
        {
            DOVirtual.Float(le.preferredWidth, targetWidth, duration, v => le.preferredWidth = v)
                .SetEase(Ease.OutQuad);
        }
        else
        {
            var sd = rt.sizeDelta;
            rt.DOSizeDelta(new Vector2(targetWidth, sd.y), duration).SetEase(Ease.OutQuad);
        }
    }

    private void KillFeedbackTweens()
    {
        // On tue UNIQUEMENT ce qui concerne le feedback visuel
        foodAContainer?.DOKill();
        foodBContainer?.DOKill();
        bgA?.DOKill();
        bgB?.DOKill();
        dial?.DOKill();
    }
}