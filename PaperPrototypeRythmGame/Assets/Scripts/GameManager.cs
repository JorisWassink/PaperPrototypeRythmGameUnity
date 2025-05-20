using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float currentPoints;
    [SerializeField] private TextMeshProUGUI pointsText;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddPoints(float points)
    {
        currentPoints += points;
        pointsText.text = ((int)currentPoints).ToString();
    }
}
