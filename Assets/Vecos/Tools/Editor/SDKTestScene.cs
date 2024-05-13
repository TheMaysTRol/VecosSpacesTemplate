using UnityEngine;
using UnityEditor;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;


public class SDKTestScene : EditorWindow
{

    private GameObject pcPlayerPrefab; // Reference to the SpawnManager prefab
    private GameObject vrPlayerPrefab; // Reference to the SpawnManager prefab


    [MenuItem("Vecos/Testing Scene")]
    public static void ShowWindow()
    {
        GetWindow<SDKTestScene>("Testing Scene");
    }



    private void OnEnable()
    {
        pcPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vecos/Tools/Prefabs/PC Test Player.prefab");
        vrPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vecos/Tools/Prefabs/PC Test Player.prefab");
    }

    private void OnGUI()
    {
        DrawPlayerSpawn();
    }


    private void DrawPlayerSpawn()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Testing", EditorStyles.boldLabel);
        List<Tool_PlayerExistance> players = GameObject.FindObjectsByType<Tool_PlayerExistance>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
        if (players.Count > 0)
        {
            EditorGUILayout.HelpBox("There is already a player in the scene", MessageType.Info);
            foreach (Tool_PlayerExistance player in players)
            {
                Debug.Log(player.gameObject.name);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(player.name);

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = player.gameObject;
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete player", "Are you sure you want to delete the camera " + player.name + "?", "Yes", "No"))
                    {
                        // Record the camera deletion for undo
                        Undo.DestroyObjectImmediate(player.gameObject);
                    }
                }

                if (GUILayout.Button("Disable", GUILayout.Width(60)))
                {
                    // Record the camera disable action for undo
                    Undo.RecordObject(player.gameObject, "Disable player");
                    player.gameObject.SetActive(false);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            CreatePcPlayer();
            CreateVrPlayer();
        }

    }

    private void CreatePcPlayer()
    {
        if (GUILayout.Button("Create PC Player"))
        {
            if (pcPlayerPrefab != null)
            {
                GameObject spawnManagerInstance = PrefabUtility.InstantiatePrefab(pcPlayerPrefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(spawnManagerInstance, "Create PC player");
                Selection.activeGameObject = spawnManagerInstance;
            }
            else
            {
                Debug.LogError("SpawnManager prefab is missing. Please assign the prefab in the SDKManager script.");
            }
        }
    }

    private void CreateVrPlayer()
    {
        if (GUILayout.Button("Create VR Player"))
        {
            if (vrPlayerPrefab != null)
            {
                GameObject spawnManagerInstance = PrefabUtility.InstantiatePrefab(vrPlayerPrefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(spawnManagerInstance, "Create VR player");
                Selection.activeGameObject = spawnManagerInstance;
            }
            else
            {
                Debug.LogError("SpawnManager prefab is missing. Please assign the prefab in the SDKManager script.");
            }
        }
    }
}
