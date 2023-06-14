using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlider : MonoBehaviour
{
    private Slider slider;
    void Awake() 
    {
        slider = GetComponentInChildren<Slider>();
        slider.value = Time.timeScale;
        slider.onValueChanged.AddListener(value => {
            Time.timeScale = value;
        });
    }
}
