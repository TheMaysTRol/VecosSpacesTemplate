using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class SDK_ObjectControl : MonoBehaviour
{
    [Header("Grabbing specific")]
    public bool isGrabbable = true;
    public bool isDistanceGrabbable = true;
    public UnityEvent<GameObject> onGrabObject;
    public UnityEvent<GameObject> onReleaseObject;

    public void OnEnable()
    {
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
