using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    private void Awake()
    {
        Instance = this;
    }



    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private TMP_Dropdown dropdown;

    private Resolution[] resolutions;
    private List<Resolution> filterdResolutionList;

    private int cResolutionIndex;
    private RefreshRate cRefreshRate;

    [SerializeField] private TextMeshProUGUI fullscreenButtonText;

    [SerializeField] private bool displayRefreshRate;


    [SerializeField] private DisplayData displayData;


    private async void Start()
    {
        (bool loadSucces, DisplayData loadedDisplayData) = await FileManager.LoadInfo<DisplayData>("ResolutionAndFullScreen.fpx");

        //if data was found, use it
        if (loadSucces)
        {
            displayData = loadedDisplayData;

            Screen.SetResolution(displayData.width, displayData.height, displayData.fullScreenState);
        }

        //if no data was found, create it
        else
        {
            displayData = new DisplayData(1920, 1080, true);

            await FileManager.SaveInfo(displayData, "ResolutionAndFullScreen.fpx");
        }

        fullscreenButtonText.text = Screen.fullScreen ? "Go Windowed" : "Go Fullscreen";




        #region Setup DropDown menu for Resolution

        resolutions = Screen.resolutions;
        filterdResolutionList = new List<Resolution>();

        dropdown.ClearOptions();
        cRefreshRate = Screen.currentResolution.refreshRateRatio;

        for (int i = 0; i < resolutions.Length; i++)
        {
            bool isSixteenNineRatio = Mathf.Approximately((float)resolutions[i].width / (float)resolutions[i].height, 1.7777777777777777777777777777778f);
            if (resolutions[i].refreshRateRatio.Equals(cRefreshRate))
            {
                filterdResolutionList.Add(resolutions[i]);
            }
        }


        List<string> options = new List<string>();
        for (int i = 0; i < filterdResolutionList.Count; i++)
        {
            float refreshRate = (float)filterdResolutionList[i].refreshRateRatio.numerator / filterdResolutionList[i].refreshRateRatio.denominator;

            string resolutionOption = filterdResolutionList[i].width + " x " + filterdResolutionList[i].height + (displayRefreshRate ? (" " + refreshRate + "Hz") : "");

            options.Add(resolutionOption);
        }
        filterdResolutionList.Reverse();
        options.Reverse();

        dropdown.AddOptions(options);

        for (int i = 0; i < filterdResolutionList.Count; i++)
        {
            if (filterdResolutionList[i].width == Screen.width && filterdResolutionList[i].height == Screen.height)
            {
                cResolutionIndex = i;
                break;
            }
        }

        dropdown.value = cResolutionIndex;
        dropdown.captionText.text = Screen.width + "x" + Screen.height + " " + Screen.currentResolution.refreshRateRatio + "Hz";

        dropdown.RefreshShownValue();
        #endregion
    }



    public async void ToggleFullScreenStateAsync()
    {
        bool newState = !Screen.fullScreen;

        displayData.fullScreenState = newState;

        Screen.fullScreen = newState;
        fullscreenButtonText.text = newState ? "Go Windowed" : "Go Fullscreen";

        await FileManager.SaveInfo(displayData, "ResolutionAndFullScreen.fpx");
    }


    public async void SetResolutionAsync(int resolutionIndex)
    {
        Resolution resolution = filterdResolutionList[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        displayData.width = resolution.width;
        displayData.height = resolution.height;

        await FileManager.SaveInfo(displayData, "ResolutionAndFullScreen.fpx");
    }


    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }
}
