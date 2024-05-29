using Photon.Pun;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class SDK_ObjectControl : MonoBehaviour
{
    public enum Hand
    {
        None,
        LeftHand,
        RightHand,
        PC
    }

    [Header("Grabbing specific")]
    public bool isGrabbable = true;
    public bool isDistanceGrabbable = true;
    public UnityEvent<GameObject, Hand> onGrabObject;
    public UnityEvent<GameObject, Hand> onReleaseObject;

    [Header("Networking specific")]
    public bool networked = true;


    private XRGrabInteractable grabbableLocal;
#if UNITY_EDITOR
    private PunSceneSettings sceneSettings;
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
            pvv.m_SynchronizeScale = true;
            pvv.m_UseLocal = false;
        }

        grabbableLocal = this.GetComponent<XRGrabInteractable>();
        if (grabbableLocal)
        {
            grabbableLocal.selectEntered.AddListener(OnGrabbed);
        }
    }

    private void OnDisable()
    {
        if (grabbableLocal)
        {
            grabbableLocal.selectEntered.RemoveListener(OnGrabbed);
        }
    }

    public void Start()
    {
        grabbableLocal = this.GetComponent<XRGrabInteractable>();
        if (grabbableLocal)
        {
            grabbableLocal.selectEntered.AddListener(OnGrabbed);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs argu)
    {
        if (argu.interactorObject.transform.name.ToLower().Contains("right"))
        {
            onGrabObject.Invoke(this.gameObject, Hand.RightHand);
        }
        else
        {
            onGrabObject.Invoke(this.gameObject, Hand.LeftHand);
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

}


