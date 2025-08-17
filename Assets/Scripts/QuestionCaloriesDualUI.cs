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
    [SerializeField] private RectTransform dial;           // sprite au milieu qui se déplace
    [SerializeField] private float dialTravel = 220f;      // amplitude +-X
    [SerializeField] private float dialTween = 0.12f;      // lissage du dial

    [Header("Dimensions")]
    [SerializeField] private float widthDeselected = 425f;
    [SerializeField] private float widthSelected = 600f;
    [SerializeField] private float widthNeutral = 512f;

    [Header("Tweens")]
    [SerializeField] private float widthTween = 0.12f;
    [SerializeField] private float colorTween = 0.12f;
    [SerializeField] private float scaleTween = 0.10f;

    [Header("Couleurs")]
    [SerializeField] private Color colorSelected = Color.white;
    [SerializeField] private Color colorDeselected = new Color(1f, 1f, 1f, 0.6f);

    [Header("Couleurs ON/OFF")]
    [Tooltip("Autour de 0, on reste neutre pour éviter le flicker.")]
    [Range(0f, 0.2f)] [SerializeField] private float colorDeadZone = 0.04f;

    [Header("Réponse Continue (mapping X → f)")]
    [Tooltip("Zone morte autour du centre (0..1). 0.08 = 8% de demi-largeur d’écran ignorée.")]
    [Range(0f, 0.5f)] [SerializeField] private float deadZone = 0.08f;
    [Tooltip("Amplification globale de la réponse (0.0..2.0). 1 = linéaire après deadZone.")]
    [Range(0f, 2f)] [SerializeField] private float responseGain = 1.0f;
    [Tooltip("Courbure de la réponse (>1 = plus doux au début, <1 = plus réactif).")]
    [Range(0.25f, 3f)] [SerializeField] private float responseExponent = 1.35f;
    [Tooltip("Décale le point neutre (ex: 0.05 = pousse légèrement à droite).")]
    [Range(-0.25f, 0.25f)] [SerializeField] private float responseOffset = 0f;
    [Tooltip("Échelle max/min appliquée en bout de course.")]
    [SerializeField] private float maxScale = 1.12f;
    [SerializeField] private float minScale = 0.88f;

    protected FoodData foodA;
    protected FoodData foodB;

    protected bool hasAnswered = false;
    protected Vector2 originalPosition;
    protected Quaternion originalRotation;
    protected float screenWidth = 0;

    // Séquence liée au cardTransform (reset position/rotation)
    private Sequence cardSequence;

    protected Action<int, bool> onAnswered;

    public void Init(FoodData a, FoodData b, PortionSelection portionA, PortionSelection portionB, Action<int, bool> callback)
    {
        screenWidth = Screen.width;
        foodA = a;
        foodB = b;
        onAnswered = callback;

        nameA.text = PortionTextFormatter.ToDisplayWithFood(foodA, portionA);
        nameB.text = PortionTextFormatter.ToDisplayWithFood(foodB, portionB);

        imageA.sprite = FoodSpriteLoader.LoadFoodSprite(foodA.Name);
        imageB.sprite = FoodSpriteLoader.LoadFoodSprite(foodB.Name);

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
            onAnswered?.Invoke(index, false);
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
        TweenWidth(foodAContainer, widthNeutral, 0.2f);
        TweenWidth(foodBContainer, widthNeutral, 0.2f);

        if (bgA) bgA.DOColor(colorDeselected, 0.2f);
        if (bgB) bgB.DOColor(colorDeselected, 0.2f);

        // Dial revient au centre
        if (dial)
        {
            dial.DOKill();
            dial.DOAnchorPosY(0f, dialTween).SetEase(Ease.OutQuad);
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
        nx = Mathf.Clamp(nx + responseOffset, -1f, 1f);

        // Dead-zone + courbe
        float sign = Mathf.Sign(nx);
        float ax = Mathf.Abs(nx);
        float t = 0f;
        if (ax > deadZone)
        {
            t = Mathf.InverseLerp(deadZone, 0.75f, ax);
            t = Mathf.Pow(t, responseExponent);
            t = Mathf.Clamp01(t * responseGain);
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
        float scaleBig   = Mathf.Lerp(1f, maxScale, t);
        float scaleSmall = Mathf.Lerp(1f, minScale, t);
        float targetScaleA = towardsB ? scaleSmall : scaleBig;
        float targetScaleB = towardsB ? scaleBig   : scaleSmall;

        foodAContainer.DOScale(targetScaleA, instant ? 0f : scaleTween).SetEase(Ease.OutQuad);
        foodBContainer.DOScale(targetScaleB, instant ? 0f : scaleTween).SetEase(Ease.OutQuad);

        // Largeurs
        float widthBig   = Mathf.Lerp(widthNeutral, widthSelected, t);
        float widthSmall = Mathf.Lerp(widthNeutral, widthDeselected, t);

        TweenWidth(foodAContainer, towardsB ? widthSmall : widthBig, instant ? 0f : widthTween);
        TweenWidth(foodBContainer, towardsB ? widthBig   : widthSmall, instant ? 0f : widthTween);

        // Couleurs ON/OFF
        Color targetA, targetB;
        if (Mathf.Abs(f) <= colorDeadZone)
        {
            targetA = colorDeselected;
            targetB = colorDeselected;
        }
        else if (towardsB)
        {
            targetA = colorDeselected;
            targetB = colorSelected;
        }
        else
        {
            targetA = colorSelected;
            targetB = colorDeselected;
        }

        if (bgA) bgA.DOColor(targetA, instant ? 0f : colorTween);
        if (bgB) bgB.DOColor(targetB, instant ? 0f : colorTween);

        // Dial (glisse en X)
        if (dial)
        {
            float targetY = f * dialTravel;
            dial.DOAnchorPosY(targetY, instant ? 0f : dialTween).SetEase(Ease.OutQuad);
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