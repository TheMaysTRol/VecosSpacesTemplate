using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<Transform> spawnPositions = new List<Transform>();

    public void Awake()
    {
        if(SDK_Player.Instance != null)
        {
            Debug.Log("test");
            SDK_Player.Instance.onPlayerSpawn.AddListener(OnSpawn);
        }
    }

    private void OnSpawn(GameObject player)
    {
        SDK_Player.Instance.onPlayerSpawn.RemoveListener(OnSpawn);
        player.transform.position = spawnPositions[Random.Range(0, spawnPositions.Count)].position;
    }
}
