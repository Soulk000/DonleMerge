using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigateButton : MonoBehaviour
{
    public string Url;

    public void Start()
    {
        GetComponent<Button>().onClick.AddListener(Navigate);
    }

    public void Navigate()
    {
        Application.OpenURL(Url);
    }
}