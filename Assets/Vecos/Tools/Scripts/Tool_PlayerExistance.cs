using UnityEngine;

public class Tool_PlayerExistance : MonoBehaviour
{
    public static Tool_PlayerExistance Instance;

    public void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Player destroyed, because another already exists!");
            Destroy(this.gameObject);
        }
    }

    public void Start()
    {
        if (SDK_Player.Instance != null)
        {
            SDK_Player.Instance.onPlayerSpawn.Invoke(this.gameObject);
        }
    }
}
