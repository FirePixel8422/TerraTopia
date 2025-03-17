using Unity.Burst;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


[BurstCompile]
public class NavButtonManager : NetworkBehaviour
{
    [Space(15)]

    [SerializeField]
    public UnityEvent<int> OnConfirm, OnClick;

    [Header("Call OnClick event on this script and selected button shen selecting new button")]
    public bool callOnClickWhenSelectingButton;


    [Header("UI Movement Keys")]
    [SerializeField] private InputAction UIMoveInput = new InputAction("UIMove", InputActionType.Value);

    [Space(10)]

    [Header("UI Confirm Keys")]
    [SerializeField] private InputAction UIConfirmInput = new InputAction("UIConfirm", InputActionType.Button);

    [Space(10)]

    protected NavButton[] buttonAnims;
    [SerializeField] protected int selectedButtonId;


    public bool buttonsEnabled = true;

    [BurstCompile]
    public void ToggleEnabledState(bool state)
    {
        buttonsEnabled = state;

        for (int i = 0; i < buttonAnims.Length; i++)
        {
            buttonAnims[i].enabled = state;
        }
    }




    #region Setup Input Events And Button Animation States

    /// <summary>
    /// setup input events
    /// </summary>
    private void OnEnable()
    {
        UIMoveInput.Enable();
        UIConfirmInput.Enable();

        UIMoveInput.performed += (InputAction.CallbackContext ctx) => OnMoveInput(ctx.ReadValue<Vector2>());
        UIMoveInput.canceled += _ => lastInput = Vector2.zero;

        UIConfirmInput.performed += _ => OnConfirmInput();

        //set button asnimation and selection states
        ResetNavButtonStates();
    }

    /// <summary>
    /// Reset Button Animation States and set selected buttonId to 0
    /// </summary>
    protected virtual void ResetNavButtonStates()
    {
        if (buttonAnims == null)
        {
            buttonAnims = GetComponentsInChildren<NavButton>(true);

            for (int i = 0; i < buttonAnims.Length; i++)
            {
                buttonAnims[i].Initialize(i, OnSelectNewButton_FromMouseInput);
            }
        }

        //select button 0
        buttonAnims[0].Anim.SetTrigger("MoveError");
        buttonAnims[0].Anim.SetBool("Selected", true);

        //set selected buttonId to 0
        selectedButtonId = 0;


        //deselect all ither buttons
        for (int i = 1; i < buttonAnims.Length; i++)
        {
            buttonAnims[i].Anim.SetBool("Selected", false);
        }
    }




    /// <summary>
    /// disable input events
    /// </summary>
    private void OnDisable()
    {
        UIMoveInput.Disable();
        UIConfirmInput.Disable();

        UIMoveInput.performed -= (InputAction.CallbackContext ctx) => OnMoveInput(ctx.ReadValue<Vector2>());
        UIMoveInput.canceled -= _ => lastInput = Vector2.zero;

        UIConfirmInput.performed -= _ => OnConfirmInput();
    }

    #endregion




    /// <summary>
    /// if a new button is selcted through mouseClick input
    /// </summary>
    protected virtual void OnSelectNewButton_FromMouseInput(int newButtonId)
    {
        if (buttonsEnabled == false) return;

        //if already selected button is pressed again with mouse, call wobbly animation on current selected button and return
        if (selectedButtonId == newButtonId)
        {
            buttonAnims[selectedButtonId].Anim.SetTrigger("MoveError");
            return;
        }


        //deselect old button
        buttonAnims[selectedButtonId].Anim.SetBool("Selected", false);


        //select new button
        selectedButtonId = newButtonId;

        //when a button is selected, invoke OnClick with buttonId
        OnClick?.Invoke(newButtonId);

        buttonAnims[newButtonId].Anim.SetTrigger("MoveError");
        buttonAnims[newButtonId].Anim.SetBool("Selected", true);
    }



    #region Select New Button Through Move Input

    private Vector2 lastInput;

    protected virtual void OnMoveInput(Vector2 moveInput)
    {
        if (buttonsEnabled == false) return;

        //math.sign so when pressing A+W the vector2 which would be .7, .7 is converted to 1, 1.
        moveInput = GetTrueMoveInput(moveInput);

        //save old selectedButtonId
        int oldSelectedButtonId = selectedButtonId;

        //get potential new selectedButtonId
        (bool changed, int newSelectedButtonId) = GetNewButtonIdFromMoveInput(moveInput);
        selectedButtonId = newSelectedButtonId;



        //if WASD movement cant select new button, call wobbly animation on current selected button adn return
        if (changed == false)
        {
            buttonAnims[selectedButtonId].Anim.SetTrigger("MoveError");
            return;
        }

        //Trigger Animations for old button (deselect) and new button (select wiggle with direction)
        SelectNewButton_FromMoveInput(oldSelectedButtonId, selectedButtonId, moveInput);
    }


    /// <summary>
    /// turns the moveDir (0.7f, 0) into a 1 or -1 on every axis (.7 => 1, -.7 => -1)
    /// </summary>
    /// <returns>The Input only from the keys that were actually pressed this frame, not the vector</returns>
    [BurstCompile]
    protected Vector2 GetTrueMoveInput(Vector2 moveInput)
    {
        new Vector2(math.sign(moveInput.x), math.sign(moveInput.y));

        //only use fresh input.
        //when holding W and A and then realesing A will cause cause vector.x to still be 1, altough W is not freshly pressed. so set those "fake" inputs to 0
        if (moveInput.x == lastInput.x) moveInput.x = 0;
        if (moveInput.y == lastInput.y) moveInput.y = 0;

        lastInput = moveInput;

        return moveInput;
    }

    /// <summary>
    /// Use WASD Input to check and get what button should be selected.
    /// </summary>
    /// <returns>That buttons Id</returns>
    [BurstCompile]
    protected (bool changed, int newId) GetNewButtonIdFromMoveInput(Vector2 moveInput)
    {
        //pre-initialized
        bool changed = false;
        int connectedButtonId;
        int newButtonId = selectedButtonId;

        //left
        if (moveInput.x < 0 && buttonAnims[selectedButtonId].TryGetConnection(0, out connectedButtonId))
        {
            newButtonId = connectedButtonId;
            changed = true;
        }
        //right
        else if (moveInput.x > 0 && buttonAnims[selectedButtonId].TryGetConnection(1, out connectedButtonId))
        {
            newButtonId = connectedButtonId;
            changed = true;
        }

        //up
        if (moveInput.y > 0 && buttonAnims[selectedButtonId].TryGetConnection(2, out connectedButtonId))
        {
            newButtonId = connectedButtonId;
            changed = true;
        }
        //down
        else if (moveInput.y < 0 && buttonAnims[selectedButtonId].TryGetConnection(3, out connectedButtonId))
        {
            newButtonId = connectedButtonId;
            changed = true;
        }

        return (changed, newButtonId);
    }


    /// <summary>
    /// if a new button is selcted through Move input
    /// </summary>
    [BurstCompile]
    protected virtual void SelectNewButton_FromMoveInput(int oldButtonId, int newButtonId, Vector2 moveInput)
    {
        if (callOnClickWhenSelectingButton)
        {
            //when a button is selected, invoke OnClick with buttonId and call OnClick on selected button
            OnClick?.Invoke(newButtonId);
            buttonAnims[selectedButtonId].OnClick?.Invoke();
        }

        //deselect old button
        buttonAnims[oldButtonId].Anim.SetBool("Selected", false);

        //set AnimId equivelant to the direction moveInput
        int animId = 0;
        if (moveInput.x > 0) animId= 1;
        if (moveInput.y > 0) animId = 2;
        if (moveInput.y < 0) animId = 3;

        buttonAnims[newButtonId].Anim.SetBool("Selected", true);
        buttonAnims[newButtonId].Anim.SetInteger("WiggleId", animId);
    }

    #endregion



    /// <summary>
    /// When Confirm input is pressed invoke OnConfirm<int selectedButtonId> and aditionally OnConfirm on the selected button
    /// </summary>
    [BurstCompile]
    private void OnConfirmInput()
    {
        if (buttonsEnabled == false) return;

        OnConfirm?.Invoke(selectedButtonId);

        buttonAnims[selectedButtonId].OnConfirm?.Invoke();
    }




    //clean up function (action) ref from navbuttons to the OnMouseClick function in this script
    public override void OnDestroy()
    {
        base.OnDestroy();

        for (int i = 0; i < buttonAnims.Length; i++)
        {
            buttonAnims[i].CleanUpEventData(OnSelectNewButton_FromMouseInput);
        }
    }
}