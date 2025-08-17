using UnityEngine;
using NaughtyAttributes;
using System;

public class ScoreManager : MonoBehaviour
{
    [Header("Statistiques")]
    [ReadOnly, SerializeField] private int bonnesReponses;
    [ReadOnly, SerializeField] private int mauvaisesReponses;
    [ReadOnly, SerializeField] private int score;

    [Header("Streak")]
    [ReadOnly, SerializeField] private int streakActuel;
    [ReadOnly, SerializeField] private int meilleurStreak;

    [Header("Config scoring")]
    [SerializeField] private int pointsParBonneReponse = 1;
    [SerializeField] private bool utiliserMultiplicateur = true;
    [ShowIf(nameof(utiliserMultiplicateur))]
    [SerializeField, Min(1)] private int palierStreakPourMultiplicateur = 3; // +1x tous les 3 coups
    [ShowIf(nameof(utiliserMultiplicateur))]
    [SerializeField, Min(1)] private int multiplicateurMax = 5;

    // Events
    public event Action<int> OnScoreChange;                               // score
    public event Action<int,int> OnStreakChange;                          // streakActuel, meilleurStreak
    public event Action<int> OnMultiplicateurChange;                      // multiplicateur effectif

    public int BonnesReponses => bonnesReponses;
    public int MauvaisesReponses => mauvaisesReponses;
    public int Score => score;
    public int StreakActuel => streakActuel;
    public int MeilleurStreak => meilleurStreak;

    public int MultiplicateurEffectif
    {
        get
        {
            if (!utiliserMultiplicateur || palierStreakPourMultiplicateur <= 0) return 1;
            int multi = 1 + (streakActuel / palierStreakPourMultiplicateur);
            return Mathf.Clamp(multi, 1, multiplicateurMax);
        }
    }

    public void EnregistrerBonneReponse()
    {
        bonnesReponses++;
        streakActuel++;
        if (streakActuel > meilleurStreak) meilleurStreak = streakActuel;

        int gained = pointsParBonneReponse * MultiplicateurEffectif;
        score += gained;

        OnStreakChange?.Invoke(streakActuel, meilleurStreak);
        OnMultiplicateurChange?.Invoke(MultiplicateurEffectif);
        OnScoreChange?.Invoke(score);
    }

    public void EnregistrerMauvaiseReponse()
    {
        mauvaisesReponses++;
        bool hadStreak = streakActuel > 0;
        streakActuel = 0;

        if (hadStreak)
        {
            OnStreakChange?.Invoke(streakActuel, meilleurStreak);
            OnMultiplicateurChange?.Invoke(MultiplicateurEffectif);
        }
        OnScoreChange?.Invoke(score); // score inchangé, mais utile pour rafraîchir le HUD
    }

    public void Reinitialiser()
    {
        bonnesReponses = 0;
        mauvaisesReponses = 0;
        score = 0;
        streakActuel = 0;
        meilleurStreak = 0;

        OnStreakChange?.Invoke(streakActuel, meilleurStreak);
        OnMultiplicateurChange?.Invoke(MultiplicateurEffectif);
        OnScoreChange?.Invoke(score);
    }
}