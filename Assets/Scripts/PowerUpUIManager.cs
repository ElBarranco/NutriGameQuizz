using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class PowerUpUIManager : MonoBehaviour
{
    [BoxGroup("Références UI")]
    [SerializeField] private GameObject panelPowerUps;



    public void OuvrirPanel()
    {
        panelPowerUps.SetActive(true);
    }

    public void FermerPanel()
    {
        panelPowerUps.SetActive(false);
    }
}