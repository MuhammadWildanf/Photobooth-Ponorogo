using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;
using SimpleJSON;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ComfyUIManager : MonoBehaviour
{
    [Header("ComfyUI Settings")]
    public string server = "http://127.0.0.1:8188";
    public string workflowPath = "Assets/gemini.json";
    public string imagePath = "Assets/jerome.jpg";
    public string outputSavePath = "D:/Stable Diffusion/ComfyAPITests/";

    [TextArea(5, 15)]
    public string newPrompt =
        "A 16-year-old Indonesian boy, with the exact same face as in the reference image...";

    [Header("UI Output")]
    public UnityEngine.UI.RawImage outputImage;

    private string uploadedImageName;
    private string promptId;
    private JSONNode workflow;

    public void StartProcess()
    {
        StartCoroutine(RunWorkflow());
    }

    private IEnumerator RunWorkflow()
    {
        // STEP 1: Upload image
        yield return UploadImage();

        // STEP 2: Load and modify workflow
        LoadAndModifyWorkflow();

        // STEP 3: Send workflow
        yield return SendWorkflow();

        // STEP 4: Wait for result
        yield return WaitForResult();
    }

    // ===============================
    // UPLOAD IMAGE
    // ===============================
    private IEnumerator UploadImage()
    {
        Debug.Log("[STEP 1] Upload image...");
        byte[] imgData = File.ReadAllBytes(imagePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imgData, Path.GetFileName(imagePath), "image/jpeg");

        using (UnityWebRequest www = UnityWebRequest.Post($"{server}/upload/image", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Upload failed: " + www.error);
            else
            {
                var res = JSON.Parse(www.downloadHandler.text);
                uploadedImageName = res["name"];
                Debug.Log($"[UPLOAD] Image uploaded as: {uploadedImageName}");
            }
        }
    }

    // ===============================
    // LOAD + MODIFY WORKFLOW
    // ===============================
    private void LoadAndModifyWorkflow()
    {
        Debug.Log("[STEP 2] Modify workflow...");
        string jsonText = File.ReadAllText(workflowPath);
        workflow = JSON.Parse(jsonText);

        // Node 2 = LoadImage
        if (workflow["2"] != null && workflow["2"]["inputs"] != null)
        {
            workflow["2"]["inputs"]["image"] = uploadedImageName;
            Debug.Log($" ↳ Node 2 (LoadImage) updated with image: {uploadedImageName}");
        }

        // Node 5 = GeminiImageNode
        if (workflow["5"] != null && workflow["5"]["inputs"] != null)
        {
            workflow["5"]["inputs"]["prompt"] = newPrompt;
            long newSeed = (long)UnityEngine.Random.Range(100000000000000, 999999999999999);
            workflow["5"]["inputs"]["seed"] = newSeed;
            Debug.Log($" ↳ Node 5 (GeminiImageNode) updated with new prompt and seed {newSeed}");
        }

        // Node 30 = SaveImage
        if (workflow["30"] != null && workflow["30"]["inputs"] != null)
        {
            workflow["30"]["inputs"]["images"][0] = "5";
            Debug.Log($" ↳ Node 30 (SaveImage) linked to GeminiImageNode.");
        }
    }

    // ===============================
    // SEND WORKFLOW
    // ===============================
    private IEnumerator SendWorkflow()
    {
        Debug.Log("[STEP 3] Sending workflow to ComfyUI...");

        string clientId = Guid.NewGuid().ToString();
        var payload = new JSONObject();
        payload["prompt"] = workflow;
        payload["client_id"] = clientId;
        payload["extra_data"] = new JSONObject();
        payload["extra_data"]["api_key_comfy_org"] = "comfyui-60e74e7bd047d2b9539209030755b1d09ee3b82eb06a17fa5ce4d3093df48587";
        payload["extra_data"]["disable_cache"] = true;

        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload.ToString());
        UnityWebRequest www = new UnityWebRequest($"{server}/prompt", "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.LogError("Prompt send failed: " + www.error);
        else
        {
            var res = JSON.Parse(www.downloadHandler.text);
            promptId = res["prompt_id"];
            Debug.Log($" ↳ Prompt ID: {promptId}");
        }
    }

    // ===============================
    // WAIT FOR RESULT
    // ===============================
    private IEnumerator WaitForResult()
    {
        Debug.Log("[STEP 4] Waiting for result...");
        float timeout = 180f;
        float elapsed = 0f;
        float interval = 2f;

        while (elapsed < timeout)
        {
            string url = $"{server}/history/{promptId}";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var history = JSON.Parse(www.downloadHandler.text);
                    if (history[promptId] != null && history[promptId]["outputs"] != null)
                    {
                        Debug.Log("✅ Result found in /history!");
                        yield return DownloadOutput(history[promptId]["outputs"]);
                        yield break;
                    }
                }
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
            Debug.Log($"[WAIT] Waiting... {elapsed}s");
        }

        Debug.LogError("❌ Timeout waiting for result.");
    }

    // ===============================
    // DOWNLOAD OUTPUT
    // ===============================
    private IEnumerator DownloadOutput(JSONNode outputs)
    {
        string filename = "";
        string subfolder = "";

        foreach (KeyValuePair<string, JSONNode> kvp in outputs)
        {
            if (kvp.Value["images"] != null)
            {
                filename = kvp.Value["images"][0]["filename"];
                subfolder = kvp.Value["images"][0]["subfolder"];
                break;
            }
        }

        if (string.IsNullOrEmpty(filename))
        {
            Debug.LogError("❌ No SaveImage node found.");
            yield break;
        }

        string url = $"{server}/view?filename={filename}&subfolder={subfolder}&type=output";
        Debug.Log($"[STEP 5] Downloading {filename}...");

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(www);
                Debug.Log("✅ Image downloaded successfully.");

                // Save locally
                string savePath = Path.Combine(outputSavePath, filename);
                File.WriteAllBytes(savePath, tex.EncodeToJPG());
                Debug.Log($"✅ Saved to: {savePath}");

                // Show on UI
                if (outputImage != null)
                    outputImage.texture = tex;
            }
            else
                Debug.LogError("❌ Download failed: " + www.error);
        }
    }
}
