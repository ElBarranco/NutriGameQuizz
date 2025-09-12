using UnityEngine;
using DG.Tweening;

public class FeedbackUI : MonoBehaviour
{
    private void Start()
    {
        transform.localScale = Vector3.zero;
        AppearAnim();
        Destroy(gameObject, 1f); // ❌ auto-destruction brute après 2s
    }

    private void AppearAnim()
    {
        transform.DOScale(Vector3.one, 0.3f)
                 .SetEase(Ease.InBack);
    }
}