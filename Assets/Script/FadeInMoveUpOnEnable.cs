using UnityEngine;
using DG.Tweening;

public class FadeInMoveUpOnEnable : MonoBehaviour
{
    [Header("Animation Settings")]
    public float fadeDuration = 0.8f;
    public float moveDistance = 50f; // jarak dari bawah
    public float moveDuration = 0.8f;
    public Ease easeType = Ease.OutCubic;

    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;

    void Awake()
    {
        // Simpan posisi asli
        originalPosition = transform.localPosition;

        // Tambahkan CanvasGroup jika belum ada (untuk kontrol alpha)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // Reset posisi dan alpha setiap aktif
        transform.localPosition = originalPosition - new Vector3(0, moveDistance, 0);
        canvasGroup.alpha = 0f;

        // Hentikan animasi sebelumnya jika ada
        DOTween.Kill(transform);
        DOTween.Kill(canvasGroup);

        // Jalankan animasi paralel (fade + gerak naik)
        Sequence seq = DOTween.Sequence();

        seq.Join(canvasGroup.DOFade(1f, fadeDuration));
        seq.Join(transform.DOLocalMoveY(originalPosition.y, moveDuration).SetEase(easeType));

        seq.Play();
    }
}
