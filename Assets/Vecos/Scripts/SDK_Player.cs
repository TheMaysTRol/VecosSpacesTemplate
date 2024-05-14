using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-999)]
public class SDK_Player : MonoBehaviour
{

    public static SDK_Player Instance;

    public UnityEvent<GameObject> onPlayerSpawn = new UnityEvent<GameObject>();

    public GameObject playerObject;
    public Camera playerCamera;

    public void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        onPlayerSpawn.AddListener(OnPlayerSpawn);
    }

    private void OnPlayerSpawn(GameObject player)
    {
        playerObject = player;
        GetCamera();
    }

    public GameObject GetPlayer()
    {
        if (Tool_PlayerExistance.Instance != null)
        {
            playerObject = Tool_PlayerExistance.Instance.gameObject;
            return Tool_PlayerExistance.Instance.gameObject;
        }
        return null;
    }

    public Camera GetCamera()
    {
        if (Tool_PlayerExistance.Instance != null)
        {
            var cam = Tool_PlayerExistance.Instance.transform.GetComponentInChildren<Camera>();
            playerCamera = cam;
            return cam;
        }
        return null;
    }


}
