using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class QuestionRecyclingUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private FoodConveyorSpawnerUI spawner;

    private QuestionData question;
    private Action<int, bool> onAnswered;

    [ReadOnly, SerializeField] private List<FoodConveyorItemUI> aliveItems = new List<FoodConveyorItemUI>();

    private void OnEnable()
    {
        FoodConveyorItemUI.OnAnyDestroyed += HandleItemDestroyed;
    }

    private void OnDisable()
    {
        FoodConveyorItemUI.OnAnyDestroyed -= HandleItemDestroyed;
    }

    public void Init(QuestionData q, Action<int, bool> callback)
    {
        question = q;
        onAnswered = callback;

        aliveItems.Clear();

        // Lance le spawn
        spawner.Init(q.Aliments, q.PortionSelections, q.Solutions);

        // Récupère les items spawnés au fur et à mesure
        aliveItems.AddRange(spawner.GetSpawnedItems());
    }

    private void HandleItemDestroyed(FoodConveyorItemUI item, bool wasIntruder)
    {
        if (aliveItems.Contains(item))
            aliveItems.Remove(item);

        Debug.Log($"[QuestionRecyclingUI] Item détruit. Restants: {aliveItems.Count} (AllSpawned={spawner.HaveAllSpawned()})");

        // ✅ Ne check la fin QUE si tout a spawn
        if (spawner.HaveAllSpawned() && aliveItems.Count == 0)
        {
            Debug.Log("[QuestionRecyclingUI] Tous les items détruits → question terminée !");
            onAnswered?.Invoke(1, true);
            Destroy(gameObject);
        }
    }
}