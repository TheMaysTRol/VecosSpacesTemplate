using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;


public class SDKBuildChcker : EditorWindow
{

    private GameObject spawnManagerPrefab; // Reference to the SpawnManager prefab
    private GameObject playerSetupPrefab; // Reference to the SpawnManager prefab


    [MenuItem("Vecos/Build checker")]
    public static void ShowWindow()
    {
        GetWindow<SDKBuildChcker>("Vecos Manager");
    }



    private void OnEnable()
    {
        // Load the SpawnManager prefab
        spawnManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vecos/Tools/Prefabs/SpawnManagerPrefab.prefab");
        playerSetupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vecos/Tools/Prefabs/PlayerSetup.prefab");
    }

    private void OnGUI()
    {
        DrawPlayerButtons();

        DrawCameraButtons();

        CheckSpawnManager();

        CheckPlayerSetup();

        DrawAddressablePathVerification();
    }


    private void DrawPlayerButtons()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Players check", EditorStyles.boldLabel);
        Tool_PlayerExistance[] players = FindObjectsOfType<Tool_PlayerExistance>();

        if (players.Length > 0)
        {
            EditorGUILayout.HelpBox("You must delete the players.", MessageType.Error);
        }
        else
        {
            EditorGUILayout.HelpBox("Players check is successful.", MessageType.Info);
        }

        foreach (Tool_PlayerExistance player in players)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(player.name);

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = player.gameObject;
            }

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Player", "Are you sure you want to delete the Player " + player.name + "?", "Yes", "No"))
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

    private void DrawCameraButtons()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Cameras check", EditorStyles.boldLabel);
        Camera[] cameras = FindObjectsOfType<Camera>();

        if (cameras.Length > 0)
        {
            EditorGUILayout.HelpBox("You must delete the cameras.", MessageType.Error);
        }
        else
        {
            EditorGUILayout.HelpBox("Cameras check is successful.", MessageType.Info);
        }

        foreach (Camera camera in cameras)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(camera.name);

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = camera.gameObject;
            }

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Camera", "Are you sure you want to delete the camera " + camera.name + "?", "Yes", "No"))
                {
                    // Record the camera deletion for undo
                    Undo.DestroyObjectImmediate(camera.gameObject);
                }
            }

            if (GUILayout.Button("Disable", GUILayout.Width(60)))
            {
                // Record the camera disable action for undo
                Undo.RecordObject(camera.gameObject, "Disable Camera");
                camera.gameObject.SetActive(false);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void CheckSpawnManager()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Spawn Manager check", EditorStyles.boldLabel);
        SpawnManager spawnManager = GameObject.FindAnyObjectByType<SpawnManager>();

        if (spawnManager == null)
        {
            EditorGUILayout.HelpBox("SpawnManager does not exist in the scene.", MessageType.Error);
            CreateSpawnManager();
        }
        else
        {

            if (spawnManager == null || spawnManager.spawnPositions == null || spawnManager.spawnPositions.Count == 0)
            {
                EditorGUILayout.HelpBox("SpawnManager exists but Spawn Positions array is empty.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("SpawnManager check is successful.", MessageType.Info);
            }
        }
    }
    private void CreateSpawnManager()
    {
        if (GUILayout.Button("Create SpawnManager"))
        {
            if (spawnManagerPrefab != null)
            {
                GameObject spawnManagerInstance = PrefabUtility.InstantiatePrefab(spawnManagerPrefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(spawnManagerInstance, "Create SpawnManager");
                Selection.activeGameObject = spawnManagerInstance;
            }
            else
            {
                Debug.LogError("SpawnManager prefab is missing. Please assign the prefab in the SDKManager script.");
            }
        }
    }


    private void CheckPlayerSetup()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Player setup check", EditorStyles.boldLabel);
        SDK_PlayerSetup playersetup = GameObject.FindAnyObjectByType<SDK_PlayerSetup>();

        if (playersetup == null)
        {
            EditorGUILayout.HelpBox("Player setup does not exist in the scene.", MessageType.Error);
            CreatePlayerSetup();
        }
        else
        {
            EditorGUILayout.HelpBox("Player setup check is successful.", MessageType.Info);
        }
    }

    private void CreatePlayerSetup()
    {
        if (GUILayout.Button("Create player setup"))
        {
            if (playerSetupPrefab != null)
            {
                GameObject playerSetupInstance = PrefabUtility.InstantiatePrefab(playerSetupPrefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(playerSetupInstance, "Create PlayerSetup");
                Selection.activeGameObject = playerSetupInstance;
            }
            else
            {
                Debug.LogError("player setup prefab is missing. Please assign the prefab in the SDKManager script.");
            }
        }
    }



    private void DrawAddressablePathVerification()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Addressables check", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Addressable Remote Load Path:", GUILayout.Width(200));

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var contentVersion = settings.profileSettings.GetValueByName(settings.activeProfileId, "Remote.LoadPath");

        var remoteLoadPath = contentVersion;

        GUILayout.Label(remoteLoadPath);
        string addressablePathMessage = "";
        string expectedPath = "https://hyper.vecos.us/public/ressources/vecosSpace/[BuildTarget]";
        if (remoteLoadPath == expectedPath)
        {
            addressablePathMessage = "Addressable path is checked successfully.";
        }
        else
        {
            addressablePathMessage = "Wrong addressable path. Expected: " + expectedPath;
            if (GUILayout.Button("Fix Path"))
            {
                settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", expectedPath);
            }
        }


        GUILayout.EndHorizontal();

        // Display addressable path message
        if (!string.IsNullOrEmpty(addressablePathMessage))
        {
            if (addressablePathMessage.Contains("Wrong"))
            {
                EditorGUILayout.HelpBox(addressablePathMessage, MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox(addressablePathMessage, MessageType.Info);
            }
        }
    }
}