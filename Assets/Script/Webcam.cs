using Michsky.MUIP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting; // MUIP namespace
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
public class Webcam : MonoBehaviour
{
    public CustomDropdown myDropdown; // Your dropdown variable
    public TMP_Dropdown webcamDropdown;
    public Sprite imgCamera;
    public GameObject startButton;
    private List<string> webcamNames = new List<string>();
    public RawImage webcam;
    public RawImage generateImage;
    public RawImage responseImage;
    public DriveQR Drive;
    public LoadingManager loading;
    public LoadingTimer loadingtimer;
    public GameObject hasil;
    public GameObject photo;
    public GameObject select;
    public GameObject blitz;
    public GameObject text;
    public GameObject Male;
    public GameObject Female;
    //public GameObject Hijab;
    public TextMeshProUGUI countdown;
    public int count;
    public GameManager gameManager;
    private IEnumerator currentRoutine = null;
    private WebCamTexture webCamTexture;
    private byte[] resultimg;
    private string targetimage;
    private string imgcapture;
    private string sourceimage;
    private int[] face;
    private string genderselected;
    private static Texture2D capturedImage;

    void Start()
    {
        WebCamDevice[] device = WebCamTexture.devices;
        // for (int i = 0; i < device.Length; i++)
        // {
        //     print("Webcam available: " + device[i].name);
        // }

        if (device.Length > 0)
        {
            webcamNames.Add("Pilih Webcam");
            foreach (var webcam in device)
            {
                webcamNames.Add(webcam.name); // Menyimpan nama perangkat
            }

            // Membersihkan opsi dropdown yang ada dan menambahkan daftar nama webcam
            webcamDropdown.ClearOptions();
            webcamDropdown.AddOptions(webcamNames);

            startButton.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Tidak ada perangkat webcam yang ditemukan.");
            webcamNames.Add("No Webcam Detected");
            webcamDropdown.ClearOptions();
            webcamDropdown.AddOptions(webcamNames);

            startButton.SetActive(false);
        }

        webcamDropdown.onValueChanged.AddListener(delegate { StartSelectedWebcam(); });
        //  WebCamDevice[] device = WebCamTexture.devices;
        //  /* webCamTexture = new WebCamTexture(device[0].name, 1280, 720, 30);
        //   webcam.texture = webCamTexture;
        //   webCamTexture.Play();*/

        //  //for (int i = 0; i < device.Length; i++)
        //  //{
        //  //    print("Webcam available: " + device[i].name);
        //  //}

        //  if (device.Length > 0)
        //  {
        //      webcamNames.Add("SELECT CAMERA");
        //      foreach (var webcam in device)
        //      {
        //          webcamNames.Add(webcam.name); // Menyimpan nama perangkat
        //      }

        //      // Membersihkan opsi dropdown yang ada dan menambahkan daftar nama webcam

        //      //webcamDropdown.AddOptions(webcamNames);
        //      //myDropdown.AddComponent(webcamNames);

        //      // seting dropdown UI
        //      myDropdown.CreateNewItem("SELECT CAMERA", true);

        //      for (int i = 1; i < device.Length; ++i)
        //      {
        //          // Use false when using this in a loop
        //          myDropdown.CreateNewItem(webcamNames[i], imgCamera, false);
        //      }

        //      // Initialize the new items
        //      myDropdown.SetupDropdown();

        //      // Add int32 (dynamic) events
        //      myDropdown.onValueChanged.AddListener(TestFunction);

        //      myDropdown.ChangeDropdownInfo(0); // Changing index & updating UI
        //                                        // myDropdown.RemoveItem("Pilih Webcam"); // Delete a specific item
        //                                        // myDropdown.Animate(); // Animate dropdown
        //      startButton.SetActive(false);
        //      //startButton.interactable = false;
        //  }
        //  else
        //  {
        //      Debug.LogWarning("Tidak ada perangkat webcam yang ditemukan.");
        //      webcamNames.Add("No Webcam Detected");
        //      //webcamDropdown.ClearOptions();
        //      //webcamDropdown.AddOptions(webcamNames);

        //      //startButton.Interactable = false;
        //      startButton.SetActive(false);
        //  }

        ////  myDropdown.onValueChanged.AddListener(delegate { StartSelectedWebcam2(myDropdown.ChangeDropdownInfo()); });
        // // webcamDropdown.onValueChanged.AddListener(delegate { StartSelectedWebcam(); });
    }

    void TestFunction(int value)
    {
        string selectedItem = webcamNames[value];

        if (selectedItem == "Select Camera")
        {
            // Kembalikan ke posisi "Select Camera" dan jangan jalankan aksi lain
            myDropdown.ChangeDropdownInfo(0);
            startButton.SetActive (false);
            return;
        }

        // Lanjutkan logika normal jika bukan "Select Camera"
        startButton.SetActive(true);
        Debug.Log("Selected camera: " + selectedItem);

        //Debug.Log("Changed index to: " + value.ToString());
        StartSelectedWebcam2(value);
    }

    public void StartSelectedWebcam2(int value)
    {
        startButton.SetActive( true);
       
        myDropdown.gameObject.SetActive(false);

        // Hentikan WebCamTexture sebelumnya jika ada
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }

        // Dapatkan nama webcam berdasarkan pilihan di dropdown
        string selectedDeviceName = webcamNames[value];

        // Inisialisasi dan mulai WebCamTexture dengan perangkat yang dipilih
        webCamTexture = new WebCamTexture(selectedDeviceName, 1280, 720, 30);
        webcam.texture = webCamTexture;
        webCamTexture.Play();
        gameManager.ChangeState("HOME");
    }
    public void StartSelectedWebcam()
    {
        startButton.SetActive (webcamDropdown.value >= 0);
        webcamDropdown.gameObject.SetActive(false);

        // Hentikan WebCamTexture sebelumnya jika ada
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }

        // Dapatkan nama webcam berdasarkan pilihan di dropdown
        string selectedDeviceName = webcamNames[webcamDropdown.value];

        // Inisialisasi dan mulai WebCamTexture dengan perangkat yang dipilih
        webCamTexture = new WebCamTexture(selectedDeviceName, 1280, 720, 30);
        webcam.texture = webCamTexture;
        webCamTexture.Play();
        gameManager.ChangeState("Home");
    }

    public bool IsInProgress()
    {
        return currentRoutine != null;
    }

    public void SelectGender(string gender)
    {
        genderselected = gender;
        if (gender == "Male")
        {
            Male.SetActive(true);
            Female.SetActive(false);
        }
        else if (gender == "Female")
        {
            Male.SetActive(false);
            Female.SetActive(true);
        }
    }

    public void SelectEra(string Era)
    {
        targetimage = ConvertTargetImageToBase64(Era);
    }
    public string ConvertTargetImageToBase64(string Era)
    {
        int img = 0;
        if (genderselected == "Male")
        {
            if (Era == "Past")
            {
                int randomNumber = Random.Range(1, 5);
                img = randomNumber;
                face = (img == 4) ? new int[] { 1 } : new int[] { 0 };
            }
            else if (Era == "Present")
            {
                int randomNumber = Random.Range(1, 5);
                img = randomNumber;
                face = new int[] { 0 };
            }
            else if (Era == "Future")
            {
                int randomNumber = Random.Range(1, 5);
                img = randomNumber;
                face = (img == 2) ? new int[] { 1 } : new int[] { 0 };
            }
        }
        else if (genderselected == "Female")
        {
            if (Era == "Past")
            {
                int randomNumber = Random.Range(1, 5);
                img = randomNumber;
                face = new int[] { 0 };
                //face = new int[] { 1, 0, 0, 0, 0 };
                face = (img == 1 || img == 2) ? new int[] { 1 } : new int[] { 0 };
            }
            else if (Era == "Present")
            {
                int randomNumber = Random.Range(1, 5);
                img = randomNumber;
                face = new int[] { 0 };
            }
            else if (Era == "Future")
            {
                int randomNumber = Random.Range(1, 5);
                img = randomNumber;
                //face = new int[] { 0 };
                //face = new int[] { 0, 1, 0, 0, 0 };
                face = (img == 2) ? new int[] { 1 } : new int[] { 0 };
            }
        }
        string absolutePath = Path.Combine(Application.streamingAssetsPath, genderselected + "/" + Era + "/" + img.ToString() + ".png");

        Debug.Log(absolutePath);

        byte[] imageBytes = File.ReadAllBytes(absolutePath);

        return Convert.ToBase64String(imageBytes);
    }

    public void CaptureFrame()
    {
        // Buat Texture2D untuk menyimpan frame
        Texture2D frame = new Texture2D(webCamTexture.width, webCamTexture.height);
        frame.SetPixels(webCamTexture.GetPixels());
        frame.Apply();

        // Encode texture ke PNG
        byte[] bytes = frame.EncodeToPNG();

        string PhotoSavePath = Path.Combine(Application.dataPath, "Photo/image_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");

        // Simpan file PNG
       //File.WriteAllBytes(PhotoSavePath, bytes);


        // Load PNGRaw Image GENERATING
        //imgcapture = PhotoSavePath;
        //Invoke("LoadGenerateTexture", 10f);


        // Simpan tangkapan kamera ke Texture2D UNTUNK GENERATED IMAGE
        capturedImage = new Texture2D(webCamTexture.width, webCamTexture.height);
        capturedImage.SetPixels(webCamTexture.GetPixels());
        capturedImage.Apply();

        



        byte[] cropped = CropTexture(frame, 500, 0, 280, 720).EncodeToPNG();

        /*string absoluteSavePath = Path.Combine(Application.dataPath, "cropped/image_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
        File.WriteAllBytes(absoluteSavePath, cropped);*/

        sourceimage = Convert.ToBase64String(cropped);
        Invoke("SendFrame", 1f);
        // gameManager.ChangeState("select");
        countdown.text = "";
        LoadGenerateTexture();
    }

    public void LoadGenerateTexture()
    {
        if (generateImage != null && capturedImage != null)
        {
            generateImage.texture = capturedImage;
        }
        Debug.Log("load image ke generated");
    }


    public void CountdownAndMakePhoto()
    {
        if (currentRoutine != null)
            return;

        currentRoutine = Countdown();
        StartCoroutine(currentRoutine);
    }

    public IEnumerator Countdown()
    {
        if (countdown != null && count > 0)
        {
            for (int i = count; i > 0; i--)
            {
                countdown.text = i.ToString();

                LeanTween.scale(text, new Vector3(5f, 5f, 5f), 0.5f).setEaseOutQuad();
                yield return new WaitForSeconds(1f);
                LeanTween.scale(text, new Vector3(0f, 0f, 0f), 0f);
            }
        }

        StartCoroutine("Blitz");
        yield return null;
        currentRoutine = null;
    }

    IEnumerator Blitz()
    {
        blitz.SetActive(true);
        LeanTween.alphaCanvas(blitz.GetComponent<CanvasGroup>(), 1f, 0.1f);
        yield return new WaitForSeconds(0.5f);
        LeanTween.alphaCanvas(blitz.GetComponent<CanvasGroup>(), 0f, 0.1f);
        yield return new WaitForSeconds(0.2f);
        blitz.SetActive(false);

        CaptureFrame();
        photo.SetActive(false);
        gameManager.ChangeState("Loading");
        loadingtimer.StartLoading();
    }

    public Texture2D CropTexture(Texture2D originalTexture, int x, int y, int width, int height)
    {
        // Buat texture baru dengan ukuran sesuai cropping
        Texture2D croppedTexture = new Texture2D(width, height);

        // Salin pixel dari area cropping
        Color[] pixels = originalTexture.GetPixels(x, y, width, height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

    public void SendFrame()
    {
        string base64TargetImage = targetimage;
        string base64Image = sourceimage;

        // Siapkan JSON request body
        string jsonBody = JsonUtility.ToJson(new RequestBody
        {
            source_image = base64Image,
            target_image = base64TargetImage,
            source_faces_index = new int[] { 0 },
            face_index = face,
            upscaler = "None",
            scale = 1,
            upscale_visibility = 1,
            face_restorer = "CodeFormer",
            restorer_visibility = 1,
            codeformer_weight = 1f,
            restore_first = 1,
            model = "inswapper_128.onnx",
            gender_source = 0,
            gender_target = 0,
            save_to_file = 0,
            result_file_path = "",
            device = "CUDA",
            mask_face = 1,
            select_source = 0,
            face_model = "None",
            source_folder = "",
            random_image = 0,
            upscale_force = 0,
            det_thresh = 0.1f,
            det_maxnum = 0
        });

        // Kirim web request
        StartCoroutine(SendWebRequest(jsonBody));
    }

    private IEnumerator SendWebRequest(string jsonBody)
    {
        UnityWebRequest request = new("http://127.0.0.1:7860/reactor/image", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request sent successfully: " + request.downloadHandler.text);
            // Tangani respons base64 dan tampilkan gambarnya
            HandleResponse(request.downloadHandler.text);
            // loading.FinishLoadingScreen(hasil, "preview");
            // loadingtimer.StopTimer(hasil, "preview");
        }
        else
        {
            Debug.LogError("Error sending request: " + request.error);
        }
    }

    private void HandleResponse(string jsonResponse)
    {
        ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);
        // Decode base64 ke byte array
        byte[] imageBytes = System.Convert.FromBase64String(responseData.image);
        resultimg = imageBytes;

        // Buat Texture2D dari byte array
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        // Tampilkan gambar di RawImage
        responseImage.texture = texture;
        responseImage.material.mainTexture = texture;
        hasil.active = true;
        loadingtimer.loadingscreen.SetActive(false);
        UploadQR();

        SaveImageToDisk(imageBytes);
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

    public void PrintFile()
    {
        // Tentukan path penyimpanan absolut
        string absoluteSavePath = "C:/hotfolder/print/print.png";

        // Simpan file PNG
        File.WriteAllBytes(absoluteSavePath, resultimg);

        Debug.Log("Image saved to: " + absoluteSavePath);
    }

    [System.Serializable]
    public class RequestBody
    {
        public string source_image;
        public string target_image;
        public int[] source_faces_index;
        public int[] face_index;
        public string upscaler;
        public int scale;
        public int upscale_visibility;
        public string face_restorer;
        public int restorer_visibility;
        public float codeformer_weight;
        public int restore_first;
        public string model;
        public int gender_source;
        public int gender_target;
        public int save_to_file;
        public string result_file_path;
        public string device;
        public int mask_face;
        public int select_source;
        public string face_model;
        public string source_folder;
        public int random_image;
        public int upscale_force;
        public float det_thresh;
        public int det_maxnum;
    }

    public class ResponseData
    {
        public string image; // Sesuaikan dengan nama field dalam respons server Anda
    }
}
