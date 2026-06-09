using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;

public class DriveQR : MonoBehaviour
{
    public string textToEncode = "Hello, World!";
    public int qrCodeWidth = 256;
    public int qrCodeHeight = 256;
    public RawImage qrCodeImage;
    public GameObject spinner;
    public GameObject unduh;
    // public LoadingManager loading;
    public LoadingTimer loadingtimer;
    public GameObject qr;
    public GameObject btnRetake;

    public void UploadBase64Image(byte[] image)
    {
        string fileName = GenerateFileName();
        string folderId = "1YDgHbFE1KYzhDQThqm0qeIVlBN-wvr0J";
        StartCoroutine(UploadFile(image, fileName, folderId));
    }

    private string GenerateFileName()
    {
        // Generate a timestamp-based filename
        return "image_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
    }

    private IEnumerator UploadFile(byte[] imageData, string fileName, string folderId)
    {
        // Create texture from byte array
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        var content = texture.EncodeToPNG();

        // Create file metadata and content
        var file = new UnityGoogleDrive.Data.File()
        {
            Name = fileName,
            Content = content,
            Parents = new List<string> { folderId }
        };

        var request = GoogleDriveFiles.Create(file);
        request.Fields = new List<string> { "id" };
        yield return request.Send();

        if (request.IsError)
        {
            Debug.LogError("Error uploading file: " + request.Error);
        }
        else
        {
            string fileId = request.ResponseData.Id;
            string fileUrl = "https://drive.google.com/file/d/" + fileId + "/view?usp=sharing";
            StartCoroutine(GenerateQRCode(fileUrl, qrCodeWidth, qrCodeHeight));
            Debug.Log("File uploaded successfully. File ID: " + fileId);
            Debug.Log("🔗 URL: " + fileUrl);
           /*  StartCoroutine(SendPhotoUrlToServer(fileUrl)); */
        }
    }

    private IEnumerator SendPhotoUrlToServer(string photoUrl)
    {
        // ================================
        // TUNGGU SAMPAI SESSION USER ADA
        // ================================

        int timeout = 0;
        while (WebSocketManager.ActiveUser == null)
        {
            if (timeout > 50) // 50 × 0.1 detik = 5 detik
            {
                Debug.LogError("❗ Timeout: Session user tidak ditemukan (ActiveUser masih null).");
                yield break;
            }

            Debug.Log("⏳ Menunggu ActiveUser...");
            timeout++;
            yield return new WaitForSeconds(0.1f);
        }

        // ================================
        // ACTIVE USER TERDETEKSI
        // ================================
        string userId = WebSocketManager.ActiveUser;
        Debug.Log("✅ ActiveUser terdeteksi: " + userId);

        // string apiUrl = "http://localhost:3002/photobooth/upload";
        string apiUrl = "https://scmdigitalday2025.com/photobooth/upload";

        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("photo_url", photoUrl);

        using (UnityWebRequest req = UnityWebRequest.Post(apiUrl, form))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Gagal kirim URL ke server: " + req.error);
            }
            else
            {
                Debug.Log("✅ URL foto berhasil disimpan server untuk user: " + userId);
            }
        }
    }

    private IEnumerator GenerateQRCode(string text, int width, int height)
    {
        string url = $"https://api.qrserver.com/v1/create-qr-code/?data={UnityWebRequest.EscapeURL(text)}&size={width}x{height}";

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load QR code: " + www.error);
            }
            else
            {
                Texture2D qrCodeTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                // loading.FinishLoadingScreen(qr, "print");
                loadingtimer.StopTimer(qr, "print");
                qrCodeImage.texture = qrCodeTexture;
                spinner.SetActive(false);
                unduh.SetActive(true);
                //btnRetake.active = true;

            }
        }
    }

    public void backtohome()
    {
        StopAllCoroutines();
    }
    public void UpdateQRCode(string newText)
    {
        textToEncode = newText;
        StartCoroutine(GenerateQRCode(textToEncode, qrCodeWidth, qrCodeHeight));
    }
}
