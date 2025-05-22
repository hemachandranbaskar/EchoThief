using UnityEngine;

public class EchoPulseRing : MonoBehaviour
{
    public float maxScale = 5f;
    public float duration = 1f;
    private float timer = 0f;

    private Vector3 initialScale;
    private Material pulseMaterial;
    private Color startColor;

    void Start()
    {
        initialScale = transform.localScale;
        pulseMaterial = GetComponent<Renderer>().material;
        startColor = pulseMaterial.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        transform.localScale = Vector3.Lerp(initialScale, Vector3.one * maxScale, t);

        // Fade out effect
        Color newColor = startColor;
        newColor.a = Mathf.Lerp(1f, 0f, t);
        pulseMaterial.color = newColor;

        if (t >= 1f)
            Destroy(gameObject);
    }
}
