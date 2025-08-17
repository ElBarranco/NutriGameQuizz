using UnityEditor;
using UnityEngine;

public class PrefabSwitcherEditor : EditorWindow
{
    private GameObject[] selectedPrefabs; // Liste des prefabs spécifiques à afficher
    private Vector2 scrollPos;

    [MenuItem("Custom Tools/Prefab Switcher")]
    public static void ShowWindow()
    {
        GetWindow<PrefabSwitcherEditor>("Prefab Switcher");
    }

    private void OnEnable()
    {
        // Charger manuellement les prefabs que tu veux afficher
        selectedPrefabs = new GameObject[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DualQuestionGO.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SpecialQuestionGO.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EstimateQuestionGO.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MoreInfo_DualQuestionGO.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MoreInfo_EstimateQuestionGO.prefab"),

        };
    }

    void OnGUI()
    {
        GUILayout.Label("Ouvrir un Prefab spécifique", EditorStyles.boldLabel);

        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true);

        foreach (GameObject prefab in selectedPrefabs)
        {
            if (prefab != null) // S'assurer que le prefab n'est pas null
            {
                if (GUILayout.Button(prefab.name))
                {
                    Selection.activeObject = prefab;
                    AssetDatabase.OpenAsset(prefab);
                }
            }
            else
            {
                GUILayout.Label("Prefab non trouvé", EditorStyles.miniLabel);
            }
        }

        GUILayout.EndScrollView();
    }
}
