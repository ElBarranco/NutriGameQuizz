// Assets/Scripts/UI/StreakCelebrationUI.cs
using UnityEngine;
using TMPro;
using DG.Tweening;

public class StreakCelebrationUI : MonoBehaviour
{
    [SerializeField] private RectTransform panel;   // ⚡ Le conteneur à déplacer
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Timing")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float showDuration = 2.0f;
    [SerializeField] private float fadeDuration = 0.15f;

    [Header("Offsets")]
    [SerializeField] private float slideOffset = 1200f; // distance hors écran

    private Tween seq;
    private Vector2 centerPos;

    private void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        if (!panel) panel = GetComponent<RectTransform>();

        centerPos = panel.anchoredPosition;
        HideInstant();
    }

    public void HideInstant()
    {
        seq?.Kill();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        if (titleText) titleText.text = "";

        // On cache hors écran gauche par défaut
        panel.anchoredPosition = centerPos - new Vector2(slideOffset, 0);
    }

    public void Show(int streak, System.Action onDone)
    {
        seq?.Kill();
        if (titleText) titleText.text = GetTitle(streak);

        group.alpha = 0f;
        group.interactable = true;
        group.blocksRaycasts = true;

        // Start position → hors écran à droite
        panel.anchoredPosition = centerPos + new Vector2(slideOffset, 0);

        var s = DOTween.Sequence();

        // Entrée : slide + fade in
        s.Append(panel.DOAnchorPos(centerPos, slideDuration).SetEase(Ease.OutBack));
        s.Join(group.DOFade(1f, fadeDuration));

        // Pause
        s.AppendInterval(showDuration);

        // Sortie : slide vers la gauche + fade out
        s.Append(panel.DOAnchorPos(centerPos - new Vector2(slideOffset, 0), slideDuration).SetEase(Ease.InBack));
        s.Join(group.DOFade(0f, fadeDuration));

        s.OnComplete(() =>
        {
            group.interactable = false;
            group.blocksRaycasts = false;
            onDone?.Invoke();
        });

        seq = s;
    }

    private string GetTitle(int streak)
    {
        if (streak >= 20) return $"{streak} à la suite ! Imparable.";
        if (streak >= 10) return $"{streak} d’affilée ! Ça déroule.";
        if (streak >= 5)  return $"{streak} de suite ! Continue.";
        return $"{streak} à la suite !";
    }
}