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


    private void TrySendText()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        inputField.ActivateInputField();

        SendTextGlobal_ServerRPC(NetworkManager.Singleton.LocalClientId, localPlayerName, inputField.text);

        inputField.text = "";
    }





    [ServerRpc(RequireOwnership = false)]
    public void SendTextGlobal_ServerRPC(ulong fromClientId, FixedString128Bytes senderName, FixedString128Bytes text)
    {
        SendTextGlobal_ClientRPC(fromClientId, senderName, text);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendTextGlobal_ClientRPC(ulong fromClientId, FixedString128Bytes senderName, FixedString128Bytes text)
    {
        StartCoroutine(AddTextToChatBox(fromClientId, senderName, text));
    }



    [ServerRpc(RequireOwnership = false)]
    public void SendTextToClient_ServerRPC(ulong toClientId, FixedString128Bytes senderName, FixedString128Bytes text)
    {
        SendTextToClient_ClientRPC(toClientId, senderName, text);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendTextToClient_ClientRPC(ulong toClientId, FixedString128Bytes senderName, FixedString128Bytes text)
    {
        //send to only "toClientId"
        if (NetworkManager.LocalClientId != toClientId) return;

        StartCoroutine(AddTextToChatBox(toClientId, senderName, text));
    }



    private IEnumerator AddTextToChatBox(ulong clientId, FixedString128Bytes playerName, FixedString128Bytes text)
    {
        GameObject obj = Instantiate(textBoxPrefab, chatContentHolder, false);

        TextMeshProUGUI textObj = obj.GetComponent<TextMeshProUGUI>();

        if (NetworkManager.LocalClientId == clientId && showLocalNameAs_You)
        {
            playerName = "You";
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
