using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Animasi Bernapas (Breathing)")]
    [Tooltip("Aktifkan animasi membesar/mengecil saat diam")]
    public bool enableBreathing = true;
    [Tooltip("Seberapa cepat animasinya")]
    public float breatheSpeed = 2.5f;
    [Tooltip("Seberapa besar ukurannya saat mengembang maksimal")]
    public float breatheScaleMultiplier = 1.05f;

    [Header("Animasi Ditekan (Touch)")]
    [Tooltip("Ukuran mengecil saat jari menyentuh (misal 0.9 = 90%)")]
    public float pushScaleMultiplier = 0.9f;
    [Tooltip("Kecepatan transisi saat ditekan dan dilepas")]
    public float touchTransitionSpeed = 15f;

    private Vector3 originalScale;
    private bool isPressed = false;

    void Start()
    {
        // Simpan ukuran aslinya
        originalScale = transform.localScale;
    }

    void Update()
    {
        Vector3 targetScale = originalScale;

        if (isPressed)
        {
            // Jika disentuh, ukuran ditargetkan ke pushScale (mengecil/membal)
            targetScale = originalScale * pushScaleMultiplier;
        }
        else
        {
            if (enableBreathing)
            {
                // Animasi napas menggunakan gelombang Sinus (bergerak naik turun secara halus)
                // Time.unscaledTime digunakan agar animasi tetap jalan meski game di-pause
                float wave = (Mathf.Sin(Time.unscaledTime * breatheSpeed) + 1f) / 2f; 
                float currentMultiplier = Mathf.Lerp(1f, breatheScaleMultiplier, wave);
                targetScale = originalScale * currentMultiplier;
            }
        }

        // Terapkan pergerakan yang halus menuju ukuran target (menggunakan deltaTime)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * touchTransitionSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Jari menyentuh layar / Mouse di-klik
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Jari dilepas dari layar / Mouse dilepas
        isPressed = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Jari tergeser ke luar kotak gambar sebelum dilepas
        isPressed = false;
    }
}
