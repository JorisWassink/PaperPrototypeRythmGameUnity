using System.Collections.Generic;
using UnityEngine;

public class KeyMaterialMapper : MonoBehaviour
{
    public static KeyMaterialMapper Instance { get; private set; }

    [System.Serializable]
    public struct KeyMaterialPair
    {
        public KeyCode key;
        public Material material;
    }

    public KeyMaterialPair[] keyMaterialPairs;

    public Dictionary<KeyCode, Material> KeyMaterialMap { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        KeyMaterialMap = new Dictionary<KeyCode, Material>();
        foreach (var pair in keyMaterialPairs)
        {
            if (!KeyMaterialMap.ContainsKey(pair.key))
            {
                KeyMaterialMap.Add(pair.key, pair.material);
            }
        }
    }

    public Material GetMaterial(KeyCode key)
    {
        return KeyMaterialMap.TryGetValue(key, out var mat) ? mat : null;
    }
}