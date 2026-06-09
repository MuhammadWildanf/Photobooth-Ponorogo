using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventResult : MonoBehaviour
{
    public GameObject spinner;
    public void ShowSpinner() { 
        spinner.SetActive(true);
    }
    public void HideSpinner() { 
        spinner.SetActive(false);
    }
}
