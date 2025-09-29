using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class FoodConveyorSpawnerUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private ConveyorManager conveyorManager;
    [SerializeField] private FoodConveyorItemUI foodPrefab;
    [SerializeField] private QuestionRecyclingUI questionRecyclingUI;

    [Header("Paramètres")]
    [Header("Temps de spawn par difficulté")]
    [SerializeField] private float spawnIntervalEasy = 2.0f;
    [SerializeField] private float spawnIntervalMedium = 1.5f;
    [SerializeField] private float spawnIntervalHard = 1.0f;

    [SerializeField] private float fastSpawnDelay = 0.2f;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI debugTextTimer;
    [ReadOnly, SerializeField] private int aliveCount = 0;

    private readonly List<FoodConveyorItemUI> spawnedItems = new();

    private int maxItemsToSpawn = 0;
    private int spawnedCount = 0;
    [ReadOnly, SerializeField] private bool allSpawned = false;

    private float spawnInterval = 1.5f;
    private float spawnTimer = 0f;
    private bool isRunning = false;

    private List<FoodData> cachedFoods;
    private List<PortionSelection> cachedPortions;
    private List<int> cachedAnswers;
    private QuestionSubType currentQuestionSubType;

    private void OnEnable()
    {
        FoodConveyorItemUI.OnAnyDestroyed += HandleItemDestroyed;
    }

    private void OnDisable()
    {
        FoodConveyorItemUI.OnAnyDestroyed -= HandleItemDestroyed;
    }

    public void Init(List<FoodData> foods, List<PortionSelection> portions, List<int> answers, QuestionSubType questionSubType)
    {
        spawnedItems.Clear();
        currentQuestionSubType = questionSubType;

        cachedFoods = foods;
        cachedPortions = portions;
        cachedAnswers = answers;

        SetSpawnDifficulty(DifficultyManager.Instance.CurrentDifficulty);

        maxItemsToSpawn = foods.Count;
        conveyorManager.GenerateSlots(maxItemsToSpawn);
        conveyorManager.Init();

        spawnedCount = 0;
        aliveCount = 0;
        allSpawned = false;

        spawnTimer = 0f;
        isRunning = true;

        UpdateDebugText();
    }

    private void Update()
    {
        if (!isRunning || allSpawned) return;

        spawnTimer -= Time.deltaTime;
        debugTextTimer.text = $"{spawnTimer:F2}s";
        if (spawnTimer <= 0f)
        {
            TrySpawnNext();
        }
    }

    private void TrySpawnNext()
    {
        if (spawnedCount >= cachedFoods.Count)
        {
            allSpawned = true;
            isRunning = false;
            return;
        }

        ConveyorSlotUI slot = conveyorManager.GetNextFreeSlot();
        if (slot == null)
        {
            Debug.LogWarning("Pas de slot dispo !");
            return;
        }

        FoodData f = cachedFoods[spawnedCount];
        PortionSelection sel = cachedPortions[spawnedCount];
        int answer = cachedAnswers[spawnedCount];

        conveyorManager.ActivateSlot(slot);
        FoodConveyorItemUI item = Instantiate(foodPrefab, slot.transform, false);
        item.Init(f, sel, currentQuestionSubType, spawnedCount);
        item.PlaySpawnAnimation();

        bool isIntruder = answer == 0;
        item.SetAsIntruder(isIntruder);
        item.gameObject.name = $"Item_{f.Name}_{(isIntruder ? "_Intru" : "")}";

        spawnedItems.Add(item);
        spawnedCount++;
        aliveCount++;

        UpdateDebugText();

        spawnTimer = (aliveCount == 0) ? fastSpawnDelay : spawnInterval;

        if (spawnedCount >= maxItemsToSpawn)
        {
            allSpawned = true;
            isRunning = false;
            questionRecyclingUI.SetAllSpawned();
        }

    }

    private void HandleItemDestroyed(FoodConveyorItemUI item, bool wasIntruder)
    {
        if (spawnedItems.Contains(item))
        {
            spawnedItems.Remove(item);
            aliveCount = Mathf.Max(0, aliveCount - 1);
            UpdateDebugText();

            if (aliveCount == 0 && spawnedCount < maxItemsToSpawn)
            {
                spawnTimer = 0.1f; // relance rapide
            }
        }
    }

    private void UpdateDebugText()
    {

        string fastMode = (aliveCount == 0 && !allSpawned) ? " [FAST]" : "";
        debugText.text = $"{spawnedCount}/{maxItemsToSpawn} (Alive: {aliveCount}){fastMode}";

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

    public List<FoodConveyorItemUI> GetSpawnedItems() => new(spawnedItems);

    public bool HaveAllSpawned() => allSpawned;
}