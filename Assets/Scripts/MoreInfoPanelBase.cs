using UnityEngine;
using DG.Tweening;

public abstract class MoreInfoPanelBase : MonoBehaviour
{
    [SerializeField] protected GameObject panel;
    [SerializeField] protected CanvasGroup canvasGroup;

    public virtual void Hide()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(0f, 0.3f));
        sequence.Join(panel.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));
        sequence.OnComplete(() =>
{
    DOTween.Kill(this); // on tue toute tween liée à ce target
    Destroy(gameObject);
});
    }

    protected string FormatValue(float value, string unit)
    {
        return $"{Mathf.RoundToInt(value)} {unit}";
    }
}