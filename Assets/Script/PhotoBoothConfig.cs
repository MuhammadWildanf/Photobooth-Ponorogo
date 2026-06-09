using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Eraoption
{
    public string eraName;
    public Sprite thumbnail;
    public Texture backgroundTexture;
}

[CreateAssetMenu(fileName = "PhotoBoothConfig", menuName = "PhotoBooth/Configuration")]
public class PhotoBoothConfig : ScriptableObject
{
    public Eraoption[] eras;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
