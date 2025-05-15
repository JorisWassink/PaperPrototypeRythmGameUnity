using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

[System.Serializable]
public struct NoteList
{
    public GameObject note;
    public float InitialDelay;
    public float spawnInterval;
    public int spawnCount;
    public float speed;
}

public class BlockSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Material blockMaterial;
    [SerializeField] private NoteList[] noteList;

    void Start()
    {
        StartCoroutine(SpawnBlocks());
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    IEnumerator SpawnBlocks()
    {
        foreach (var note in noteList)
        {
            yield return new WaitForSeconds(note.InitialDelay);

            for (int i = 0; i < note.spawnCount; i++)
            {
                SpawnBlock(note.note, note.speed);
                yield return new WaitForSeconds(note.spawnInterval);
            }
        }
    }

    void SpawnBlock(GameObject note, float speed)
    {
        var spawnedNote = Instantiate(note, spawnPoint.position, spawnPoint.rotation, transform);
        spawnedNote.GetComponent<Renderer>().material = blockMaterial;
        spawnedNote.GetComponent<MovingBlock>().speed = speed;
    }
}
