using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Build;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string state;
    public GameObject back;
    public GameObject Home;
    public GameObject SelectGender;
    public GameObject SelectEra;
    public GameObject SelectMode; // Halaman pemilihan Single/Group
    public GameObject Loading;
    public GameObject Photo;
    public GameObject Result;

    //public GameObject btnRetake;

    void Start()
    {
        // state = "Home";
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Back()
    {
        /*  if (state == "SelectGender")
         {
             ChangeState("Home");
             Home.SetActive(true);
             SelectGender.SetActive(false);
         }
         else if (state == "SelectEra")
         {
             ChangeState("SelectGender");
             SelectEra.SetActive(false);
             SelectGender.SetActive(true);
         } */

        if (state == "SelectEra")
        {
            ChangeState("Home");
            SelectEra.SetActive(false);
            Home.SetActive(true);
        }
        else if (state == "SelectMode")
        {
            ChangeState("SelectEra");
            if (SelectMode != null) SelectMode.SetActive(false);
            SelectEra.SetActive(true);
        }
        else if (state == "Photo")
        {
            ChangeState("SelectMode");
            Photo.SetActive(false);
            if (SelectMode != null) SelectMode.SetActive(true);
        }
    }

    public void ChangeState(string states)
    {
        state = states;
    }

    public void Reset()
    {
        ChangeState("Home");

        Home.SetActive(true);
        SelectGender.SetActive(false);
        SelectEra.SetActive(false);
        if (SelectMode != null) SelectMode.SetActive(false);
        Photo.SetActive(false);
        Loading.SetActive(false);
        Result.SetActive(false);
    }

     public void Close()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
