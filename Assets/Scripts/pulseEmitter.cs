using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pulseEmitter : MonoBehaviour
{
    public GameObject pulseScannerPrefab;
    public float duration = 10;
    public float size = 500;

    void Start()
    {
        
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One)) // A button
        {
            PulseEffect();
        }
    }

    private void PulseEffect()
    {
        GameObject pulseScannerObj = Instantiate(pulseScannerPrefab, gameObject.transform.position, Quaternion.identity);
        ParticleSystem pulseParticleSystem = pulseScannerPrefab.transform.GetComponent<ParticleSystem>();

        if (pulseParticleSystem != null)
        {
            var main = pulseParticleSystem.main;
            main.startLifetime = duration;
            main.startSize = size;
        }
        else
        {
            Debug.Log("Particle system not found in the child");
        }

        Destroy(pulseScannerObj, duration + 1);
    }
}
