using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingTimer : MonoBehaviour
{
    public TextMeshProUGUI timer;
    public GameObject loadingscreen;
    public GameManager gameManager;
    public GameObject btnDownload;
    public GameObject spinner;
    private Button button;

    [Header("Progress Bar Settings")]
    public Slider progressBar;
    public Image progressFillImage;
    public TextMeshProUGUI progressText;
    public float fakeLoadingDuration = 15f;
    public float targetFakeProgress = 0.9f;

    private float startTime;
    private bool isRunning = false;
    private float currentProgress = 0f;
    private Coroutine loadingCoroutine;

    public void StartLoading()
    {
        startTime = Time.time;
        if (loadingscreen != null) loadingscreen.SetActive(true);
        isRunning = true;
        currentProgress = 0f;
        UpdateProgressUI(0f);

        if (loadingCoroutine != null) StopCoroutine(loadingCoroutine);
        loadingCoroutine = StartCoroutine(TimerProgressCoroutine());
    }

    private void UpdateProgressUI(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = progress;
        }
        if (progressText != null)
        {
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
        }
    }

    private IEnumerator TimerProgressCoroutine()
    {
        float elapsedTime = 0f;

        // Phase 1: Smoothly progress to targetFakeProgress (e.g. 90%)
        while (isRunning && currentProgress < targetFakeProgress)
        {
            elapsedTime = Time.time - startTime;
            currentProgress = Mathf.Lerp(0f, targetFakeProgress, elapsedTime / fakeLoadingDuration);
            UpdateProgressUI(currentProgress);

            if (timer != null)
            {
                float roundedTime = Mathf.Round(elapsedTime * 100f) / 100f;
                timer.text = roundedTime.ToString() + " seconds";
            }

            yield return null;
        }

        // Phase 2: Keep updating timer/slowing down progress slightly if it takes longer, until stopped
        while (isRunning)
        {
            elapsedTime = Time.time - startTime;
            
            // Very slowly creep towards 98% so it doesn't freeze completely
            if (currentProgress < 0.98f)
            {
                currentProgress += Time.deltaTime * 0.005f;
                UpdateProgressUI(currentProgress);
            }

            if (timer != null)
            {
                float roundedTime = Mathf.Round(elapsedTime * 100f) / 100f;
                timer.text = roundedTime.ToString() + " seconds";
            }

            yield return null;
        }
    }

    public void FinishLoading(GameObject targetObject = null, string state = null)
    {
        if (loadingCoroutine != null) StopCoroutine(loadingCoroutine);
        loadingCoroutine = StartCoroutine(FinishLoadingCoroutine(targetObject, state));
    }

    private IEnumerator FinishLoadingCoroutine(GameObject targetObject, string state)
    {
        isRunning = false;
        
        float startProgress = currentProgress;
        float elapsed = 0f;
        float finishDuration = 0.5f; // Animate to 100% in 0.5s

        while (elapsed < finishDuration)
        {
            elapsed += Time.deltaTime;
            currentProgress = Mathf.Lerp(startProgress, 1.0f, elapsed / finishDuration);
            UpdateProgressUI(currentProgress);
            yield return null;
        }

        currentProgress = 1.0f;
        UpdateProgressUI(1.0f);

        yield return new WaitForSeconds(0.2f); // Stay at 100% briefly for visual impact

        if (loadingscreen != null) loadingscreen.SetActive(false);
        if (targetObject != null) targetObject.SetActive(true);
        if (state != null) gameManager.ChangeState(state);
    }

    public void StopTimer(GameObject targetObject, string state)
    {
        // If we are still running, do a proper finish animation.
        // Otherwise (if already finished), just ensure target state is set.
        if (isRunning)
        {
            FinishLoading(targetObject, state);
        }
        else
        {
            if (targetObject != null) targetObject.SetActive(true);
            if (state != null) gameManager.ChangeState(state);
        }
    }
}
