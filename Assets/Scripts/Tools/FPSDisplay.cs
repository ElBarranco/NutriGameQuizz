using UnityEngine;
using TMPro; 

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private  TextMeshProUGUI fpsTextTMP; 
    private float deltaTime = 0.0f;



    void Update()
    {
        // Calcule le deltaTime pour cette frame
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Calcule les FPS
        float fps = 1.0f / deltaTime;

        // Formate le texte pour afficher les FPS arrondis
        string text = string.Format("{0:0.}", fps);
        
        // Met à jour le texte de l'objet TextMeshPro - UI
        fpsTextTMP.text = text;
    }
}
