using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;


public class SDKBuildChcker : EditorWindow
{

    private GameObject spawnManagerPrefab; // Reference to the SpawnManager prefab


    [MenuItem("Vecos/Build checker")]
    public static void ShowWindow()
    {
        GetWindow<SDKBuildChcker>("Vecos Manager");
    }



    private void OnEnable()
    {
        // Load the SpawnManager prefab
        spawnManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Vecos/Tools/Prefabs/SpawnManagerPrefab.prefab");
    }

    private void OnGUI()
    {
        DrawCameraButtons();

        CheckSpawnManager();

        DrawAddressablePathVerification();
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
                settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath",expectedPath);
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