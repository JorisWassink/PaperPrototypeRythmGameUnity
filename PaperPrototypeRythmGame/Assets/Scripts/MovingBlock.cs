using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class MovingBlock : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private Rigidbody _rigidbody;
    public Renderer _renderer;
    
    [SerializeField] public float speed;
    [SerializeField] public float minDistance = 2f;
    
    [SerializeField] private Material meh;
    [SerializeField] private Material nice;
    [SerializeField] private Material great;
    
    [SerializeField] private GameObject smoke;
    
    public Vector3 StartPosition => new Vector3(
        _renderer.bounds.center.x,
        _renderer.bounds.min.y,
        _renderer.bounds.center.z
    );

    public Vector3 EndPosition => new Vector3(
        _renderer.bounds.center.x,
        _renderer.bounds.max.y,
        _renderer.bounds.center.z
    );
    public abstract void StartHolding(GameObject goal);
    public abstract void IsHolding(GameObject goal);
    public abstract void StopHolding(GameObject goal);
    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _renderer = GetComponent<Renderer>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.velocity = -transform.up * speed;
        
        
        var particleRenderer =_particleSystem.gameObject.GetComponent<Renderer>();
        particleRenderer.material = _renderer.material;
    }

    public void DestroyBlock(bool particles)
    {
        Renderer rend = GetComponent<Renderer>();
        rend.enabled = false;
        StartCoroutine(DestroyAfterDelay(_particleSystem.main.duration));
        if (particles)
            _particleSystem.Play();
    }

    private IEnumerator DestroyAfterDelay(float waitTime)
    {
        Debug.Log(waitTime);
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }
    
    public void BlockHit(GameObject goal)
    {
        var rend = goal.GetComponent<Renderer>();
        var worldCenter = rend.bounds.center;
        var maxDistance = rend.bounds.extents.magnitude;

        CalculatePoints(goal.transform.position, worldCenter, maxDistance);
        DestroyBlock(true);
    }

    public void CalculatePoints(Vector3 goal, Vector3 position, float maxDistance)
    {
        var distanceToGoal = Vector3.Distance(goal, position);
        
        var normalized = Mathf.Clamp01(1f - (distanceToGoal / maxDistance));
        var points = normalized * 5f;
        
        GameManager.Instance.AddPoints(points);
        
        // Choose material based on points
        Material selectedMaterial;
        if (points < 1f)
            selectedMaterial = meh;
        else if (points < 2f)
            selectedMaterial = nice;
        else
            selectedMaterial = great;

        // Assign to particle system renderer
        
        GameObject smokePuff = Instantiate(smoke, goal, transform.rotation);

        var particleRenderer = smokePuff.GetComponent<ParticleSystemRenderer>();
        if (particleRenderer != null)
            particleRenderer.material = selectedMaterial;

        ParticleSystem parts = smokePuff.GetComponent<ParticleSystem>();
        float totalDuration = parts.main.duration;
        Destroy(smokePuff, totalDuration);
    }
}
