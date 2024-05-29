using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{

    public enum TestType
    {
        Pc,
        Vr
    }

    public List<Transform> spawnPositions = new List<Transform>();

    public TestType testType;
    public GameObject vrPlayer, pcPlayer;


    public void Awake()
    {
        if (SDK_Player.Instance != null)
        {
            SDK_Player.Instance.onPlayerSpawn.AddListener(OnSpawn);
            SpawnTestPlayer();
        }
    }

    private void OnSpawn(GameObject player)
    {
        SDK_Player.Instance.onPlayerSpawn.RemoveListener(OnSpawn);
        player.transform.position = spawnPositions[Random.Range(0, spawnPositions.Count)].position;
    }

    public void SpawnTestPlayer()
    {
        if (testType == TestType.Pc)
        {
            GameObject player = Instantiate(pcPlayer);
            SDK_Player.Instance.onPlayerSpawn.Invoke(player);
        }
        else
        {
            GameObject player = Instantiate(vrPlayer);
            SDK_Player.Instance.onPlayerSpawn.Invoke(player);
        }
    }
}
