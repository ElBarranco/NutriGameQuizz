using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionMealCompositionUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private RectTransform bottomPanel;   // conteneur d’instanciation des aliments
    [SerializeField] private DropZoneUI dropZone;         // zone de drop
    [SerializeField] private FoodDraggableUI foodPrefab;  // prefab draggable
    [SerializeField] private Button validateButton;       // bouton "Valider"
    [SerializeField] private InitialDockUI bottomDock;

    private QuestionData question;
    protected Action<int, bool> onAnswered;

    public void Init(QuestionData q, Action<int, bool> callback)
    {
        question = q;
        onAnswered = callback;

        // Instancie tous les aliments en bas
        for (int i = 0; i < q.Aliments.Count; i++)
        {
            FoodData f = q.Aliments[i];
            PortionSelection sel = q.PortionSelections[i];

            FoodDraggableUI item = Instantiate(foodPrefab, bottomPanel, false);
            item.gameObject.name = $"DD_{f.Name}_{i}";
            item.Init(f, sel.Grams);

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
    private void Btn_Validate()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        // Récupère les aliments actuellement dans la zone
        List<FoodDraggableUI> contents = ReadDropZoneContents();

        // Calcul du total des calories
        int totalCalories = 0;
        for (int i = 0; i < contents.Count; i++)
        {
            FoodDraggableUI item = contents[i];
            FoodData food = item.GetFood();
            float grams = item.GetGrams();
            if (food != null)
            {
                totalCalories += Mathf.RoundToInt(food.Calories * (grams / 100f));
            }
        }

        // Envoie au GameManager : index = totalCalories, bool = false (pour l’instant)
        onAnswered?.Invoke(totalCalories, false);
    }

    private List<FoodDraggableUI> ReadDropZoneContents()
    {
        List<FoodDraggableUI> list = new List<FoodDraggableUI>();
        Transform t = dropZone.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            FoodDraggableUI item = t.GetChild(i).GetComponent<FoodDraggableUI>();
            if (item != null) list.Add(item);
        }
        return list;
    }
}