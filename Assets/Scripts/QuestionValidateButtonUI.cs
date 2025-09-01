using UnityEngine;
using UnityEngine.UI;

public class QuestionValidateButtonUI : MonoBehaviour
{
    [SerializeField] private GameObject activeVisual;   // visuel actif
    [SerializeField] private GameObject inactiveVisual; // visuel désactivé
    [SerializeField] private Button button;             // bouton Unity

    public void SetActiveState(bool state)
    {
        //activeVisual.SetActive(state);
        //inactiveVisual.SetActive(!state);
        button.interactable = state;
    }
}