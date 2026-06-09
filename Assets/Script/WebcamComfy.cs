using Michsky.MUIP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebcamComfy : MonoBehaviour
{
    [Header("UI References")]
    public CustomDropdown myDropdown;
    public TMP_Dropdown webcamDropdown;
    public RawImage webcam;
    public RawImage responseImage;
    public GameObject startButton;
    public GameObject hasil;
    public GameObject photo;
    public GameObject select;
    public GameObject blitz;
    public GameObject text;
    public GameObject Male;
    public GameObject Female;
    public TextMeshProUGUI countdown;
    public GameObject qrContainer; // Drag 'QR' object kesini di Unity Inspector

    [Header("Managers")]
    public DriveQR Drive;
    public LoadingManager loading;
    public LoadingTimer loadingtimer;
    public GameManager gameManager;

    [Header("Printer Settings")]
    [Tooltip("Gunakan C:/hotfolder/print untuk lokal, atau \\\\192.168.0.184\\nama_folder untuk beda PC")]
    public string printerHotfolderPath = "C:/hotfolder/print";

    [Header("Config UI Settings")]
    public GameObject configPanel;
    public TMP_InputField printerInputField;

    [Header("Settings")]
    public int countdownTime = 3;

    // ===⬇️⬇️ Tambahan untuk prompt ===
    [Header("ComfyUI Prompts")]
    [TextArea(5, 20)] public string malePrompt;
    [TextArea(5, 20)] public string femalePrompt;
    [TextArea(5, 20)] public string hijabPrompt;
    // ===⬆️⬆️===


    [Header("Theme Prompts")]

    // 4 style untuk tema 1
    [TextArea(2, 5)] public string[] theme1Styles = new string[4];

    // 4 style untuk tema 2
    [TextArea(2, 5)] public string[] theme2Styles = new string[4];

    [TextArea(2, 5)] public string[] theme3Styles = new string[4];
    [TextArea(2, 5)] public string[] theme4Styles = new string[4];
    [TextArea(2, 5)] public string[] theme5Styles = new string[4];
    [TextArea(2, 5)] public string[] theme6Styles = new string[4];

    [Header("Group Mode Prompts")]
    [TextArea(2, 5)] public string[] theme1GroupStyles = new string[4];
    [TextArea(2, 5)] public string[] theme2GroupStyles = new string[4];
    [TextArea(2, 5)] public string[] theme3GroupStyles = new string[4];
    [TextArea(2, 5)] public string[] theme4GroupStyles = new string[4];
    [TextArea(2, 5)] public string[] theme5GroupStyles = new string[4];
    [TextArea(2, 5)] public string[] theme6GroupStyles = new string[4];

    private string selectedTheme = "Tema 1";
    public int selectedThemeIndex = 0;
    
    [Header("Mode Selection")]
    public string selectedMode = "Single";




    private List<string> webcamNames = new List<string>();
    private WebCamTexture webCamTexture;
    private IEnumerator currentRoutine;
    private string genderselected;
    private string sourceimage;
    private byte[] resultimg;
    private static Texture2D capturedImage;
    private int lastIndexTheme1 = -1;
    private int lastIndexTheme2 = -1;

    private int lastIndexTheme3 = -1;
    private int lastIndexTheme4 = -1;
    private int lastIndexTheme5 = -1;
    private int lastIndexTheme6 = -1;

    [Header("UI Settings")]
    public GameObject alertPopup;       // Drag GAMBAR Alert Anda ke sini (GameObject Image)
    public Button generateButton;       // Drag Button Video
    public float alertDuration = 3.0f;  // Durasi alert muncul

    void Start()
    {
        // Load printer path dari PlayerPrefs, jika belum ada pakai default dari Inspector
        printerHotfolderPath = PlayerPrefs.GetString("PrinterHotfolderPath", printerHotfolderPath);

        InitializeWebcams();
        if (alertPopup != null) alertPopup.SetActive(false);
        if (configPanel != null) configPanel.SetActive(false);
    }

    // ============================================================
    // ⚙️ RUNTIME CONFIGURATION METHODS
    // ============================================================
    public void OpenConfigPanel()
    {
        if (configPanel != null)
        {
            configPanel.SetActive(true);
            if (printerInputField != null)
            {
                printerInputField.text = printerHotfolderPath;
            }
        }
    }

    public void CloseConfigPanel()
    {
        if (configPanel != null)
        {
            configPanel.SetActive(false);
        }
    }

    public void SavePrinterConfig()
    {
        if (printerInputField != null)
        {
            printerHotfolderPath = printerInputField.text.Trim();
            PlayerPrefs.SetString("PrinterHotfolderPath", printerHotfolderPath);
            PlayerPrefs.Save();
            Debug.Log("💾 Printer Hotfolder Path saved: " + printerHotfolderPath);
        }
        CloseConfigPanel();
    }

    private void InitializeWebcams()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamNames.Clear();

        if (devices.Length > 0)
        {
            webcamNames.Add("Pilih Webcam");
            foreach (var cam in devices) webcamNames.Add(cam.name);
            startButton.SetActive(false);
        }
        else
        {
            webcamNames.Add("No Webcam Detected");
            startButton.SetActive(false);
        }

        webcamDropdown.ClearOptions();
        webcamDropdown.AddOptions(webcamNames);
        webcamDropdown.onValueChanged.AddListener(OnWebcamSelected);
    }

    private void OnWebcamSelected(int index)
    {
        if (index <= 0) { startButton.SetActive(false); return; }

        myDropdown.gameObject.SetActive(false);
        webcamDropdown.gameObject.SetActive(false);
        startButton.SetActive(true);
        StartWebcam(index);
    }

    private void StartWebcam(int index)
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
            webCamTexture.Stop();

        string selectedDeviceName = webcamNames[index];
        webCamTexture = new WebCamTexture(selectedDeviceName, 1280, 720, 30);
        webcam.texture = webCamTexture;
        webCamTexture.Play();
        gameManager.ChangeState("HOME");
    }

    public void SelectGender(string gender)
    {
        genderselected = gender;
        Debug.Log(genderselected);
    }

    public void SelectTheme(int index)
    {
        selectedThemeIndex = index;
        if (index == 0) selectedTheme = "Tema 1";
        else if (index == 1) selectedTheme = "Tema 2";
        else if (index == 2) selectedTheme = "Tema 3";
        else if (index == 3) selectedTheme = "Tema 4";
        else if (index == 4) selectedTheme = "Tema 5";
        else if (index == 5) selectedTheme = "Tema 6";
    }

    public void SetPhotoMode(string mode)
    {
        // Parameter string: "Single" atau "Group"
        selectedMode = mode;
        Debug.Log("📸 Mode terpilih: " + selectedMode);
    }

    private string GetRandomThemePrompt()
    {
        string style = "";

        if (selectedTheme == "Tema 1")
        {
            string[] styles = (selectedMode == "Group") ? theme1GroupStyles : theme1Styles;
            int idx;
            do { idx = UnityEngine.Random.Range(0, styles.Length); }
            while (idx == lastIndexTheme1 && styles.Length > 1);
            lastIndexTheme1 = idx;
            style = styles[idx];
        }
        else if (selectedTheme == "Tema 2")
        {
            string[] styles = (selectedMode == "Group") ? theme2GroupStyles : theme2Styles;
            int idx;
            do { idx = UnityEngine.Random.Range(0, styles.Length); }
            while (idx == lastIndexTheme2 && styles.Length > 1);
            lastIndexTheme2 = idx;
            style = styles[idx];
        }
        else if (selectedTheme == "Tema 3")
        {
            string[] styles = (selectedMode == "Group") ? theme3GroupStyles : theme3Styles;
            int idx;
            do { idx = UnityEngine.Random.Range(0, styles.Length); }
            while (idx == lastIndexTheme3 && styles.Length > 1);
            lastIndexTheme3 = idx;
            style = styles[idx];
        }
        else if (selectedTheme == "Tema 4")
        {
            string[] styles = (selectedMode == "Group") ? theme4GroupStyles : theme4Styles;
            int idx;
            do { idx = UnityEngine.Random.Range(0, styles.Length); }
            while (idx == lastIndexTheme4 && styles.Length > 1);
            lastIndexTheme4 = idx;
            style = styles[idx];
        }
        else if (selectedTheme == "Tema 5")
        {
            string[] styles = (selectedMode == "Group") ? theme5GroupStyles : theme5Styles;
            int idx;
            do { idx = UnityEngine.Random.Range(0, styles.Length); }
            while (idx == lastIndexTheme5 && styles.Length > 1);
            lastIndexTheme5 = idx;
            style = styles[idx];
        }
        else if (selectedTheme == "Tema 6")
        {
            string[] styles = (selectedMode == "Group") ? theme6GroupStyles : theme6Styles;
            int idx;
            do { idx = UnityEngine.Random.Range(0, styles.Length); }
            while (idx == lastIndexTheme6 && styles.Length > 1);
            lastIndexTheme6 = idx;
            style = styles[idx];
        }
        else
        {
            Debug.LogWarning("❗ Tema belum dipilih!");
            return "";
        }

        string finalPrompt = style;
        Debug.Log("🎨 Final Prompt (Theme + Random Style):\n" + finalPrompt);
        return finalPrompt;
    }

    public void CountdownAndMakePhoto()
    {
        if (currentRoutine != null) return;
        currentRoutine = CountdownRoutine();
        StartCoroutine(currentRoutine);
    }

    private IEnumerator CountdownRoutine()
    {
        for (int i = countdownTime; i > 0; i--)
        {
            countdown.text = i.ToString();
            LeanTween.scale(text, new Vector3(5f, 5f, 5f), 0.5f).setEaseOutQuad();
            yield return new WaitForSeconds(1f);
            LeanTween.scale(text, Vector3.zero, 0f);
        }

        StartCoroutine(FlashEffect());
        currentRoutine = null;
    }

    private IEnumerator FlashEffect()
    {
        blitz.SetActive(true);
        var cg = blitz.GetComponent<CanvasGroup>();
        LeanTween.alphaCanvas(cg, 1f, 0.1f);
        yield return new WaitForSeconds(0.5f);
        LeanTween.alphaCanvas(cg, 0f, 0.1f);
        yield return new WaitForSeconds(0.2f);
        blitz.SetActive(false);

        CaptureFrame();
    }

    private void CaptureFrame()
    {
        // 1. Ambil frame asli (High Res) untuk UI
        Texture2D originalFrame = new Texture2D(webCamTexture.width, webCamTexture.height);
        originalFrame.SetPixels(webCamTexture.GetPixels());
        originalFrame.Apply();

        // 2. Resize untuk Upload (Low Res) - 960x540 (Target ~300KB)
        Texture2D resizedFrame = ResizeTexture(originalFrame, 960, 540);

        // 3. Encode yang sudah di-resize
        byte[] resizedBytes = resizedFrame.EncodeToPNG();
        sourceimage = Convert.ToBase64String(resizedBytes);

        // 3.5 Simpan hasil capture ke Assets/Captures
        string captureFolder = Path.Combine(Application.dataPath, "Captures");
        if (!Directory.Exists(captureFolder)) Directory.CreateDirectory(captureFolder);

        string captureFilename = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string capturePath = Path.Combine(captureFolder, captureFilename);
        File.WriteAllBytes(capturePath, resizedBytes);
        Debug.Log("📸 Captured image saved to: " + capturePath);

        // 4. Set UI pakai yang High Res biar tajam
        capturedImage = originalFrame;

        photo.SetActive(false);
        gameManager.ChangeState("Loading");
        loadingtimer.StartLoading();

        StartCoroutine(UploadToComfy(resizedBytes, "webcam_input.png"));
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = rt;

        // Copy & Scale
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    private Texture2D CropTexture(Texture2D tex, int x, int y, int width, int height)
    {
        Texture2D cropped = new Texture2D(width, height);
        cropped.SetPixels(tex.GetPixels(x, y, width, height));
        cropped.Apply();
        return cropped;
    }

    // === COMFY UI SECTION ===

    private IEnumerator UploadToComfy(byte[] imageData, string filename)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, filename, "image/png");

        using (UnityWebRequest upload = UnityWebRequest.Post("http://127.0.0.1:8188/upload/image", form))
        {
            yield return upload.SendWebRequest();

            if (upload.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Upload failed: " + upload.error);
                yield break;
            }

            // Ambil nama file hasil upload dari respons ComfyUI
            string responseText = upload.downloadHandler.text;
            Debug.Log("Upload response: " + responseText);

            // Gunakan MiniJSON bawaan Unity (bisa copy dari Unity docs)
            var responseDict = MiniJSON.Json.Deserialize(responseText) as Dictionary<string, object>;
            if (responseDict == null || !responseDict.ContainsKey("name"))
            {
                Debug.LogError("Upload succeeded but response invalid!");
                yield break;
            }

            string uploadedName = responseDict["name"].ToString();
            Debug.Log("✅ Uploaded to ComfyUI as: " + uploadedName);

            // Jalankan workflow pakai nama yang dikembalikan
            yield return RunComfyWorkflow(uploadedName);
        }
    }

    private string JsonEscape(string str)
    {
        return str
            .Replace("\\", "\\\\") // Escape backslashes
            .Replace("\"", "\\\"") // Escape double quotes
            .Replace("\n", "\\n")  // Escape newline
            .Replace("\r", "\\r"); // Escape carriage return
    }

    private class UploadResult { public string filename; }

    private IEnumerator UploadTemplate(int index, UploadResult result)
    {
        string templateFolder = Path.Combine(Application.streamingAssetsPath, "template");
        string selectedTemplateName = "template_" + index + ".png";
        string selectedTemplatePath = Path.Combine(templateFolder, selectedTemplateName);

        if (!File.Exists(selectedTemplatePath))
        {
            selectedTemplateName = "template_" + index + ".jpg";
            selectedTemplatePath = Path.Combine(templateFolder, selectedTemplateName);
            if (!File.Exists(selectedTemplatePath))
            {
                Debug.LogError("❌ File template (maskot) tidak ditemukan: " + selectedTemplatePath);
                result.filename = null;
                yield break;
            }
        }

        byte[] templateBytes = File.ReadAllBytes(selectedTemplatePath);
        WWWForm templateForm = new WWWForm();
        templateForm.AddBinaryData("image", templateBytes, selectedTemplateName, "image/png");

        using (UnityWebRequest templateReq = UnityWebRequest.Post("http://127.0.0.1:8188/upload/image", templateForm))
        {
            yield return templateReq.SendWebRequest();
            if (templateReq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Gagal upload maskot: " + templateReq.error);
                result.filename = null;
            }
            else
            {
                string responseText = templateReq.downloadHandler.text;
                var responseDict = MiniJSON.Json.Deserialize(responseText) as Dictionary<string, object>;
                if (responseDict != null && responseDict.ContainsKey("name"))
                {
                    result.filename = responseDict["name"].ToString();
                }
                else
                {
                    result.filename = selectedTemplateName; // fallback
                }
            }
        }
    }

    private void SetNodeImage(Dictionary<string, object> workflow, string nodeId, string filename)
    {
        if (workflow.ContainsKey(nodeId))
        {
            var node = workflow[nodeId] as Dictionary<string, object>;
            if (node != null && node.ContainsKey("inputs"))
            {
                var inputs = node["inputs"] as Dictionary<string, object>;
                if (inputs != null)
                {
                    inputs["image"] = filename;
                }
            }
        }
    }

    private void SetNodePrompt(Dictionary<string, object> workflow, string nodeId, string promptText)
    {
        if (workflow.ContainsKey(nodeId))
        {
            var node = workflow[nodeId] as Dictionary<string, object>;
            if (node != null && node.ContainsKey("inputs"))
            {
                var inputs = node["inputs"] as Dictionary<string, object>;
                if (inputs != null)
                {
                    inputs["value"] = promptText;
                }
            }
        }
    }

    private void SetNodeSeed(Dictionary<string, object> workflow, string nodeId, int seed)
    {
        if (workflow.ContainsKey(nodeId))
        {
            var node = workflow[nodeId] as Dictionary<string, object>;
            if (node != null && node.ContainsKey("inputs"))
            {
                var inputs = node["inputs"] as Dictionary<string, object>;
                if (inputs != null)
                {
                    inputs["seed"] = seed;
                }
            }
        }
    }

    private void LogAiEngineAndModel(Dictionary<string, object> workflow, string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return;

        if (workflow.ContainsKey(nodeId))
        {
            var node = workflow[nodeId] as Dictionary<string, object>;
            if (node != null)
            {
                // -- LOGGING DETEKSI AI ENGINE --
                if (node.ContainsKey("class_type"))
                {
                    string aiType = node["class_type"].ToString();
                    Debug.Log($"🤖 [AI Engine] Memproses gambar menggunakan Node: <color=yellow>{aiType}</color> (Node ID: {nodeId})");
                }
                
                if (node.ContainsKey("inputs"))
                {
                    var inputs = node["inputs"] as Dictionary<string, object>;
                    if (inputs != null && inputs.ContainsKey("model"))
                    {
                        Debug.Log($"🧠 [AI Model] Varian model yang dipakai: <color=cyan>{inputs["model"].ToString()}</color>");
                    }
                }
            }
        }
    }

    private IEnumerator RunComfyWorkflow(string uploadedFilename)
    {
        // 1. Tentukan file workflow JSON berdasarkan mode dan tema/style yang dipilih
        string workflowFile = "";
        if (selectedMode == "Single")
        {
            if (selectedThemeIndex == 3) // Style 4 (REOG)
            {
                workflowFile = "PNRG-SELFIE-SINGLE-MASCOT-REOG.json";
            }
            else if (selectedThemeIndex == 5) // Style 6 (ALL)
            {
                workflowFile = "PNRG-SELFIE-ALL-MASCOT.json";
            }
            else // Style 1, 2, 3, 5
            {
                workflowFile = "PNRG-SELFIE-SINGLE-MASCOT.json";
            }
        }
        else // Group
        {
            if (selectedThemeIndex == 3) // Style 4 (REOG)
            {
                workflowFile = "PNRG-GRUP-SINGLE-MASCOT-REOG.json";
            }
            else if (selectedThemeIndex == 5) // Style 6 (ALL)
            {
                workflowFile = "PNRG-GRUP-ALL-MASCOT.json";
            }
            else // Style 1, 2, 3, 5
            {
                workflowFile = "PNRG-GRUP-SINGLE-MASCOT.json";
            }
        }

        string workflowPath = Path.Combine(Application.streamingAssetsPath, workflowFile);
        
        // Fallback jika file tidak ada
        if (!File.Exists(workflowPath)) 
        {
            Debug.LogError($"❌ File workflow {workflowFile} tidak ditemukan!");
            yield break;
        }

        Debug.Log($"🔌 Menggunakan workflow: {workflowFile}");
        string jsonText = File.ReadAllText(workflowPath);
        Dictionary<string, object> workflow = MiniJSON.Json.Deserialize(jsonText) as Dictionary<string, object>;

        if (workflow == null)
        {
            Debug.LogError("❌ Gagal parse workflow JSON");
            yield break;
        }

        // ============================================================
        // 🔥 LOAD MASKOT (TEMPLATE) DARI FOLDER template
        // ============================================================
        string[] uploadedTemplates = new string[5];
        if (selectedThemeIndex == 5) // Style 6 meload template 0 sampai 4
        {
            Debug.Log("🖼️ [Template] Memilih template ALL (Style 6) - Mengunggah semua template (template_0 sampai template_4)...");
            for (int i = 0; i < 5; i++)
            {
                UploadResult res = new UploadResult();
                yield return UploadTemplate(i, res);
                if (res.filename == null)
                {
                    Debug.LogError($"❌ Gagal upload template_{i}");
                    yield break;
                }
                uploadedTemplates[i] = res.filename;
                Debug.Log($"🖼️ [Template] (ALL) Berhasil mengunggah template ke-{i + 1}: {res.filename}");
            }
        }
        else
        {
            UploadResult res = new UploadResult();
            yield return UploadTemplate(selectedThemeIndex, res);
            if (res.filename == null)
            {
                Debug.LogError($"❌ Gagal upload template_{selectedThemeIndex}");
                yield break;
            }
            uploadedTemplates[selectedThemeIndex] = res.filename;
            Debug.Log($"🖼️ [Template] Memilih template ke-{selectedThemeIndex + 1} (File: {res.filename})");
        }

        // ============================================================
        // 🌆 LOAD BACKGROUND DARI FOLDER background
        // ============================================================
        string bgFolder = Path.Combine(Application.streamingAssetsPath, "background");
        if (!Directory.Exists(bgFolder)) Directory.CreateDirectory(bgFolder);

        string selectedBgName = "BG.png"; // User menamai file-nya BG.png
        string selectedBgPath = Path.Combine(bgFolder, selectedBgName);
        
        if (!File.Exists(selectedBgPath))
        {
            selectedBgName = "BG.jpg";
            selectedBgPath = Path.Combine(bgFolder, selectedBgName);
        }

        string uploadedBgName = selectedBgName;
        // Jika ada file background, upload ke ComfyUI
        if (File.Exists(selectedBgPath))
        {
            byte[] bgBytes = File.ReadAllBytes(selectedBgPath);
            WWWForm bgForm = new WWWForm();
            bgForm.AddBinaryData("image", bgBytes, selectedBgName, "image/png");

            using (UnityWebRequest bgReq = UnityWebRequest.Post("http://127.0.0.1:8188/upload/image", bgForm))
            {
                yield return bgReq.SendWebRequest();
                if (bgReq.result != UnityWebRequest.Result.Success) 
                { 
                    Debug.LogError("❌ Gagal upload background: " + bgReq.error); 
                    yield break; 
                }
                
                string responseText = bgReq.downloadHandler.text;
                var responseDict = MiniJSON.Json.Deserialize(responseText) as Dictionary<string, object>;
                if (responseDict != null && responseDict.ContainsKey("name"))
                {
                    uploadedBgName = responseDict["name"].ToString();
                }
            }
        }
        else
        {
            Debug.LogError("❌ Folder background kosong atau BG.png/BG.jpg tidak ditemukan!");
            yield break;
        }

        // ============================================================
        // 💉 INJEKSI KE NODE JSON 
        // ============================================================
        string newPrompt = GetRandomThemePrompt();
        int randomSeed = UnityEngine.Random.Range(100000, 999999);
        string aiNodeId = "";

        if (selectedMode == "Single")
        {
            if (selectedThemeIndex == 3) // Style 4 (REOG)
            {
                SetNodeImage(workflow, "4", uploadedFilename); // Visitor
                SetNodeImage(workflow, "7", uploadedBgName);    // Background
                SetNodeImage(workflow, "8", uploadedTemplates[3]); // Mascot (template_3)
                SetNodePrompt(workflow, "6", newPrompt);
                SetNodeSeed(workflow, "5", randomSeed);
                aiNodeId = "5";
            }
            else if (selectedThemeIndex == 5) // Style 6 (ALL)
            {
                SetNodeImage(workflow, "14", uploadedFilename); // Visitor
                SetNodeImage(workflow, "9", uploadedBgName);    // Background
                
                // Mascots mapping:
                // node 1 -> template_2 , node 2 -> template_3 , node 3 -> template_1, node 4 -> template_4 , node 5 -> template_0
                SetNodeImage(workflow, "1", uploadedTemplates[2]);
                SetNodeImage(workflow, "2", uploadedTemplates[3]);
                SetNodeImage(workflow, "3", uploadedTemplates[1]);
                SetNodeImage(workflow, "4", uploadedTemplates[4]);
                SetNodeImage(workflow, "5", uploadedTemplates[0]);

                SetNodePrompt(workflow, "18", newPrompt);
                SetNodeSeed(workflow, "12", randomSeed);
                aiNodeId = "12";
            }
            else // Style 1, 2, 3, 5
            {
                SetNodeImage(workflow, "4", uploadedFilename); // Visitor
                SetNodeImage(workflow, "7", uploadedBgName);    // Background
                SetNodeImage(workflow, "5", uploadedTemplates[selectedThemeIndex]); // Mascot
                SetNodePrompt(workflow, "3", newPrompt);
                SetNodeSeed(workflow, "8", randomSeed);
                aiNodeId = "8";
            }
        }
        else // Group
        {
            if (selectedThemeIndex == 3) // Style 4 (REOG)
            {
                SetNodeImage(workflow, "4", uploadedFilename); // Visitor
                SetNodeImage(workflow, "5", uploadedBgName);    // Background
                SetNodeImage(workflow, "6", uploadedTemplates[3]); // Mascot (template_3)
                SetNodePrompt(workflow, "7", newPrompt);
                SetNodeSeed(workflow, "3", randomSeed);
                aiNodeId = "3";
            }
            else if (selectedThemeIndex == 5) // Style 6 (ALL)
            {
                SetNodeImage(workflow, "4", uploadedFilename); // Visitor
                SetNodeImage(workflow, "5", uploadedBgName);    // Background
                
                // Mascots mapping:
                // node 10 -> template_2 , node 8 -> template_3 , node 11 -> template_1, node 6 -> template_4 , node 9 -> template_0
                SetNodeImage(workflow, "10", uploadedTemplates[2]);
                SetNodeImage(workflow, "8", uploadedTemplates[3]);
                SetNodeImage(workflow, "11", uploadedTemplates[1]);
                SetNodeImage(workflow, "6", uploadedTemplates[4]);
                SetNodeImage(workflow, "9", uploadedTemplates[0]);

                SetNodePrompt(workflow, "7", newPrompt);
                SetNodeSeed(workflow, "20", randomSeed);
                aiNodeId = "20";
            }
            else // Style 1, 2, 3, 5
            {
                SetNodeImage(workflow, "7", uploadedFilename); // Visitor
                SetNodeImage(workflow, "3", uploadedBgName);    // Background
                SetNodeImage(workflow, "4", uploadedTemplates[selectedThemeIndex]); // Mascot
                SetNodePrompt(workflow, "8", newPrompt);
                SetNodeSeed(workflow, "5", randomSeed);
                aiNodeId = "5";
            }
        }

        // Tampilkan log AI engine & model yang digunakan
        LogAiEngineAndModel(workflow, aiNodeId);

        // ============================================================
        // 📦 Payload ComfyUI
        // ============================================================
        var payload = new Dictionary<string, object> 
        { 
            { "prompt", workflow }, 
            { "client_id", System.Guid.NewGuid().ToString() }, 
            { "extra_data", new Dictionary<string, object> 
                { 
                    { "api_key_comfy_org", "comfyui-3cd6df1ed8fceecee1a6a689eadb4cab70484ee88a09f8bbf7af93ccac6f64cf" }, 
                    { "disable_cache", true } 
                } 
            } 
        };

        string payloadJson = MiniJSON.Json.Serialize(payload);
        Debug.Log("📦 Payload ke ComfyUI:\n" + payloadJson);

        using (UnityWebRequest req = new UnityWebRequest("http://127.0.0.1:8188/prompt", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(payloadJson));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Error sending workflow: " + req.error);
                Debug.LogError(req.downloadHandler.text);
                yield break;
            }

            string promptId = ExtractPromptId(req.downloadHandler.text);
            Debug.Log("📥 Response: " + req.downloadHandler.text);

            if (!string.IsNullOrEmpty(promptId))
            {
                yield return WaitForComfyResult(promptId);
            }
        }
    }

    private string ExtractPromptId(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText))
            return null;

        // Cari "prompt_id"
        int keyIndex = jsonText.IndexOf("\"prompt_id\"", StringComparison.OrdinalIgnoreCase);
        if (keyIndex == -1)
            return null;

        // Cari tanda kutip pertama setelah tanda titik dua
        int colonIndex = jsonText.IndexOf(':', keyIndex);
        if (colonIndex == -1)
            return null;

        int startQuote = jsonText.IndexOf('"', colonIndex + 1);
        if (startQuote == -1)
            return null;

        int endQuote = jsonText.IndexOf('"', startQuote + 1);
        if (endQuote == -1)
            return null;

        string promptId = jsonText.Substring(startQuote + 1, endQuote - startQuote - 1);
        return string.IsNullOrEmpty(promptId) ? null : promptId;
    }

    private IEnumerator WaitForComfyResult(string promptId)
    {
        Debug.Log($"🔍 Menunggu hasil untuk prompt: {promptId}");
        string historyUrl = $"http://127.0.0.1:8188/history/{promptId}";
        float timeout = 180f;   // sama seperti Python: 180 detik
        float interval = 1f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(historyUrl))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    string json = req.downloadHandler.text;

                    // kalau prompt_id sudah ada di dalam JSON, berarti hasilnya sudah keluar
                    if (json.Contains(promptId) && json.Contains("\"outputs\""))
                    {
                        Debug.Log("✅ Hasil ditemukan di /history!");
                        yield return DownloadComfyImage(promptId);
                        yield break;
                    }
                }
                else if (req.responseCode != 404)
                {
                    Debug.LogWarning($"⚠️ Request gagal: {req.error}");
                }

                Debug.Log($"[WAIT] Belum ada hasil ({elapsed:F0}s)");
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        Debug.LogError("❌ Timeout: Tidak ada hasil di /history.");
    }

    private Texture2D ApplyOverlay(Texture2D baseTexture, Texture2D overlayTexture)
    {
        // Resize overlay secara GPU jika beda ukuran
        if (overlayTexture.width != baseTexture.width || overlayTexture.height != baseTexture.height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(baseTexture.width, baseTexture.height);
            Graphics.Blit(overlayTexture, rt);

            Texture2D resized = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            resized.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            resized.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            overlayTexture = resized;
        }

        Texture2D result = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false);

        Color[] basePixels = baseTexture.GetPixels();
        Color[] overlayPixels = overlayTexture.GetPixels();

        for (int i = 0; i < basePixels.Length; i++)
        {
            Color overlayColor = overlayPixels[i];
            // Blending sederhana dengan alpha compositing
            basePixels[i] = Color.Lerp(basePixels[i], overlayColor, overlayColor.a);
        }

        result.SetPixels(basePixels);
        result.Apply();
        return result;
    }

    private IEnumerator DownloadComfyImage(string promptId)
    {
        string url = $"http://127.0.0.1:8188/history/{promptId}";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Gagal ambil history: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            Debug.Log("📜 History JSON: " + json);

            // Parsing history secara aman menggunakan MiniJSON
            string filename = null;
            string subfolder = "";
            string type = "output";

            var dict = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            if (dict != null && dict.ContainsKey(promptId))
            {
                var promptData = dict[promptId] as Dictionary<string, object>;
                if (promptData != null && promptData.ContainsKey("outputs"))
                {
                    var outputs = promptData["outputs"] as Dictionary<string, object>;
                    if (outputs != null)
                    {
                        foreach (var kvp in outputs)
                        {
                            var nodeOutput = kvp.Value as Dictionary<string, object>;
                            if (nodeOutput != null && nodeOutput.ContainsKey("images"))
                            {
                                var imagesList = nodeOutput["images"] as List<object>;
                                if (imagesList != null && imagesList.Count > 0)
                                {
                                    var firstImage = imagesList[0] as Dictionary<string, object>;
                                    if (firstImage != null)
                                    {
                                        string imgType = firstImage.ContainsKey("type") ? firstImage["type"].ToString() : "";
                                        // Abaikan gambar bertipe "temp" (hasil dari PreviewImage Node)
                                        if (imgType == "output") 
                                        {
                                            filename = firstImage.ContainsKey("filename") ? firstImage["filename"].ToString() : "";
                                            subfolder = firstImage.ContainsKey("subfolder") ? firstImage["subfolder"].ToString() : "";
                                            type = imgType;
                                            break; 
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(filename))
            {
                Debug.LogError("❌ Tidak menemukan filename bertipe 'output' di history JSON.");
                yield break;
            }

            string viewUrl = $"http://127.0.0.1:8188/view?filename={filename}&subfolder={subfolder}&type={type}";
            Debug.Log("📥 Downloading from: " + viewUrl);

            using (UnityWebRequest imgReq = UnityWebRequestTexture.GetTexture(viewUrl))
            {
                yield return imgReq.SendWebRequest();

                if (imgReq.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Download gagal: " + imgReq.error);
                    yield break;
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(imgReq);
                byte[] imgBytes = tex.EncodeToPNG();

                Debug.Log("✅ Gambar berhasil diunduh dari ComfyUI!");

                // Gunakan kembali pipeline HandleResponse kamu
                string base64 = System.Convert.ToBase64String(imgBytes);
                HandleResponse("{\"image\":\"" + base64 + "\"}");
            }
        }
    }

    private void HandleResponse(string jsonResponse)
    {
        ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);

        // Decode base64 ke byte array
        byte[] imageBytes = Convert.FromBase64String(responseData.image);

        // Buat Texture2D dari byte array
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        // 🔹 Load overlay dari Resources (misal file: Assets/Resources/overlay.png)
        Texture2D overlay = Resources.Load<Texture2D>("overlay_ponorogo_4x6_new");

        if (overlay != null)
        {
            Debug.Log("✅ Overlay ditemukan, menerapkan overlay...");
            texture = ApplyOverlay(texture, overlay);

            // 🔸 Simpan hasil overlay ke resultimg
            resultimg = texture.EncodeToJPG(95);
        }
        else
        {
            Debug.LogWarning("⚠️ Overlay tidak ditemukan di Resources! Gunakan hasil asli.");
            resultimg = imageBytes; // fallback
        }

        // 🔹 Tampilkan hasil di UI
        responseImage.texture = texture;
        responseImage.material.mainTexture = texture;

        // 🔹 Sesuaikan ukuran RawImage agar tidak gepeng (preserve aspect ratio)
        RectTransform rt = responseImage.GetComponent<RectTransform>();
        float texAspect = (float)texture.width / texture.height;
        float frameWidth = rt.rect.width > 0 ? rt.rect.width : 1730f;
        float frameHeight = rt.rect.height > 0 ? rt.rect.height : 2308f;
        float frameAspect = frameWidth / frameHeight;

        if (texAspect > frameAspect)
        {
            // Texture lebih lebar dari frame → sesuaikan berdasarkan lebar
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, frameWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, frameWidth / texAspect);
        }
        else
        {
            // Texture lebih tinggi dari frame → sesuaikan berdasarkan tinggi
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, frameHeight);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, frameHeight * texAspect);
        }

        hasil.SetActive(true);
        loadingtimer.FinishLoading();
        
        // Sembunyikan QR secara default agar user bisa melihat gambar tanpa terhalang
        if (qrContainer != null) qrContainer.SetActive(false);

        // 🔹 Upload & Simpan
        UploadQR();
        SaveImageToDisk(resultimg); // gunakan resultimg (sudah termasuk overlay)
    }


    private string ExtractJsonValue(string json, string key)
    {
        int keyIndex = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (keyIndex == -1) return null;

        int colonIndex = json.IndexOf(':', keyIndex);
        int quoteStart = json.IndexOf('"', colonIndex + 1);
        int quoteEnd = json.IndexOf('"', quoteStart + 1);

        if (quoteStart == -1 || quoteEnd == -1)
            return null;

        return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
    }
    private void SaveImageToDisk(byte[] imageBytes)
    {
        // Tentukan path penyimpanan absolut
        string absoluteSavePath = Path.Combine(Application.dataPath, "Generated/image_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");

        // Simpan file PNG
        File.WriteAllBytes(absoluteSavePath, imageBytes);

        Debug.Log("Image saved to: " + absoluteSavePath);
    }

    public void UploadQR()
    {
        Drive.UploadBase64Image(resultimg);
        // PrintFile(resultimg);
    }

    private string ExtractImageFilename(string json)
    {
        int start = json.IndexOf("ComfyUI");
        if (start == -1) return null;
        int end = json.IndexOf(".png", start);
        return end == -1 ? null : json.Substring(start, end - start + 4);
    }

    [Serializable]
    public class VideoPayload
    {
        public string image;
        public string device;
        public string timestamp;
        public string theme;
    }

    public void OnGenerateVideoPressed()
    {
        // 1. Matikan tombol agar user tidak spam klik
        if (generateButton != null) generateButton.interactable = false;
        // 2. Tampilkan Alert Gambar (Toast)
        StartCoroutine(ShowToast(alertDuration));

        StartCoroutine(SendImageToVideoPlatform());
    }

    private IEnumerator ShowToast(float delay)
    {
        // Munculkan Gambar Alert
        if (alertPopup != null) alertPopup.SetActive(true);
        // Tunggu sekian detik
        yield return new WaitForSeconds(delay);
        // Hilangkan Gambar Alert
        if (alertPopup != null) alertPopup.SetActive(false);
    }


    private IEnumerator SendImageToVideoPlatform()
    {
        if (resultimg == null || resultimg.Length == 0)
        {
            Debug.LogError("❌ resultimg kosong! Pastikan gambar sudah digenerate.");

            if (generateButton != null) generateButton.interactable = true;

            yield break;
        }

        Debug.Log("📤 Mengirim gambar ke server video...");

        string base64Img = Convert.ToBase64String(resultimg);

        // Buat payload JSON
        VideoPayload payload = new VideoPayload
        {
            image = base64Img,
            device = SystemInfo.deviceName,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            theme = selectedTheme
        };

        string jsonBody = JsonUtility.ToJson(payload);

        Debug.Log($"✅ Mengirim gambar ke server video. Tema yang dipilih: {selectedTheme}");

        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        //  UnityWebRequest req = new UnityWebRequest("http://localhost:3000/api/generate-video", "POST");
        UnityWebRequest req = new UnityWebRequest("https://192.192.168.102:3000/api/generate-video", "POST");
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.certificateHandler = new BypassCertificate();

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Gagal mengirim gambar: " + req.error);
        }
        else
        {
            Debug.Log("✅ Berhasil mengirim ke server video!");
            Debug.Log("Response: " + req.downloadHandler.text);
        }
        if (generateButton != null) generateButton.interactable = true;
    }

    // ============================================================
    // 🔘 FUNGSI TOMBOL BARU (UNDUH & CETAK)
    // ============================================================
    public void OnUnduhClicked()
    {
        // Munculkan QR panel (Frame 10)
        if (qrContainer != null) 
        {
            qrContainer.SetActive(true);
            Debug.Log("🔽 Tombol UNDUH ditekan: Menampilkan QR Code.");
        }
        else
        {
            Debug.LogWarning("⚠️ qrContainer belum di-drag ke Inspector!");
        }
    }

    public void OnKembaliClicked()
    {
        // Tutup QR panel, kembali ke tampilan awal (Frame 9)
        if (qrContainer != null) 
        {
            qrContainer.SetActive(false);
            Debug.Log("↩️ Tombol KEMBALI ditekan: Menutup popup QR.");
        }
    }

    public void OnCetakClicked()
    {
        Debug.Log("🖨️ Tombol CETAK ditekan: Menyiapkan file untuk diprint.");
        PrintFile();
    }

    private void PrintFile()
    {
        if (resultimg == null || resultimg.Length == 0)
        {
            Debug.LogError("❌ Tidak ada gambar yang bisa di-print (resultimg kosong)!");
            return;
        }

        try
        {
            // Load gambar asli (9:16) ke dalam Texture2D untuk diubah ke format JPG
            Texture2D originalTex = new Texture2D(2, 2);
            originalTex.LoadImage(resultimg);

            // Tentukan nama file unik agar antrean print tidak saling menimpa
            string fileName = "print_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
            
            // Gabungkan Path dari Inspector dengan nama file
            string absoluteSavePath = Path.Combine(printerHotfolderPath, fileName);

            // Pastikan foldernya ada
            if (!Directory.Exists(printerHotfolderPath))
            {
                Directory.CreateDirectory(printerHotfolderPath);
            }

            // Simpan file JPG murni (tanpa manipulasi) ke direktori tersebut
            byte[] jpgBytes = originalTex.EncodeToJPG(100);
            File.WriteAllBytes(absoluteSavePath, jpgBytes);

            Destroy(originalTex);

            Debug.Log("✅ Gambar sukses dikirim ke antrean printer: " + absoluteSavePath);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Gagal menyimpan gambar untuk print: Pastikan PC tujuan menyala dan folder sudah di-share (Sharing Access). Error detail: " + e.Message);
        }
    }
}






[System.Serializable]
public class ResponseData
{
    public string image;
}
