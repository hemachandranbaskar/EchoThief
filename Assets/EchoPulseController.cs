using System.Collections;
using UnityEngine;

public class EchoPulseController : MonoBehaviour
{
    public float pulseRadius = 5f;
    public float revealDuration = 2f;
    public LayerMask obstacleLayer;
    public Material revealMaterial;
    public Material defaultMaterial;
    public GameObject pulseRingPrefab;

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One)) // A button
        {
            TriggerPulse();
        }
    }

    void TriggerPulse()
    {
        // Instantiate the ring at controller position
        Instantiate(pulseRingPrefab, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, pulseRadius, obstacleLayer);

        foreach (Collider hit in hits)
        {
            StartCoroutine(RevealObject(hit.gameObject));
        }
    }

    IEnumerator RevealObject(GameObject obj)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            Material[] originalMats = rend.materials;
            rend.material = revealMaterial;

            yield return new WaitForSeconds(revealDuration);

            rend.material = defaultMaterial;
        }
    }
}
