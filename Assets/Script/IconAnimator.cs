using UnityEngine;
public class IconAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public bool animateRotation = false; // Centang jika ingin icon berputar
    public float rotationSpeed = -200f;

    public bool animatePulse = true;     // Centang jika ingin icon berdenyut (Zoom In/Out)
    public float pulseSpeed = 0.5f;
    public float pulseSize = 1.2f;
    private Vector3 originalScale;
    private void Awake()
    {
        originalScale = transform.localScale;
    }
    private void OnEnable()
    {
        // Reset kondisi awal
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;
        // 1. Animasi Putar (Spinning)
        if (animateRotation)
        {
            LeanTween.rotateAroundLocal(gameObject, Vector3.forward, 360f, 1f / (Mathf.Abs(rotationSpeed) / 360f))
                .setLoopClamp();
        }
        // 2. Animasi Denyut (Pulse)
        if (animatePulse)
        {
            LeanTween.scale(gameObject, originalScale * pulseSize, pulseSpeed)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong();
        }
    }
    private void OnDisable()
    {
        // Stop semua animasi saat object mati
        LeanTween.cancel(gameObject);
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;
    }
}