using Unity.Netcode;
using UnityEngine;


public class CoalitionButtonManager : NavButtonManager
{
    protected override void ResetNavButtonStates()
    {
        TrySelectButton_FromMouseInput_ServerRPC(ClientManager.LocalClientGameId, -1, 0);
    }

    public override void OnNetworkSpawn()
    {
        if (buttonAnims == null)
        {
            buttonAnims = GetComponentsInChildren<NavButton>(true);

            for (int i = 0; i < buttonAnims.Length; i++)
            {
                buttonAnims[i].Initialize(i, OnSelectNewButton_FromMouseInput);
            }
        }
    }



    #region Try Select New Button Through Mouse Input

    protected override void OnSelectNewButton_FromMouseInput(int newButtonId)
    {
        if (buttonsEnabled == false) return;

        TrySelectButton_FromMouseInput_ServerRPC(ClientManager.LocalClientGameId, selectedButtonId, newButtonId);
    }


    /// <summary>
    /// Try Select new button trough server.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void TrySelectButton_FromMouseInput_ServerRPC(int requestingClientGameId, int oldButtonId, int newButtonId)
    {
        bool isTeamFull = CoalitionManager.IsTeamFull(newButtonId);

        //if already selected button is pressed again with mouse OR the team is already full, call wobbly animation on current selected button and return
        if (oldButtonId == newButtonId || isTeamFull)
        {
            FailToSelectNewButton_ClientRPC(requestingClientGameId, isTeamFull);
            return;
        }


        //if requesting client can still join the team, update the server Data
        CoalitionManager.SetNewTeam_OnServer(requestingClientGameId, oldButtonId, newButtonId);

        //then call the according animation on that client
        SelectNewButton_FromMouseInput_ClientRPC(requestingClientGameId, newButtonId);
    }


    /// <summary>
    /// Get Callback from server that signals that attempted button selection with mouse was allowed.
    /// </summary>
    [ClientRpc(RequireOwnership = false)]
    private void SelectNewButton_FromMouseInput_ClientRPC(int requestingClientGameId, int newButtonId)
    {
        //only call on reqesting client
        if (requestingClientGameId != ClientManager.LocalClientGameId) return;

        //deselect old button
        buttonAnims[selectedButtonId].Anim.SetBool("Selected", false);

        //select new button
        selectedButtonId = newButtonId;

        //when a button is selected, invoke OnClick with buttonId
        OnClick?.Invoke(newButtonId);

        buttonAnims[newButtonId].Anim.SetTrigger("MoveError");
        buttonAnims[newButtonId].Anim.SetBool("Selected", true);
    }

    #endregion




    #region Try Select New Button Through Move Input

    protected override void OnMoveInput(Vector2 moveInput)
    {
        if (buttonsEnabled == false) return;

        TrySelectNewButton_FromMoveInput_ServerRPC(ClientManager.LocalClientGameId, selectedButtonId, moveInput);
    }


    [ServerRpc(RequireOwnership = false)]
    private void TrySelectNewButton_FromMoveInput_ServerRPC(int requestingClientGameId, int oldButtonId, Vector2 moveInput)
    {
        //math.sign so when pressing A+W the vector2 which would be .7, .7 is converted to 1, 1.
        moveInput = GetTrueMoveInput(moveInput);

        //get potential new selectedButtonId
        (bool changed, int newButtonId) = GetNewButtonIdFromMoveInput(moveInput);

        //if WASD movement cant select new button, call wobbly animation animation on that reqeustingClient on current selected button and return
        if (changed == false)
        {
            FailToSelectNewButton_ClientRPC(requestingClientGameId);
            return;
        }


        //if requesting client can still join the team, update the server Data
        CoalitionManager.SetNewTeam_OnServer(requestingClientGameId, oldButtonId, newButtonId);

        //Trigger Animations for old button (deselect) and new button (select wiggle with direction)
        SelectNewButton_FromMoveInput_ClientRPC(requestingClientGameId, oldButtonId, newButtonId, moveInput);
    }


    /// <summary>
    /// Get Callback from server that signals that attempted button selection with mouse was allowed.
    /// </summary>
    [ClientRpc(RequireOwnership = false)]
    private void SelectNewButton_FromMoveInput_ClientRPC(int requestingClientGameId, int oldButtonId, int newButtonId, Vector2 moveInput)
    {
        //only call on reqesting client
        if (requestingClientGameId != ClientManager.LocalClientGameId) return;

        //select new button
        selectedButtonId = newButtonId;

        //deselect old button
        buttonAnims[oldButtonId].Anim.SetBool("Selected", false);

        //set AnimId equivelant to the direction moveInput
        int animId = 0;
        if (moveInput.x > 0) animId = 1;
        if (moveInput.y > 0) animId = 2;
        if (moveInput.y < 0) animId = 3;

        buttonAnims[newButtonId].Anim.SetBool("Selected", true);
        buttonAnims[newButtonId].Anim.SetInteger("WiggleId", animId);
    }

    #endregion




    /// <summary>
    /// Get Callback from server that signals that attempted button selection failed.
    /// </summary>
    [ClientRpc(RequireOwnership = false)]
    private void FailToSelectNewButton_ClientRPC(int requestingClientGameId, bool wasTeamFull = false)
    {
        //only call on reqesting client
        if (requestingClientGameId != ClientManager.LocalClientGameId) return;

        buttonAnims[selectedButtonId].Anim.SetTrigger("MoveError");
    }
}
