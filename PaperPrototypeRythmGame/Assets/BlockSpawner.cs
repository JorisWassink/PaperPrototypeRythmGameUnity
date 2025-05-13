using UnityEngine;
using System.Collections;

public class BlockSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject blockPrefab;

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public int numberOfBlocks = 10;

    [Header("Delays")]
    public float initialDelay = 2f;      // Delay before first block
    public float spawnInterval = 1f;     // Delay between blocks

    void Start()
    {
        StartCoroutine(SpawnBlocks());
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    IEnumerator SpawnBlocks()
    {
        // Wait before starting
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < numberOfBlocks; i++)
        {
            SpawnBlock();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnBlock()
    {
        Instantiate(blockPrefab, spawnPoint.position, spawnPoint.rotation, transform);
    }
}
