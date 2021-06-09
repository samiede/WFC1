using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GlobalVars _vars;
    [SerializeField] private WavefunctionCollapse _wfc;
    [SerializeField] private Slider animationSpeedSlider;
    
    void Start()
    {
        _wfc = FindObjectOfType<WavefunctionCollapse>();
        InitSpeed(0.5f);
    }

    public void UpdateSpeed(float speed)
    {
        _vars.animationDelay = speed;
    }

    public void StartGeneration()
    {
        _wfc.StartCollapse();
    }

    public void ResetMap()
    {
        _wfc.SetupMap();
    }

    private void InitSpeed(float value)
    {
        animationSpeedSlider.value = value;
    }
}
