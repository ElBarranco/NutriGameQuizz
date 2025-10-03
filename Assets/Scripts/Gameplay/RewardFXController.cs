using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine.UI;

public class RewardFXController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform secondaryTarget;


    [Header("Quantité")]
    [SerializeField, Min(0)] private int coinsOnPerfect = 10;
    [SerializeField] private int[] streakToCoinCount = new int[5] { 3, 4, 5, 6, 7 };

    [Header("Éclatement initial")]
    [SerializeField, Range(0f, 200f)] private float spawnSpread = 80f;
    [SerializeField, Range(0.05f, 1.5f)] private float firstTweenDuration = 0.25f;
    [SerializeField] private Ease firstEase = Ease.OutBack;

    [Header("Trajet vers la cible")]
    [SerializeField, Range(0.2f, 2f)] private float travelDuration = 0.7f;
    [SerializeField, Range(0f, 0.25f)] private float perCoinStagger = 0.05f;
    [SerializeField] private bool useJumpArc = true;
    [SerializeField, ShowIf(nameof(useJumpArc)), Range(10f, 400f)] private float jumpPower = 120f;
    [SerializeField, ShowIf(nameof(useJumpArc)), Range(1, 3)] private int jumpCount = 1;
    [SerializeField] private Ease travelEase = Ease.InCubic;

    [Header("Rotation / vie")]
    [SerializeField] private bool rotateDuringTravel = true;
    [SerializeField, ShowIf(nameof(rotateDuringTravel)), Range(0f, 180f)] private float travelRotMin = 30f;
    [SerializeField, ShowIf(nameof(rotateDuringTravel)), Range(0f, 180f)] private float travelRotMax = 90f;
    [SerializeField, ShowIf(nameof(rotateDuringTravel)), Range(0f, 30f)] private float initialRotJitter = 8f;
    [SerializeField, ShowIf(nameof(rotateDuringTravel))] private RotateMode rotateMode = RotateMode.Fast;

    [Header("Randomisation simple")]
    [SerializeField, Range(0.5f, 2f)] private float spawnScaleMin = 0.9f;
    [SerializeField, Range(0.5f, 2f)] private float spawnScaleMax = 1.2f;
    [SerializeField, Range(0f, 2f)] private float spreadMulMin = 0.9f;
    [SerializeField, Range(0f, 2f)] private float spreadMulMax = 1.1f;

    [Header("Pool")]
    [SerializeField, Min(0)] private int poolPrewarm = 20;
    [SerializeField] private bool autoExpandPool = true;

    private readonly List<GameObject> pool = new List<GameObject>();

    [Button("Prewarm Pool")]
    private void Prewarm()
    {
        if (coinPrefab == null) { Debug.LogWarning("[RewardFX] coinPrefab manquant."); return; }
        for (int i = pool.Count; i < poolPrewarm; i++)
        {
            GameObject go = Instantiate(coinPrefab, transform);
            go.SetActive(false);
            pool.Add(go);
        }
    }

    private void Awake()
    {
        Prewarm();
    }

    public void PlayForAnswer(bool isCorrect, bool isPerfect, int currentStreak, int overrideCount = -1)
    {
        if (!isCorrect)
            return;

        if (isPerfect)
            PlayStarEffect();

        int count = overrideCount >= 0
            ? overrideCount
            : (isPerfect ? coinsOnPerfect : GetCoinsFromStreak(currentStreak));

        if (count <= 0 || target == null)
            return;

        SpawnAndAnimate(count, isPerfect);
    }

    private void PlayStarEffect()
    {
        GameObject star = Instantiate(starPrefab, spawnOrigin.position, Quaternion.identity, transform);
        RectTransform rect = star.transform as RectTransform;
        rect.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        seq.Append(rect.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack));
        seq.Append(rect.DORotate(new Vector3(0f, 0f, 360f), 0.5f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(2, LoopType.Restart));
        seq.Append(rect.DOMove(target.position, 0.7f).SetEase(Ease.InCubic));
        seq.Join(rect.DOScale(0f, 0.7f).SetEase(Ease.InBack));
        seq.Join(rect.DORotate(new Vector3(0f, 0f, 360f), 0.7f, RotateMode.FastBeyond360));
        seq.OnComplete(() => Destroy(star));
    }

    private void SpawnAndAnimate(int count, bool isPerfect)
    {
        Vector3 origin = spawnOrigin.position;

        int primaryCount = Mathf.CeilToInt(count / 2f);
        int secondaryCount = count - primaryCount;

        for (int i = 0; i < count; i++)
        {
            GameObject coin = GetFromPool();
            RectTransform coinRect = coin.transform as RectTransform;
            coinRect.position = origin;

            float scale = isPerfect ? 0.5f : 1f;
            coinRect.rotation = Quaternion.identity;
            coinRect.localScale = Vector3.zero;
            coinRect.localEulerAngles = new Vector3(0f, 0f, Random.Range(-initialRotJitter, initialRotJitter));

            Image img = coin.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;
            }

            coin.SetActive(true);

            float targetScale = Random.Range(spawnScaleMin, spawnScaleMax);
            float spreadMul = Random.Range(spreadMulMin, spreadMulMax);
            Vector2 rand = Random.insideUnitCircle * (spawnSpread * spreadMul);
            Vector3 burstPos = origin + new Vector3(rand.x, rand.y, 0f);

            Vector3 finalTarget = (i < primaryCount || secondaryTarget == null) ? target.position : secondaryTarget.position;

            Sequence seq = DOTween.Sequence();

            seq.Append(coinRect.DOScale(targetScale, 0.15f).SetEase(Ease.OutBack));
            if (img != null) seq.Join(img.DOFade(1f, 0.15f));
            seq.Append(coinRect.DOMove(burstPos, firstTweenDuration).SetEase(firstEase));

            float delay = perCoinStagger * i;

            if (useJumpArc)
            {
                seq.Append(coinRect.DOJump(finalTarget, jumpPower, jumpCount, travelDuration)
                    .SetDelay(delay)
                    .SetEase(travelEase));
            }
            else
            {
                seq.Append(coinRect.DOMove(finalTarget, travelDuration)
                    .SetDelay(delay)
                    .SetEase(travelEase));
            }

            if (rotateDuringTravel)
            {
                float rotZ = Random.Range(travelRotMin, travelRotMax) * Mathf.Sign(Random.Range(-1f, 1f));
                seq.Join(coinRect.DORotate(new Vector3(0f, 0f, rotZ), travelDuration + 0.001f, rotateMode)
                    .SetDelay(delay)
                    .SetEase(Ease.Linear));
            }

            seq.Append(coinRect.DOScale(0f, 0.15f).SetEase(Ease.InBack));
            if (img != null) seq.Join(img.DOFade(0f, 0.15f));
            seq.OnComplete(() => ReturnToPool(coin));
        }
    }

    private int GetCoinsFromStreak(int streak)
    {
        int clamped = Mathf.Clamp(streak, 1, streakToCoinCount.Length);
        return streakToCoinCount[clamped - 1];
    }

    private GameObject GetFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
            if (!pool[i].activeSelf)
                return pool[i];

        if (!autoExpandPool)
            return pool.Count > 0 ? pool[0] : Instantiate(coinPrefab, transform);

        GameObject go = Instantiate(coinPrefab, transform);
        go.SetActive(false);
        pool.Add(go);
        return go;
    }

    public float GetAnimationDuration()
    {
        int maxCount = Mathf.Max(coinsOnPerfect, streakToCoinCount.Length > 0 ? streakToCoinCount[^1] : 0);
        int count = Mathf.Max(1, maxCount);

        float totalDuration = firstTweenDuration + travelDuration + (perCoinStagger * (count - 1)) + 0.15f;
        return totalDuration;
    }

    private void ReturnToPool(GameObject go)
    {
        go.transform.DOKill(true);
        go.SetActive(false);
        go.transform.SetParent(transform, worldPositionStays: false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (perCoinStagger < 0f) perCoinStagger = 0f;
        if (spawnSpread < 0f) spawnSpread = 0f;
        if (poolPrewarm < 0) poolPrewarm = 0;
        if (spawnScaleMax < spawnScaleMin) spawnScaleMax = spawnScaleMin;
        if (spreadMulMax < spreadMulMin) spreadMulMax = spreadMulMin;
        if (travelRotMax < travelRotMin) travelRotMax = travelRotMin;
    }
#endif
}