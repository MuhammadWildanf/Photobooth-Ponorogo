using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToogleButton : MonoBehaviour
{
    private bool donlot = false;

    public GameObject download;



    public void click()
    {
        if (donlot == false)
        {
            Debug.Log(donlot);
            download.SetActive(true);
            donlot = true;

        }
        else
        {
            Debug.Log(donlot);
            download.SetActive(false);

            donlot = false;
        }
    }
    public void donlotFalse()
    {
        donlot = false;

    }

}
