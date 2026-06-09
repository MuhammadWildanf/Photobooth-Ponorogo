using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{

    //[SerializeField] private RawImage _img;
    [SerializeField] private GameObject _patern1, _patern2, patern3;
    public CanvasGroup _touch_here;
    // [SerializeField] private float _x, _y;
    // Start is called before the first frame update
    void Start()
    {
        LeanTween.rotateAround(_patern1, Vector3.forward, -360, 30f).setLoopClamp();
        LeanTween.rotateAround(_patern2, Vector3.forward, -360, 50f).setLoopClamp();
        LeanTween.rotateAround(patern3, Vector3.forward, -360, 70f).setLoopClamp();
    }

    // Update is called once per frame

}
