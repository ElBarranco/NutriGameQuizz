using UnityEngine;

public class AppManager : MonoBehaviour
{
    private void Awake()
    {

       // Verrouiller à 60 FPS
        QualitySettings.vSyncCount = 0;       // désactive la VSync pour que targetFrameRate s'applique
        Application.targetFrameRate = 60;

        // Optionnel mais utile sur mobile
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // empêche l’extinction d’écran
        Application.runInBackground = false;           // l’app se met en pause quand elle n’est pas au premier plan
    }
}