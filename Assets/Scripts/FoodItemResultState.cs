public enum FoodItemResultState
{
    Neutral,             // ni sélectionné ni solution
    SelectedCorrect,     // sélectionné et c'était la bonne réponse
    SelectedWrong,       // sélectionné mais c'était une erreur
    MissedCorrect        // non sélectionné mais c'était la bonne réponse
}