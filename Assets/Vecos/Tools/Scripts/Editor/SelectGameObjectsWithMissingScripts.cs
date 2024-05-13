using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement; //3

public class SelectGameObjectsWithMissingScripts : Editor
{
    [MenuItem("Tools/NullRefs/Select Objects with Missing Components")]
    public static void SelectMissingComponents()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        Selection.objects = FindObjectsWithMissingComponents(allObjects);
    }

    private static GameObject[] FindObjectsWithMissingComponents(GameObject[] allObjects)
    {
        System.Collections.Generic.List<GameObject> objectsWithMissingComponents = new System.Collections.Generic.List<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (HasMissingComponent(go))
            {
                objectsWithMissingComponents.Add(go);
            }

            // Recursively check children
            Transform[] children = go.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (HasMissingComponent(child.gameObject))
                {
                    objectsWithMissingComponents.Add(child.gameObject);
                }
            }
        }

        return objectsWithMissingComponents.ToArray();
    }

    private static bool HasMissingComponent(GameObject go)
    {
        Component[] components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                return true;
            }
        }
        return false;
    }

    [MenuItem("Tools/NullRefs/Delete Objects with Missing Components")]
    private static void DeleteAllComp()
    {
        SelectMissingComponents();
        foreach (GameObject obj in Selection.objects)
        {
            DeleteMissingComponentsInObject(obj);
        }
    }


    private static void DeleteMissingComponentsInObject(GameObject go)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
       
    }
}