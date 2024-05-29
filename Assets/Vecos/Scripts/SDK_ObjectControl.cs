using Photon.Pun;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class SDK_ObjectControl : MonoBehaviour
{
    public enum Hand{
        LeftHand,
        RightHand,
        PC
    }

    [Header("Grabbing specific")]
    public bool isGrabbable = true;
    public bool isDistanceGrabbable = true;
    public UnityEvent<GameObject,Hand> onGrabObject;
    public UnityEvent<GameObject,Hand> onReleaseObject;
#if UNITY_EDITOR
    public PunSceneSettings sceneSettings;
#endif

    public void OnEnable()
    {
#if UNITY_EDITOR
        sceneSettings = AssetDatabase.LoadAssetAtPath<PunSceneSettings>("Assets/Vecos/Photon/PhotonUnityNetworking/Code/Editor/PunSceneSettingsFile.asset");
        OnSceneOpened();
#endif
        PhotonView pv = this.gameObject.GetComponent<PhotonView>();
        if (!pv)
        {
            pv = this.gameObject.AddComponent<PhotonView>();
            pv.OwnershipTransfer = OwnershipOption.Request;

        }

        PhotonTransformView pvv = this.gameObject.GetComponent<PhotonTransformView>();
        if (!pvv)
        {
            pvv = this.gameObject.AddComponent<PhotonTransformView>();
        }
    }

    private void OnSceneOpened()
    {
#if UNITY_EDITOR
        sceneSettings.MinViewIdPerScene.Clear();
        var set = new SceneSetting();
        set.sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneManager.GetActiveScene().path);
        set.sceneName = SceneManager.GetActiveScene().name;
        set.minViewId = 100;
        sceneSettings.MinViewIdPerScene.Add(set);
#endif
    }

    public void OnDestroy()
    {
        PhotonView pv = this.gameObject.GetComponent<PhotonView>();
        if (pv)
        {
            DestroyImmediate(pv);
        }

        PhotonTransformView pvv = this.gameObject.GetComponent<PhotonTransformView>();
        if (pvv)
        {
            DestroyImmediate(pvv);
        }
    }
    public void OnObjectGrabbedDissapear(GameObject selectedObj)
    {
        if (selectedObj == null)
            return;
        Debug.Log("scaled");
        selectedObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

}
