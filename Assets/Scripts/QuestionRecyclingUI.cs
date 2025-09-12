using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestionRecyclingUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private FoodConveyorSpawnerUI spawner; // ✅ le tapis roulant
    [SerializeField] private RectTransform bottomPanel;     // zone fin (optionnel, pour valider)

    private QuestionData question;
    private Action<int, bool> onAnswered;

    // Liste interne pour stocker ce que le joueur a marqué comme intrus
    private List<int> currentSelection = new List<int>();

    public void Init(QuestionData q, Action<int, bool> callback)
    {
        question = q;
        onAnswered = callback;

        // Par défaut → tout est "valide" (1)
        currentSelection.Clear();
        for (int i = 0; i < q.Aliments.Count; i++)
            currentSelection.Add(1);


        spawner.Init(q.Aliments, q.PortionSelections, q.Solutions);
    }

    /// <summary>
    /// Appelé par les aliments quand le joueur les retire (swipe, clic, bouton...).
    /// </summary>
    public void MarkAsIntrus(int index)
    {
        if (index >= 0 && index < currentSelection.Count)
            currentSelection[index] = 2; // 2 = intrus
    }



    private int EncodeSelection(List<int> order)
    {
        string concat = string.Concat(order);
        return int.Parse(concat);
    }
}