using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class FoodConveyorSpawnerUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private FoodConveyorItemUI foodPrefab;
    [SerializeField] private Transform itemsParent;

    [Header("Paramètres")]
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float fastSpawnDelay = 0.2f; // délai réduit si plus d’alive
    [SerializeField] private float itemSpeed = 100f;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugText; // assigner dans l’inspecteur
    [ReadOnly, SerializeField] private int aliveCount = 0;

    private readonly List<FoodConveyorItemUI> spawnedItems = new List<FoodConveyorItemUI>();

    private int maxItemsToSpawn = 0;
    private int spawnedCount = 0;
    [ReadOnly, SerializeField] private bool allSpawned = false;

    private Coroutine spawnRoutine;

    // Caches pour relancer la boucle
    private List<FoodData> cachedFoods;
    private List<PortionSelection> cachedPortions;
    private List<int> cachedAnswers;

    private void OnEnable()
    {
        FoodConveyorItemUI.OnAnyDestroyed += HandleItemDestroyed;
    }

    private void OnDisable()
    {
        FoodConveyorItemUI.OnAnyDestroyed -= HandleItemDestroyed;
    }

    public void Init(List<FoodData> foods, List<PortionSelection> portions, List<int> answers)
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnedItems.Clear();

        cachedFoods = foods;
        cachedPortions = portions;
        cachedAnswers = answers;

        maxItemsToSpawn = foods.Count;
        spawnedCount = 0;
        aliveCount = 0;
        allSpawned = false;

        UpdateDebugText();

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        for (int i = spawnedCount; i < cachedFoods.Count; i++)
        {
            FoodData f = cachedFoods[i];
            PortionSelection sel = cachedPortions[i];
            int answer = cachedAnswers[i];

            FoodConveyorItemUI item = Instantiate(foodPrefab, itemsParent);
            item.transform.position = spawnPoint.position;
            item.Init(f, sel, i);
            item.SetAsIntruder(answer == 0);
            item.PlaySpawnAnimation();

            item.SetSpeed(itemSpeed);
            item.SetEndPoint(endPoint);

            spawnedItems.Add(item);
            spawnedCount++;
            aliveCount++;

            UpdateDebugText();

            if (i < cachedFoods.Count - 1)
            {
                float delay = (aliveCount == 0) ? fastSpawnDelay : spawnInterval;
                yield return new WaitForSeconds(delay);
            }
        }

        allSpawned = true;
        spawnRoutine = null;
    }

    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            string fastMode = (aliveCount == 0 && !allSpawned) ? " [FAST]" : "";
            debugText.text = $"{spawnedCount}/{maxItemsToSpawn} (Alive: {aliveCount}){fastMode}";
        }
    }

    private void HandleItemDestroyed(FoodConveyorItemUI item, bool wasIntruder)
    {
        if (spawnedItems.Contains(item))
        {
            spawnedItems.Remove(item);
            aliveCount = Mathf.Max(0, aliveCount - 1);
            UpdateDebugText();

            // 🚀 Si plus d’alive mais encore des spawns → accélérer
            if (aliveCount == 0 && spawnedCount < maxItemsToSpawn && spawnRoutine == null)
            {
                spawnRoutine = StartCoroutine(SpawnLoop());
            }
        }
    }

    public List<FoodConveyorItemUI> GetSpawnedItems() => new List<FoodConveyorItemUI>(spawnedItems);

    public bool HaveAllSpawned() => allSpawned;
}