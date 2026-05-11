using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PropertySlot : MonoBehaviour
{
    public Slider slider;
    public TMP_InputField inputField;
    public string sufix = "(kg)";
    public string floats = "F1";
    
    void Update()
    {
        if (inputField == null || slider == null || Application.isPlaying) return;
        
        inputField.text = $"{slider.value.ToString(floats)} {sufix}";
    }

    public void OnSubmitInputField()
    {
        if (float.TryParse(inputField.text, out float result))
        {
            slider.value = Mathf.Clamp(result, slider.minValue, slider.maxValue);
            UpdateText();
        }
        else
        {
            UpdateText();
        }
    }

    public void OnSubmitSlider()
    {
        UpdateText();
    }
    
    void UpdateText()
    {
        inputField.text = $"{slider.value.ToString(floats)} {sufix}";
    }
}
