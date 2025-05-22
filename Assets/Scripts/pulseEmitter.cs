using Oculus.Haptics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pulseEmitter : MonoBehaviour
{
    [Header("Visual")]
    public GameObject pulseScannerPrefab;
    [SerializeField] private Material effectMeshMaterial; // The black material on EffectMesh
    [SerializeField] private float normalAlpha = 1.0f; // Fully opaque (dark)
    public float duration = 10f;
    public float size = 500;

    [Header("Haptics")]
    public HapticClip sonarClip;
    HapticClipPlayer _playerRight;
    [SerializeField] private float hapticDuration = 0.1f;
    [SerializeField] private float hapticStrength = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioSource pulseAudioSource;
    [SerializeField] private AudioClip pulseSound;
    [SerializeField] private AudioClip reflectionSound;
    [SerializeField] private float maxReflectionDistance = 5f;
    [SerializeField] private LayerMask reflectiveLayers;

    private bool isPulsing = false;

    void Start()
    {
        _playerRight = new HapticClipPlayer(sonarClip);

        if (effectMeshMaterial != null)
        {
            SetMaterialAlpha(normalAlpha);
        }
        else
        {
            Debug.LogError("EffectMesh Material not assigned!");
        }
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One)) // A button
        {
            if (!isPulsing) PulseEffect();
        }

        CheckProximityForHaptics();
    }

    public void SetMaterialAlpha(float alpha)
    {
        if (effectMeshMaterial != null)
        {
            Color color = effectMeshMaterial.color;
            color.a = alpha;
            effectMeshMaterial.color = color;
        }
    }

    private void PulseEffect()
    {
        isPulsing = true;

        GameObject pulseScannerObj = Instantiate(pulseScannerPrefab, gameObject.transform.position, Quaternion.identity);
        ParticleSystem pulseParticleSystem = pulseScannerPrefab.transform.GetComponent<ParticleSystem>();

        //OVRInput.SetControllerVibration(0.2f, hapticStrength, OVRInput.Controller.RTouch);
        _playerRight.Play(Controller.Right);

        if (pulseAudioSource != null && pulseSound != null)
        {
            pulseAudioSource.PlayOneShot(pulseSound);
        }

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

        CastAudioReflections();

        Destroy(pulseScannerObj, duration + 1);

        isPulsing = false;
    }

    private void CastAudioReflections()
    {
        Vector3 origin = Camera.main.transform.position;

        // Cast rays in multiple directions to find reflective surfaces
        // Simplified approach - cast 12 rays around the player
        int reflectionCount = 0;
        int maxReflections = 8;

        // Cast rays in key directions
        for (int i = 0; i < 12 && reflectionCount < maxReflections; i++)
        {
            // Get direction based on angle
            float angle = i * 30f * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));

            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxReflectionDistance, reflectiveLayers))
            {
                // Calculate delay and volume based on distance
                float distance = hit.distance;
                float volume = Mathf.Clamp01(1f - (distance / maxReflectionDistance));

                // Play reflection sound with delay based on distance
                float delay = distance / 343f; // Speed of sound in m/s

                StartCoroutine(PlayDelayedReflection(hit.point, delay, volume));
                reflectionCount++;
            }
        }
    }

    private IEnumerator PlayDelayedReflection(Vector3 position, float delay, float volume)
    {
        yield return new WaitForSeconds(delay);

        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.spatialBlend = 1.0f; // Full 3D
        source.volume = volume;
        source.clip = reflectionSound;
        source.Play();

        float destroyDelay = reflectionSound.length + 0.1f;
        Destroy(tempAudio, destroyDelay);
    }

    private void CheckProximityForHaptics()
    {
        // Simple proximity check for walls
        float checkRadius = 0.5f;
        Vector3 headPosition = Camera.main.transform.position;

        Collider[] hitColliders = Physics.OverlapSphere(headPosition, checkRadius, reflectiveLayers);

        if (hitColliders.Length > 0)
        {
            // Found a nearby obstacle - get closest
            float closestDist = checkRadius;
            foreach (Collider col in hitColliders)
            {
                float dist = Vector3.Distance(headPosition, col.ClosestPoint(headPosition));
                if (dist < closestDist)
                {
                    closestDist = dist;
                }
            }

            // Scale haptic intensity by proximity
            float intensity = 1.0f - (closestDist / checkRadius);

            // Generate haptic feedback
            if (intensity > 0.1f) OVRInput.SetControllerVibration(intensity * 0.5f, intensity * hapticStrength, OVRInput.Controller.RTouch);
            else OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);

            if (intensity > 0.1f) OVRInput.SetControllerVibration(intensity * 0.5f, intensity * hapticStrength, OVRInput.Controller.LTouch);
            else OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
        }
    }

    protected virtual void OnDestroy()
    {
        _playerRight?.Dispose();
    }

    protected virtual void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }
}
