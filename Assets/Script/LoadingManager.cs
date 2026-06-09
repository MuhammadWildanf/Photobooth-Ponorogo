using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    public GameObject loadingScreen;
    public Slider progressBar;
    public TMP_Text progressText;
    public float fakeLoadingTime = 2f; // Waktu loading palsu untuk menampilkan progress hingga 80%
    public GameManager gameManager;
    public RectTransform Mask;
    public float minX;
    public float maxX;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateFill(float value)
    {
        // Hitung posisi baru berdasarkan nilai slider (value 0-1)
        float newXPosition = Mathf.Lerp(minX, maxX, value);
        Mask.localPosition = new Vector3(newXPosition, Mask.localPosition.y, 0);
    }

    public void StartLoadingScreen()
    {
        loadingScreen.SetActive(true);
        StartCoroutine(LoadingStage1());
    }

    private IEnumerator LoadingStage1()
    {
        // Tahap 1: Loading palsu hingga 80%
        float elapsedTime = 0f;
        while (elapsedTime < fakeLoadingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fakeLoadingTime) * 0.9f;
            progressBar.value = progress;
            UpdateFill(progress);
            progressText.text = (progress * 100f).ToString("F0") + "%";
            yield return null;
        }
    }

    public void FinishLoadingScreen(GameObject targetObject, string state)
    {
        StartCoroutine(LoadingStage2(targetObject, state));
    }

    private IEnumerator LoadingStage2(GameObject targetObject, string state)
    {
        float startProgress = progressBar.value; // Progress bar saat ini
        float endProgress = 1f; // 100%
        float progress = startProgress;

        // Tahap 2: Selesaikan loading ke 100%
        while (progress < endProgress)
        {
            progress += Time.deltaTime / (fakeLoadingTime / 2); // Percepat ke 100%
            if (progress > endProgress) progress = endProgress; // Pastikan tidak melebihi 100%
            UpdateFill(progress);
            progressBar.value = progress;
            progressText.text = (progress * 100f).ToString("F0") + "%";
            yield return null;
        }

        gameManager.ChangeState(state);
        loadingScreen.SetActive(false);
        targetObject.SetActive(true);
    }
}
