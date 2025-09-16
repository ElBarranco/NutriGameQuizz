using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class FoodConveyorSpawnerUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private ConveyorManager conveyorManager;
    [SerializeField] private FoodConveyorItemUI foodPrefab;

    [Header("Paramètres")]
    [Header("Temps de spawn par difficulté")]
    [SerializeField] private float spawnIntervalEasy = 2.0f;
    [SerializeField] private float spawnIntervalMedium = 1.5f;
    [SerializeField] private float spawnIntervalHard = 1.0f;

    [SerializeField] private float fastSpawnDelay = 0.2f; // délai réduit si plus d’alive

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugText; // assigner dans l’inspecteur
    [ReadOnly, SerializeField] private int aliveCount = 0;

    private readonly List<FoodConveyorItemUI> spawnedItems = new List<FoodConveyorItemUI>();

    private int maxItemsToSpawn = 0;
    private int spawnedCount = 0;
    [ReadOnly, SerializeField] private bool allSpawned = false;

    private Coroutine spawnRoutine;

    private float spawnInterval = 1.5f;

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

        SetSpawnDifficulty(DifficultyManager.Instance.CurrentDifficulty);

        maxItemsToSpawn = foods.Count;
        conveyorManager.GenerateSlots(maxItemsToSpawn);
        spawnedCount = 0;
        aliveCount = 0;
        allSpawned = false;

        UpdateDebugText();

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        conveyorManager.Init();

        for (int i = spawnedCount; i < cachedFoods.Count; i++)
        {
            FoodData f = cachedFoods[i];
            PortionSelection sel = cachedPortions[i];
            int answer = cachedAnswers[i];

            // 🔹 Récupère le prochain slot
            ConveyorSlotUI slot = conveyorManager.GetNextFreeSlot();
            if (slot == null)
            {
                Debug.LogWarning("Pas de slot dispo !");
                yield break;
            }

            // 🔹 Instantie l’item DANS le slot
            conveyorManager.ActivateSlot(slot);
            FoodConveyorItemUI item = Instantiate(foodPrefab, slot.transform, false);
            item.Init(f, sel, i);
            item.PlaySpawnAnimation();

            bool isIntruder = answer == 0;
            item.SetAsIntruder(isIntruder);

            string intruSuffix = isIntruder ? "_Intru" : "";
            item.gameObject.name = $"Item_{f.Name}_{intruSuffix}";

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

    public void SetSpawnDifficulty(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy:
                spawnInterval = spawnIntervalEasy;
                break;
            case DifficultyLevel.Medium:
                spawnInterval = spawnIntervalMedium;
                break;
            case DifficultyLevel.Hard:
                spawnInterval = spawnIntervalHard;
                break;
        }
    }

    public List<FoodConveyorItemUI> GetSpawnedItems() => new List<FoodConveyorItemUI>(spawnedItems);

    public bool HaveAllSpawned() => allSpawned;
}

