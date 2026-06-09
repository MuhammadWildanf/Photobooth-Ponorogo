using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GenderSelectionController : MonoBehaviour
{
    public void SelectGender(String gender)
    {
        PlayerPrefs.SetString("SelectedGender", gender);
        SceneManager.LoadScene("SelectPosition");
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
