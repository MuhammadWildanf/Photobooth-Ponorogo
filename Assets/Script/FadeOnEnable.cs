using UnityEngine;
using DG.Tweening;

public class FadeOnEnable : MonoBehaviour
{
    public float fadeDuration = 1f; // durasi fade in
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // tambahkan CanvasGroup jika belum ada
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // mulai dengan transparan penuh
        canvasGroup.alpha = 0f;

        // hentikan animasi sebelumnya jika ada
        //canvasGroup.DOKill();

        // fade dari 0 ke 1
        canvasGroup.DOFade(1f, fadeDuration)
            .SetEase(Ease.OutQuad);
    }
}