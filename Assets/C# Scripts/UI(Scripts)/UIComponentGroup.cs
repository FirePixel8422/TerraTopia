using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;



public class UIComponentGroup : MonoBehaviour
{
    [Header("<<Info>>\nAdd Max 1 Slider, 1 Toggle and 1 InputField on or under this GamObject")]
    [Space(5)]

    [Header("Min and Max values of the Slider and InputField part of this group (if it exists)")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 1;

    [Header("Default value of the Toggle part of this group (if it exists)")]
    [SerializeField] private bool toggleDefaultValue;

    [Header("Should InputFields Display nothing when value is 0? (if it exists)")]
    [SerializeField] private bool inputFieldDisplayAirWhenZero;


    private Slider slider;
    private Toggle toggle;
    private TMP_InputField inputField;

    [HideInInspector] public UnityAction<int> OnValueChanged;


    /// <summary>
    /// Initialize by getting the slider, toggle and input field components and setting their values to the startValue
    /// </summary>
    public void Init(int startValue)
    {
        slider = GetComponentInChildren<Slider>(true);
        toggle = GetComponentInChildren<Toggle>(true);
        inputField = GetComponentInChildren<TMP_InputField>(true);


        //call update UI when any value of any of the 3 components is changed
        OnValueChanged += UpdateUI;

        if (slider != null)
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;

            slider.onValueChanged.AddListener((float value) => OnValueChanged.Invoke(math.clamp((int)value, minValue, maxValue)));
        }
        if (toggle != null)
        {
            toggle.isOn = toggleDefaultValue;
            toggle.onValueChanged.AddListener((bool value) => OnValueChanged.Invoke(value ? 1 : 0));
        }
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener((string value) =>
            {
                if (value == "-") return;

                OnValueChanged.Invoke(MathematicsLogic.ClampToInt(string.IsNullOrEmpty(value) ? 0 : long.Parse(value), minValue, maxValue));
            });
        }

        UpdateUI(startValue);
    }



    private void UpdateUI(int value)
    {
        value = math.clamp(value, minValue, maxValue);

        if (slider != null)
        {
            slider.value = value;
        }
        if (toggle != null)
        {
            toggle.isOn = value == 0 ? false : true;
        }
        if (inputField != null)
        {
            inputField.text = value == 0 && inputFieldDisplayAirWhenZero ? "" : value.ToString();
        }
    }
}
