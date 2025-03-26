using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TextSender : NetworkBehaviour
{
    public static TextSender Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }




    public GameObject textBoxPrefab;
    public Transform chatContentHolder;

    public Scrollbar scrollBar;
    private TMP_InputField inputField;

    private FixedString128Bytes localPlayerName;

    public bool showLocalNameAs_You;
    public bool active;

    public Color serverMessagesColor;

    public float toggleSpeed;

    public Vector3 enabledPos;
    public Vector3 disabledPos;


    public override void OnNetworkSpawn()
    {
        inputField = GetComponentInChildren<TMP_InputField>();

        localPlayerName = new FixedString128Bytes(ClientManager.LocalUserName);
    }


    public void ToggleUI()
    {
        active = !active;
        StartCoroutine(ToggleUITimer(active ? enabledPos : disabledPos));
    }
    private IEnumerator ToggleUITimer(Vector3 pos)
    {
        while (Vector3.Distance(transform.localPosition, pos) > 0.001f)
        {
            yield return null;
            transform.localPosition = VectorLogic.InstantMoveTowards(transform.localPosition, pos, toggleSpeed * Time.deltaTime);
        }
    }


    public void TrySendText()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        if (inputField.text.ToLower() == "/seed")
        {
            SendTextGlobal_ServerRPC(-1, "Server", "Map Seed =\n" + MatchManager.settings.seed.ToString());
        }
        else
        {
            SendTextGlobal_ServerRPC(ClientManager.LocalClientGameId, localPlayerName, inputField.text);
        }

        inputField.ActivateInputField();
        inputField.text = "";
    }





    [ServerRpc(RequireOwnership = false)]
    public void SendTextGlobal_ServerRPC(int clientGameId, FixedString128Bytes senderName, FixedString128Bytes text)
    {
        SendTextGlobal_ClientRPC(clientGameId, senderName, text);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendTextGlobal_ClientRPC(int clientGameId, FixedString128Bytes senderName, FixedString128Bytes text)
    {
        StartCoroutine(AddTextToChatBox(clientGameId, senderName, text));
    }



    [ServerRpc(RequireOwnership = false)]
    public void SendTextToClient_ServerRPC(int clientGameId, FixedString128Bytes senderName, FixedString128Bytes text)
    {
        SendTextToClient_ClientRPC(clientGameId, senderName, text);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendTextToClient_ClientRPC(int clientGameId, FixedString128Bytes senderName, FixedString128Bytes text)
    {
        //send to only "toClientId"
        if (ClientManager.LocalClientGameId != clientGameId) return;

        StartCoroutine(AddTextToChatBox(clientGameId, senderName, text));
    }



    private IEnumerator AddTextToChatBox(int clientGameId, FixedString128Bytes playerName, FixedString128Bytes text)
    {
        GameObject obj = Instantiate(textBoxPrefab, chatContentHolder, false);

        TextMeshProUGUI textObj = obj.GetComponent<TextMeshProUGUI>();

        if (clientGameId == ClientManager.LocalClientGameId && showLocalNameAs_You)
        {
            playerName = "You";
        }
        else if (clientGameId == -1)
        {
            obj.GetComponent<TextMeshProUGUI>().color = serverMessagesColor;
        }

        textObj.text = $"[{playerName}]: " + text.ToString();

        //Set RectTransfrom Size to Fit all the text
        Vector2 temp = (textObj.transform as RectTransform).sizeDelta;
        temp.y = textObj.preferredHeight;
        (textObj.transform as RectTransform).sizeDelta = temp;

        yield return null;
        yield return new WaitForEndOfFrame();

        scrollBar.value = 0;
    }
}