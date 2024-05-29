using Photon.Pun;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

[CustomEditor(typeof(SDK_ObjectControl))]
public class SDK_ObjectControlEditor : Editor
{
    SerializedProperty isGrabbable;
    SerializedProperty isDistanceGrabbable;
    SerializedProperty networked;

    GameObject oldTarget;

    void OnEnable()
    {
        // Link the SerializedProperty variables to their corresponding fields in the target script
        isGrabbable = serializedObject.FindProperty("isGrabbable");
        isDistanceGrabbable = serializedObject.FindProperty("isDistanceGrabbable");
        networked = serializedObject.FindProperty("networked");
        oldTarget = target.GetComponent<Transform>().gameObject;
        previousNetworkedValue = !previousNetworkedValue;
    }


    public override void OnInspectorGUI()
    {
        // Update the serialized object's representation
        serializedObject.Update();

        // Draw the default Inspector layout for all other properties
        DrawDefaultInspector();

        // Conditionally draw the extra settings
        HandleNetworkedProp();
        HandleGrabbables();

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }

    bool previousGrabbableValue = true;
    bool firstTimeGrabInit = true;
    public void HandleGrabbables()
    {
        if (EditorApplication.isPlaying)
        {
            if (firstTimeGrabInit)
            {
                firstTimeGrabInit = false;
                previousGrabbableValue = !previousGrabbableValue;
            }
            if (isGrabbable.boolValue && isGrabbable.boolValue != previousGrabbableValue)
            {
                XRGrabInteractable grab = target.GetComponent<XRGrabInteractable>();
                if (grab == null)
                {
                    target.AddComponent<XRGrabInteractable>();
                }
            }
            else if (!isGrabbable.boolValue && isGrabbable.boolValue != previousGrabbableValue)
            {
                XRGrabInteractable grab = target.GetComponent<XRGrabInteractable>();
                XRGeneralGrabTransformer grabTransformer = target.GetComponent<XRGeneralGrabTransformer>();
                if (grab != null)
                {
                    DestroyImmediate(grab);
                }
                if (grabTransformer != null)
                {
                    DestroyImmediate(grabTransformer);
                }
            }
            previousGrabbableValue = isGrabbable.boolValue;
        }
    }

    bool previousNetworkedValue = true;
    public void HandleNetworkedProp()
    {
        if (networked.boolValue != previousNetworkedValue && !EditorApplication.isPlaying)
        {
            if (networked.boolValue)
            {
                PhotonView pv = target.GetComponent<PhotonView>();
                if (!pv)
                {
                    pv = target.AddComponent<PhotonView>();
                    pv.OwnershipTransfer = OwnershipOption.Request;

                }

                PhotonTransformView pvv = target.GetComponent<PhotonTransformView>();
                if (!pvv)
                {
                    pvv = target.AddComponent<PhotonTransformView>();
                    pvv.m_SynchronizeScale = true;
                    pvv.m_UseLocal = false;
                }
            }
            else
            {
                if (!EditorApplication.isPlaying)
                {
                    PhotonView pv = target.GetComponent<PhotonView>();
                    if (pv)
                    {
                        DestroyImmediate(pv);
                    }

                    PhotonTransformView pvv = target.GetComponent<PhotonTransformView>();
                    if (pvv)
                    {
                        DestroyImmediate(pvv);
                    }
                }
            }
        }
        previousNetworkedValue = networked.boolValue;
    }



    private void OnDisable()
    {
        if (target == null)
        {
            // Perform cleanup by deleting related scripts
            CleanUp();
        }
    }

    public void CleanUp()
    {
        PhotonView pv = oldTarget.GetComponent<PhotonView>();
        if (pv)
        {
            DestroyImmediate(pv);
        }

        PhotonTransformView pvv = oldTarget.GetComponent<PhotonTransformView>();
        if (pvv)
        {
            DestroyImmediate(pvv);
        }

        XRGrabInteractable grab = oldTarget.GetComponent<XRGrabInteractable>();
        XRGeneralGrabTransformer grabTransformer = oldTarget.GetComponent<XRGeneralGrabTransformer>();
        if (grab != null)
        {
            DestroyImmediate(grab);
        }
        if (grabTransformer != null)
        {
            DestroyImmediate(grabTransformer);
        }
    }

}
