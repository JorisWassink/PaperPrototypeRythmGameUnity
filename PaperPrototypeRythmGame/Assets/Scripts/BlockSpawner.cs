using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections;
using UnityEngine.Serialization;

[System.Serializable]
public class NoteList
{
    public GameObject note;
    public float InitialDelay;
    public float spawnInterval;
    public int spawnCount;
    public float speed;
    [Tooltip("Ignored if note is not a LongBlock — always set to 1.")]
    public float holdDurationInSeconds = 1f;
    
    public void OnValidate()
    {
        if (note != null && note.GetComponent<LongBlock>() == null)
        {
            holdDurationInSeconds = 1f;
        }
    }
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
                SpawnBlock(note);
                yield return new WaitForSeconds(note.spawnInterval + note.holdDurationInSeconds);
            }
        }
    }

    void SpawnBlock(NoteList list)
    {
        var spawnedNote = Instantiate(list.note, spawnPoint.position, spawnPoint.rotation, transform);
        spawnedNote.GetComponent<Renderer>().material = blockMaterial;
        spawnedNote.GetComponent<MovingBlock>().speed = list.speed;
        if (spawnedNote.GetComponent<LongBlock>() != null)
        {
            spawnedNote.GetComponent<LongBlock>().length = list.holdDurationInSeconds * list.speed;
        }

    }
}
