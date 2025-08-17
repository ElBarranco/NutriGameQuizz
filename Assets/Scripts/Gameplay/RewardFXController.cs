using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine.UI;

public class RewardFXController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField, Tooltip("Prefab UI de la pièce (Image sous Canvas).")]
    private GameObject coinPrefab;

    [SerializeField, Tooltip("Point d'apparition des pièces (dans le même Canvas).")]
    private Transform spawnOrigin;

    [SerializeField, Tooltip("Cible vers laquelle les pièces convergent (ex: icône score/banque).")]
    private Transform target;

    [Header("Quantité")]
    [SerializeField, Min(0)] private int coinsOnGoodAnswer = 7;
    [SerializeField, Min(0)] private int coinsOnBadAnswer = 0;
    [SerializeField, Min(0)] private int coinsOnPerfect = 10;

    [Header("Éclatement initial")]
    [SerializeField, Range(0f, 200f), Tooltip("Rayon (UI/world units) de dispersion autour du point de spawn.")]
    private float spawnSpread = 80f;
    [SerializeField, Range(0.05f, 1.5f)]
    private float firstTweenDuration = 0.25f;
    [SerializeField] private Ease firstEase = Ease.OutBack;

    [Header("Trajet vers la cible")]
    [SerializeField, Range(0.2f, 2f)]
    private float travelDuration = 0.7f;
    [SerializeField, Tooltip("Décalage entre chaque pièce pour lancer la seconde tween en cascade.")]
    [Range(0f, 0.25f)] private float perCoinStagger = 0.05f;
    [SerializeField, Tooltip("Utiliser un petit saut pour un arc sympa.")]
    private bool useJumpArc = true;
    [SerializeField, ShowIf(nameof(useJumpArc)), Range(10f, 400f)]
    private float jumpPower = 120f;
    [SerializeField, ShowIf(nameof(useJumpArc)), Range(1, 3)]
    private int jumpCount = 1;
    [SerializeField] private Ease travelEase = Ease.InCubic;

    [Header("Rotation / vie")]
    [SerializeField, Tooltip("Rotation pendant le trajet.")]
    private bool rotateDuringTravel = true;
    [SerializeField, ShowIf(nameof(rotateDuringTravel)), Range(0f, 180f)]
    private float travelRotMin = 30f;
    [SerializeField, ShowIf(nameof(rotateDuringTravel)), Range(0f, 180f)]
    private float travelRotMax = 90f;
    [SerializeField, ShowIf(nameof(rotateDuringTravel)), Range(0f, 30f), Tooltip("Petit tilt initial au spawn.")]
    private float initialRotJitter = 8f;
    [SerializeField, ShowIf(nameof(rotateDuringTravel))]
    private RotateMode rotateMode = RotateMode.Fast; // doux

    [Header("Randomisation simple")]
    [SerializeField, Range(0.5f, 2f), Tooltip("Scale atteint à l'apparition (min).")]
    private float spawnScaleMin = 0.9f;
    [SerializeField, Range(0.5f, 2f), Tooltip("Scale atteint à l'apparition (max).")]
    private float spawnScaleMax = 1.2f;
    [SerializeField, Range(0f, 2f), Tooltip("Facteur min appliqué au spread.")]
    private float spreadMulMin = 0.9f;
    [SerializeField, Range(0f, 2f), Tooltip("Facteur max appliqué au spread.")]
    private float spreadMulMax = 1.1f;

    [Header("Pool")]
    [SerializeField, Min(0)] private int poolPrewarm = 20;
    [SerializeField] private bool autoExpandPool = true;

    // --- Pool interne ---
    private readonly List<GameObject> pool = new List<GameObject>();

    [Button("Prewarm Pool")]
    private void Prewarm()
    {
        if (coinPrefab == null) { Debug.LogWarning("[RewardFX] coinPrefab manquant."); return; }
        for (int i = pool.Count; i < poolPrewarm; i++)
        {
            var go = Instantiate(coinPrefab, transform);
            go.SetActive(false);
            pool.Add(go);
        }
    }

    private void Awake()
    {
        Prewarm();
    }

    public void PlayForAnswer(bool isCorrect, bool isPerfect = false, int overrideCount = -1)
    {
        int count = overrideCount >= 0
            ? overrideCount
            : (isPerfect ? coinsOnPerfect : (isCorrect ? coinsOnGoodAnswer : coinsOnBadAnswer));

        if (count <= 0 || target == null || spawnOrigin == null || coinPrefab == null) return;
        SpawnAndAnimate(count);
    }

    // --- Cœur de l’anim ---
    private void SpawnAndAnimate(int count)
    {
        Vector3 origin = spawnOrigin.position;
        Vector3 targetPos = target.position;

        for (int i = 0; i < count; i++)
        {
            GameObject coin = GetFromPool();

            RectTransform coinRect = coin.transform as RectTransform;
            coinRect.position = origin;
            coinRect.rotation = Quaternion.identity;
            coinRect.localScale = Vector3.zero; // départ invisible

            // Petit tilt initial
            coinRect.localEulerAngles = new Vector3(0f, 0f, Random.Range(-initialRotJitter, initialRotJitter));

            Image img = coin.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color; c.a = 0f; img.color = c; // alpha 0 au départ
            }

            coin.SetActive(true);

            // --- Randoms par pièce ---
            float targetScale = Random.Range(spawnScaleMin, spawnScaleMax);
            float spreadMul = Random.Range(spreadMulMin, spreadMulMax);

            Vector2 rand = Random.insideUnitCircle * (spawnSpread * spreadMul);
            Vector3 burstPos = origin + new Vector3(rand.x, rand.y, 0f);

            Sequence seq = DOTween.Sequence();

            // 0) Apparition : scale up -> targetScale + fade in
            seq.Append(coinRect.DOScale(targetScale, 0.15f).SetEase(Ease.OutBack));
            if (img != null) seq.Join(img.DOFade(1f, 0.15f));

            // 1) Dispersion
            seq.Append(coinRect.DOMove(burstPos, firstTweenDuration).SetEase(firstEase));

            // 2) Trajet vers la cible (stagger)
            float delay = perCoinStagger * i;
            if (useJumpArc)
                seq.Append(coinRect.DOJump(targetPos, jumpPower, jumpCount, travelDuration)
                    .SetDelay(delay)
                    .SetEase(travelEase));
            else
                seq.Append(coinRect.DOMove(targetPos, travelDuration)
                    .SetDelay(delay)
                    .SetEase(travelEase));

            // 2-bis) Rotation douce en parallèle
            if (rotateDuringTravel)
            {
                float rotZ = Random.Range(travelRotMin, travelRotMax) * Mathf.Sign(Random.Range(-1f, 1f));
                seq.Join(coinRect.DORotate(new Vector3(0f, 0f, rotZ), travelDuration + 0.001f, rotateMode)
                    .SetDelay(delay)
                    .SetEase(Ease.Linear));
            }

            // 3) Disparition : scale down + fade out
            seq.Append(coinRect.DOScale(0f, 0.15f).SetEase(Ease.InBack));
            if (img != null) seq.Join(img.DOFade(0f, 0.15f));

            // Retour au pool
            seq.OnComplete(() => ReturnToPool(coin));
        }
    }

    // --- Pool helpers ---
    private GameObject GetFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
            if (!pool[i].activeSelf) return pool[i];

        if (!autoExpandPool) return pool.Count > 0 ? pool[0] : Instantiate(coinPrefab, transform);

        var go = Instantiate(coinPrefab, transform);
        go.SetActive(false);
        pool.Add(go);
        return go;
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
        if (spreadMulMax < spreadMulMin)   spreadMulMax  = spreadMulMin;
        if (travelRotMax < travelRotMin)   travelRotMax  = travelRotMin;
    }
#endif
}