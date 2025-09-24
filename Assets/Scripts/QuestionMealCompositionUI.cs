using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionMealCompositionUI : BaseQuestionUI
{
    [Header("Références")]
    [SerializeField] private RectTransform bottomPanel;   // conteneur d’instanciation des aliments
    [SerializeField] private DropZoneUI dropZone;         // zone de drop
    [SerializeField] private FoodDraggableUI foodPrefab;  // prefab draggable
    [SerializeField] private InitialDockUI bottomDock;

    private QuestionData question;
    protected Action<int, bool> onAnswered;

    public void Init(QuestionData q, Action<int, bool> callback)
    {
        question = q;
        onAnswered = callback;
        dropZone.Init(q.PortionSelections);

        // Instancie tous les aliments en bas
        for (int i = 0; i < q.Aliments.Count; i++)
        {
            FoodData f = q.Aliments[i];
            PortionSelection sel = q.PortionSelections[i];

            FoodDraggableUI item = Instantiate(foodPrefab, bottomPanel, false);
            item.gameObject.name = $"DD_{f.Name}_{i}";
            item.Init(f, sel, i);
            item.transform.localScale = Vector3.one * 0.9f;

            // 1) Pose l’item dans le dock, slot i (sans tween)
            if (bottomDock != null && i < bottomDock.SlotCount)
                bottomDock.PlaceAtIndex(item, i, false);

            // 2) Mémorise le dock d’origine + l’index (pour reset avec tween)
            item.SetOriginalDock(bottomDock, i);

            // 3) Fallback : mémorise aussi parent/pos d’origine au cas où pas de dock
            item.CacheOriginalTransform();

            item.PlaySpawnAnimation(i * 0.08f);
        }
    }

    public void UpdateGuess(int currentCalorie)
    {
        guess = currentCalorie;
    }



}