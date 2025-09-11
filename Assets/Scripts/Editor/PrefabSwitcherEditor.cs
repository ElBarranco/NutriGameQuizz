using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PrefabSwitcherEditor : EditorWindow
{
    private List<GameObject> questionPrefabs = new List<GameObject>();
    private List<GameObject> moreInfoPrefabs = new List<GameObject>();
    private Vector2 scrollPos;

    [MenuItem("Custom Tools/Prefab Switcher")]
    public static void ShowWindow()
    {
        GetWindow<PrefabSwitcherEditor>("Prefab Switcher");
    }

    private void OnEnable()
    {
        // Charger automatiquement les prefabs depuis les dossiers
        questionPrefabs = LoadPrefabsFromFolder("Assets/Prefabs/Question");
        moreInfoPrefabs = LoadPrefabsFromFolder("Assets/Prefabs/MoreInfo");

        //AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Question/Q_EstimateQuestionGO.prefab"),
    }

    private List<GameObject> LoadPrefabsFromFolder(string folderPath)
    {
        List<GameObject> prefabs = new List<GameObject>();

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"Dossier introuvable : {folderPath}");
            return prefabs;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
                prefabs.Add(prefab);
        }

        return prefabs;
    }

    private void DrawPrefabButtons(List<GameObject> prefabs)
    {
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
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
    }

    void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true);

        // Section Question Prefabs
        GUILayout.Label("📌 Question ", EditorStyles.boldLabel);
        DrawPrefabButtons(questionPrefabs);

        GUILayout.Space(15); // Espace visuel

        // Section MoreInfo Prefabs
        GUILayout.Label("ℹ️ MoreInfo ", EditorStyles.boldLabel);
        DrawPrefabButtons(moreInfoPrefabs);

        GUILayout.EndScrollView();
    }
}