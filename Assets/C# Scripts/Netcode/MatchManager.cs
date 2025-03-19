using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;


public class MatchManager : NetworkBehaviour
{
    //networkVar responsible for syncing the MatchData
    private static NetworkVariable<MatchSettings> matchSettingsNetworkVar = new NetworkVariable<MatchSettings>();

    [Tooltip("Retrieve MatchData")]
    public static MatchSettings settings;


    [Header("Where is UI Parent")]
    [SerializeField] private RectTransform UITransform;

    //default values
    [SerializeField] private MatchSettings defaultMatchSettings;
    public MatchSettings GetDefaultMatchSettings() => defaultMatchSettings;





    private async void Start()
    {
        //load saved MatchSettings, or load default if that doesnt exist.
        settings = await LoadSettingsFromFileAsync();

        UIComponentGroup[] UIInputHandlers = UITransform.GetComponentsInChildren<UIComponentGroup>(true);
        int UIhandlerCount = UIInputHandlers.Length;

        for (int i = 0; i < UIhandlerCount; i++)
        {
            int dataIndex = i;
            UIInputHandlers[i].Init(settings.GetSavedInt(dataIndex));

            UIInputHandlers[i].OnValueChanged += (value) => UpdateMatchSettingsData(dataIndex, value);
        }
    }

    private void UpdateMatchSettingsData(int sliderId, int value)
    {
        settings.SetIntData(sliderId, value);
    }


    /// <summary>
    /// Sync _matchSettings to server
    /// </summary>
    public async override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            matchSettingsNetworkVar.Value = settings;

            await SaveSettingsAsync(settings);
        }

        //on value changed event of matchsettings
        matchSettingsNetworkVar.OnValueChanged += (MatchSettings oldValue, MatchSettings newValue) =>
        {
            settings = newValue;
        };
    }


    private async Task<MatchSettings> LoadSettingsFromFileAsync()
    {
        (bool succes, MatchSettings loadedMatchSettings) = await FileManager.LoadInfo<MatchSettings>("SaveData/CreateLobbySettings.fpx", false);

        if (succes)
        {
            return loadedMatchSettings;
        }
        else
        {
            return GetDefaultMatchSettings();
        }
    }

    /// <summary>
    /// Settings are saved when creating the lobby
    /// </summary>
    private async Task SaveSettingsAsync(MatchSettings data)
    {
        await FileManager.SaveInfo(data, "SaveData/CreateLobbySettings.fpx", false);
    }
}
