using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScoreHUDController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Tween Config")]
    [SerializeField] private float tweenDuration = 0.5f;
    [SerializeField] private float punchScale = 1.2f;

    private int displayedScore = 0;
    private Tween scoreTween;

    private void Awake()
    {
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();
    }

    private void OnEnable()
    {
        if (scoreManager != null)
            scoreManager.OnScoreChange += UpdateScore;
    }

    private void OnDisable()
    {
        if (scoreManager != null)
            scoreManager.OnScoreChange -= UpdateScore;
    }

    private void Start()
    {
        displayedScore = scoreManager.Score;
        scoreText.text = displayedScore.ToString();
    }

    private void UpdateScore(int newScore)
    {
        // On stoppe un tween en cours si nécessaire
        scoreTween?.Kill();

        // Tween du nombre affiché
        scoreTween = DOTween.To(
            () => displayedScore,
            x =>
            {
                displayedScore = x;
                scoreText.text = displayedScore.ToString();
            },
            newScore,
            tweenDuration
        ).SetEase(Ease.OutQuad);

        // Petit effet "punch" visuel
        scoreText.transform.DOKill(); // éviter le spam
        scoreText.transform.DOPunchScale(Vector3.one * (punchScale - 1f), 0.3f, 1, 0.5f);
    }
}