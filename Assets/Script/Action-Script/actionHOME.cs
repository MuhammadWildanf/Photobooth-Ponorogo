using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class actionHOME : MonoBehaviour
{
    public GameObject current;
    public GameObject NextContainer;
    public GameObject PrevContainer;
    // Start is called before the first frame update
   public void HideCurrent()
    {
        current.SetActive(false);
        //NextContainer.SetActive(true);
    }

    public void ShowNext()
    {
        NextContainer.SetActive(true);
}
    public void ShowPrev() { 
        PrevContainer.SetActive(true);
    }
}
