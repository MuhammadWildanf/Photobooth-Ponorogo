using UnityEngine;
using System.Collections;

public class ThemeCarousel : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Masukkan Style 1 sampai Style 6 ke sini secara berurutan")]
    public GameObject[] themePanels;

    [Header("Referensi Script")]
    public WebcamComfy webcamComfy;

    [Header("Animasi Transisi")]
    [Tooltip("Kecepatan transisi fade (dalam detik)")]
    public float fadeDuration = 0.25f;

    private int currentIndex = 0;
    private Coroutine fadeRoutine;
    private CanvasGroup[] canvasGroups;

    void Start()
    {
        // Siapkan CanvasGroup untuk semua panel agar bisa kita atur transparasinya (fade)
        canvasGroups = new CanvasGroup[themePanels.Length];
        
        for (int i = 0; i < themePanels.Length; i++)
        {
            if (themePanels[i] != null)
            {
                // Ambil CanvasGroup, jika belum ada maka script akan menambahkannya secara otomatis!
                canvasGroups[i] = themePanels[i].GetComponent<CanvasGroup>();
                if (canvasGroups[i] == null)
                {
                    canvasGroups[i] = themePanels[i].AddComponent<CanvasGroup>();
                }
                
                // Matikan semuanya di awal kecuali index 0
                canvasGroups[i].alpha = (i == currentIndex) ? 1f : 0f;
                themePanels[i].SetActive(i == currentIndex);
            }
        }

        // Sinkronisasi otomatis ke script utama di awal
        if (webcamComfy != null)
        {
            webcamComfy.SelectTheme(currentIndex);
        }
    }

    public void NextTheme()
    {
        currentIndex++;
        if (currentIndex >= themePanels.Length)
        {
            currentIndex = 0; 
        }

        ChangeThemeSmoothly(currentIndex);
    }

    public void PrevTheme()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = themePanels.Length - 1; 
        }

        ChangeThemeSmoothly(currentIndex);
    }

    private void ChangeThemeSmoothly(int nextIndex)
    {
        // Langsung update tema di script utama
        if (webcamComfy != null)
        {
            webcamComfy.SelectTheme(nextIndex);
        }

        // Hentikan animasi sebelumnya jika user nge-klik beruntun terlalu cepat
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        
        // Mulai animasi fade yang baru
        fadeRoutine = StartCoroutine(FadeTransition(nextIndex));
    }

    private IEnumerator FadeTransition(int nextIndex)
    {
        if (themePanels[nextIndex] != null)
        {
            themePanels[nextIndex].SetActive(true);
        }

        float elapsedTime = 0f;
        
        // Simpan nilai alpha awal untuk semua panel
        float[] startAlphas = new float[themePanels.Length];
        for (int i = 0; i < themePanels.Length; i++)
        {
            if (canvasGroups[i] != null)
            {
                startAlphas[i] = canvasGroups[i].alpha;
            }
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            for (int i = 0; i < themePanels.Length; i++)
            {
                if (canvasGroups[i] == null || themePanels[i] == null) continue;

                if (i == nextIndex)
                {
                    // Fade in yang baru
                    canvasGroups[i].alpha = Mathf.Lerp(startAlphas[i], 1f, t);
                }
                else if (themePanels[i].activeSelf)
                {
                    // Fade out yang lain jika sedang aktif
                    canvasGroups[i].alpha = Mathf.Lerp(startAlphas[i], 0f, t);
                }
            }

            yield return null; // Tunggu frame selanjutnya
        }

        // Pastikan nilai akhir sempurna dan nonaktifkan panel yang tidak dipakai
        for (int i = 0; i < themePanels.Length; i++)
        {
            if (canvasGroups[i] == null || themePanels[i] == null) continue;

            if (i == nextIndex)
            {
                canvasGroups[i].alpha = 1f;
            }
            else
            {
                canvasGroups[i].alpha = 0f;
                themePanels[i].SetActive(false);
            }
        }
    }
}
