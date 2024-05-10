using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDK_PlayerSetup : MonoBehaviour
{
    [Header("PC & Mobile specific")]
    public float PcPlayerSpeed = 10f;
    public float PcPlayerShiftSpeed = 10f;

    [Header("VR specific")]
    public float VRPlayerSpeed = 10f;
    public float VRPlayerRotateSpeed = 10f;
    public bool canTeleport = true;
}
