using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FeedbackWrongUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private RectTransform target;   // 🎯 élément à shaker (souvent l’icône ou le container)
    [SerializeField] private Image feedbackImage;    // pour fade-out

    private void Start()
    {
        PlaySequence();
    }

    private void PlaySequence()
    {
        Sequence seq = DOTween.Sequence();

        // 1) Scale pop-in
        transform.localScale = Vector3.zero;

        seq.Append(transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        seq.Join(target.DOShakeAnchorPos(0.4f, 20f, 15, 90, false, true));


        // 4) Auto-destroy à la fin
        seq.OnComplete(() => Destroy(gameObject));
    }
}