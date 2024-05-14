using UnityEngine;
using UnityEngine.InputSystem.XR;

public class SDK_PlayerSetup : MonoBehaviour
{
    public enum VrRotationType
    {
        Continous,
        Turn
    }

    [Header("PC & Mobile specific")]
    public float PcPlayerSpeed = 2.5f;
    public float PcPlayerShiftSpeed = 5f;
    public float jumpFroce = 4f;
    public float gravityForce = 9.8f;

    [Header("VR specific")]
    public TrackedPoseDriver.TrackingType trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
    public bool canLocomotionMove = true;
    public bool canLocomotionRotate = true;
    public VrRotationType locomotionRotationType = VrRotationType.Continous;
    public float VRPlayerSpeed = 10f;
    public float VRPlayerRotateSpeed = 10f;
    public bool canTeleport = true;

    public void Setup()
    {

    }
}
