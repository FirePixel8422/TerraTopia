using TMPro;
using Unity.Netcode;
using UnityEngine;


public class LobbyCodeDisplay : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (MatchManager.settings.privateLobby)
            {
                lobbyCodeText.transform.parent.gameObject.SetActive(true);
                lobbyCodeText.text = LobbyManager.CurrentLobby.Id;
            }
        }
    }


    public void CopyLobbyCodeToClipboard()
    {
        GUIUtility.systemCopyBuffer = lobbyCodeText.text;
    }
}
