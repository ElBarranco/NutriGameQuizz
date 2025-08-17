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

    // Tweens en cours à tuer proprement
    private Tweener tweenWidthA, tweenWidthB;
    private Tweener tweenColorA, tweenColorB;
    private Tweener tweenBgA, tweenBgB;
    private Tweener tweenScaleA, tweenScaleB;
    private Tweener tweenDial;

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

        KillAllTweens();

        // Stoppe toute anim sur la carte (pos/rot/scale) pour éviter l'aspiration au centre
        cardSequence?.Kill();
        cardTransform.DOKill();

        // Reset visuel neutre mais sans snap brutal (petit tween)
        ApplyContinuousVisual(0f, instant: false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (hasAnswered) return;

        InteractionManager.Instance.TriggerLightVibration();

        // Déplacement de la carte
        if (cardTransform == null) return;
            cardTransform.anchoredPosition += eventData.delta;

        // Rotation carte
        float normalizedX = Mathf.Clamp(cardTransform.anchoredPosition.x / (screenWidth * 0.5f), -1f, 1f);
        float rotationAngle = -normalizedX * 25f;
        cardTransform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

        // Réponse continue : met à jour en fonction de X
        UpdateContinuousByPos(cardTransform.anchoredPosition.x);
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

        KillAllTweens();

        cardSequence.Join(foodAContainer.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        cardSequence.Join(foodBContainer.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        cardSequence.Join(TweenWidth(foodAContainer, widthNeutral, 0.2f));
        cardSequence.Join(TweenWidth(foodBContainer, widthNeutral, 0.2f));

        cardSequence.Join(bgA.DOColor(colorDeselected, 0.2f));
        cardSequence.Join(bgB.DOColor(colorDeselected, 0.2f));

        // Dial revient au centre
        if (dial)
        {
            tweenDial?.Kill();
            tweenDial = dial.DOAnchorPosX(0f, dialTween).SetEase(Ease.OutQuad);
        }
    }

    private void Update()
    {
        if (hasAnswered) return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            AnimateAndSelect(0);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            AnimateAndSelect(1);
        }
    }

    // ---------- Réponse Continue ----------

    private void UpdateContinuousByPos(float posX)
    {
        // Normalise la position carte → [-1..1]
        float nx = Mathf.Clamp(posX / (screenWidth * 0.5f), -1f, 1f);

        // Offset contrôlé
        nx = Mathf.Clamp(nx + responseOffset, -1f, 1f);

        // Dead-zone
        float sign = Mathf.Sign(nx);
        float ax = Mathf.Abs(nx);
        float t = 0f;
        if (ax > deadZone)
        {
            // Max atteint à ~75% de l'écran
            t = Mathf.InverseLerp(deadZone, 0.75f, ax);
            // Courbe + gain
            t = Mathf.Pow(t, responseExponent);
            t = Mathf.Clamp01(t * responseGain);
        }
        // Valeur finale f dans [-1..1]
        float f = sign * t;

        ApplyContinuousVisual(f, instant: false);
    }

    /// <summary>
    /// f ∈ [-1..1] : -1 = A à fond, +1 = B à fond, 0 = neutre.
    /// </summary>
    private void ApplyContinuousVisual(float f, bool instant)
    {
        float t = Mathf.Abs(f); // intensité
        bool towardsB = f > 0f;

        // Échelles (symétriques)
        float scaleBig   = Mathf.Lerp(1f, maxScale, t);
        float scaleSmall = Mathf.Lerp(1f, minScale, t);
        float targetScaleA = towardsB ? scaleSmall : scaleBig;
        float targetScaleB = towardsB ? scaleBig   : scaleSmall;

        tweenScaleA?.Kill(); tweenScaleB?.Kill();
        tweenScaleA = foodAContainer.DOScale(targetScaleA, instant ? 0f : scaleTween).SetEase(Ease.OutQuad);
        tweenScaleB = foodBContainer.DOScale(targetScaleB, instant ? 0f : scaleTween).SetEase(Ease.OutQuad);

        // Largeurs
        float widthBig   = Mathf.Lerp(widthNeutral, widthSelected, t);
        float widthSmall = Mathf.Lerp(widthNeutral, widthDeselected, t);

        tweenWidthA?.Kill(); tweenWidthB?.Kill();
        tweenWidthA = TweenWidth(foodAContainer, towardsB ? widthSmall : widthBig, instant ? 0f : widthTween);
        tweenWidthB = TweenWidth(foodBContainer, towardsB ? widthBig   : widthSmall, instant ? 0f : widthTween);

        // Couleurs ON/OFF (pas de dégradé)
        if (bgA || bgB || imageA || imageB)
        {
            Color targetA, targetB;

            if (Mathf.Abs(f) <= colorDeadZone)
            {
                // Neutre : les deux en "selected" (tu peux mettre Deselected si tu préfères)
                targetA = colorDeselected;
                targetB = colorDeselected;
            }
            else if (f > 0f)
            {
                // Vers B : B = selected, A = deselected
                targetA = colorDeselected;
                targetB = colorSelected;
            }
            else
            {
                // Vers A : A = selected, B = deselected
                targetA = colorSelected;
                targetB = colorDeselected;
            }

            tweenBgA?.Kill(); tweenBgB?.Kill();
            tweenColorA?.Kill(); tweenColorB?.Kill();

            if (bgA)    tweenBgA    = bgA.DOColor(targetA, instant ? 0f : colorTween);
            if (bgB)    tweenBgB    = bgB.DOColor(targetB, instant ? 0f : colorTween);
        }

        // Dial (glisse en X)
        if (dial)
        {
            float targetX = f * dialTravel;
            tweenDial?.Kill();
            tweenDial = dial.DOAnchorPosX(targetX, instant ? 0f : dialTween).SetEase(Ease.OutQuad);
        }
    }

    // ---------- Utilitaires ----------

    private Tweener TweenWidth(RectTransform rt, float targetWidth, float duration)
    {
        var le = rt.GetComponent<LayoutElement>();
        if (le != null)
        {
            return DOVirtual.Float(le.preferredWidth, targetWidth, duration, v => le.preferredWidth = v)
                .SetEase(Ease.OutQuad);
        }
        else
        {
            float h = rt.sizeDelta.y;
            return rt.DOSizeDelta(new Vector2(targetWidth, h), duration).SetEase(Ease.OutQuad);
        }
    }

    private void KillAllTweens()
    {
        tweenWidthA?.Kill(); tweenWidthB?.Kill();
        tweenColorA?.Kill(); tweenColorB?.Kill();
        tweenBgA?.Kill(); tweenBgB?.Kill();
        tweenScaleA?.Kill(); tweenScaleB?.Kill();
        tweenDial?.Kill();
    }
}