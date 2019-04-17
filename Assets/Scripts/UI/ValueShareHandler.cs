using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ValueShareHandler : MonoBehaviour
{
    [System.Serializable]
    struct SliderData
    {
        [SerializeField]
        public Slider slider;
        [HideInInspector]
        public float lastValue;

        public SliderData(Slider slider, float lastValue)
        {
            this.slider = slider;
            this.lastValue = lastValue;
        }
    }

    [System.Serializable]
    struct InputFieldData
    {
        [SerializeField]
        public InputField inputField;
        [HideInInspector]
        public string lastValue;

        public InputFieldData(InputField inputField, string lastValue)
        {
            this.inputField = inputField;
            this.lastValue = lastValue;
        }
    }

    [SerializeField]
    SliderData[] sliders = null;
    [SerializeField]
    InputFieldData[] inputFields = null;


    [SerializeField]
    protected Button[] incrementButtons = null;
    [SerializeField]
    protected Button[] decrementButtons = null;


    public delegate void ValueUpdated (float value);

    public ValueUpdated valueUpdated;

    float minValue = 0.0f;
    float maxValue = 100.0f;

    void Start()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            InputField inputField = inputFields[i].inputField;
            //Done so that the value of i at this iteration of the loop is passed into the delegate,
            //instead of the value of i at the end of the scope.
            int indexCopy = i;
            inputField.onEndEdit.AddListener(delegate (string val) { OnInputFieldChanged(val, indexCopy); });
            inputFields[i].lastValue = inputField.text;
        }

        for (int i = 0; i < sliders.Length; i++)
        {
            Slider slider = sliders[i].slider;
            //Done so that the value of i at this iteration of the loop is passed into the delegate,
            //instead of the value of i at the end of the scope.
            int indexCopy = i;
            slider.onValueChanged.AddListener(delegate(float val) { OnSliderChanged(val, indexCopy); });
            sliders[i].lastValue = slider.value;
        }

        for (int i = 0; i < incrementButtons.Length; i++)
        {
            Button incButton = incrementButtons[i];
            incButton.onClick.AddListener(Increment);
        }

        for (int i = 0; i < decrementButtons.Length; i++)
        {
            Button decButton = decrementButtons[i];
            decButton.onClick.AddListener(Decrement);
        }
    }

    /// <summary>
    /// Updates the values in all of the Sliders and InputFields that this handler is in charge of.
    /// </summary>
    /// <param name="value">The new value for all of the inputs to be.</param>
    private void UpdateAllValues(float value)
    {
        value = Mathf.Clamp(value, minValue, maxValue);
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].slider.value = value;
            sliders[i].lastValue = value;
        }

        string strVal = value.ToString();

        for(int i = 0; i < inputFields.Length; i++)
        {
            inputFields[i].inputField.text = strVal;
            inputFields[i].lastValue = strVal;
        }

        if ( valueUpdated != null )
        {
            valueUpdated(value);
        }

        for (int i = 0; i < decrementButtons.Length; i++)
        {
            decrementButtons[i].interactable = (value != minValue);
        }

        for (int i = 0; i < incrementButtons.Length; i++)
        {
            incrementButtons[i].interactable = (value != maxValue);
        }
    }

    public void OnInputFieldChanged(string str, int index)
    {
        if (inputFields[index].lastValue != str)
        {
            float value;
            if(float.TryParse(str, out value))
            {
                UpdateAllValues(value);
            }
            else
            {
                inputFields[index].inputField.text = inputFields[index].lastValue;
            }
        }
    }

    public void Increment()
    {
        AdjustBy(1f);
    }

    public void Decrement()
    {
        AdjustBy(-1f);
    }

    private void AdjustBy(float adjustment)
    {
        float newValue = Mathf.Clamp(sliders[0].slider.value + adjustment, minValue, maxValue);

        if (newValue != sliders[0].slider.value)
        {
            UpdateAllValues(newValue);
        }
    }

    public void OnSliderChanged(float value, int index)
    {
        if (sliders[index].lastValue != value)
        {
            UpdateAllValues(value);
        }
    }

    public float GetValue()
    {
        return sliders[0].slider.value;
    }
}
