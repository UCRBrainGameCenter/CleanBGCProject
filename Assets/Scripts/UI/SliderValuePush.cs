using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderValuePush : MonoBehaviour
{
    [SerializeField]
    private Text valueHolder = null;

    private void Awake()
    {
        GetComponent<Slider>().onValueChanged.AddListener(UpdateValue);
    }

    void Start()
    {
        valueHolder.text = GetComponent<Slider>().value.ToString();
    }

    private void UpdateValue(float newValue)
    {
        valueHolder.text = newValue.ToString();
    }
}
