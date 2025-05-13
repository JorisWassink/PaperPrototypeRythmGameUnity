using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MovingBlock : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private Rigidbody _rigidbody;
    private Renderer _renderer;
    [SerializeField] private float speed;

    private void Start()
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
}
