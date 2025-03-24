using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TextSender : NetworkBehaviour
{
    public GameObject textBoxPrefab;
    public Transform chatContentHolder;

    public Scrollbar scrollBar;
    private TMP_InputField inputField;
    public FixedString128Bytes localPlayerName;

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

    public void TrySendText()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        inputField.ActivateInputField();

        SendText_ServerRPC(NetworkManager.LocalClientId, localPlayerName, inputField.text);

        inputField.text = "";
    }


    [ServerRpc(RequireOwnership = false)]
    public void SendText_ServerRPC(ulong clientId, FixedString128Bytes playerName, FixedString128Bytes text)
    {
        SendText_ClientRPC(clientId, playerName, text);
    }
    [ClientRpc(RequireOwnership = false)]
    public void SendText_ClientRPC(ulong clientId, FixedString128Bytes playerName, FixedString128Bytes text)
    {
        StartCoroutine(AddTextToChatBox(clientId, playerName, text));
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
