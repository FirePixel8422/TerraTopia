using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class NavButtonManager : MonoBehaviour
{
    [SerializeField]
    public UnityEvent<int> OnConfirm, OnClick;

    [Header("UI Movement Keys")]
    [SerializeField] private InputAction UIMoveInput;

    [Space(10)]

    [Header("UI Confirm Keys")]
    [SerializeField] private InputAction UIConfirmInput;

    [Space(10)]

    private NavButton[] buttonAnims;
    public int selectedButtonId;


    public void ToggleEnabledState(bool state)
    {
        buttonsEnabled = state;

        for (int i = 0; i < buttonAnims.Length; i++)
        {
            buttonAnims[i].enabled = state;
        }
    }

    private bool buttonsEnabled = true;



    #region Setup Input Events And Button Animation Setup

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




        buttonAnims = GetComponentsInChildren<NavButton>();

        for (int i = 0; i < buttonAnims.Length; i++)
        {
            buttonAnims[i].Initialize(i, SelectNewButton_FromMouseInput);
        }

        buttonAnims[0].anim.SetTrigger("MoveError");
        buttonAnims[0].anim.SetBool("Selected", true);

        for (int i = 1; i < buttonAnims.Length; i++)
        {
            buttonAnims[i].anim.SetBool("Selected", false);
        }
        selectedButtonId = 0;
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
    private void SelectNewButton_FromMouseInput(int newButtonId)
    {
        //deselect old button
        buttonAnims[selectedButtonId].anim.SetBool("Selected", false);


        //select new button
        selectedButtonId = newButtonId;

        //when a button is selected, invoke OnClick with buttonId
        OnClick?.Invoke(newButtonId);

        buttonAnims[newButtonId].anim.SetTrigger("MoveError");
        buttonAnims[newButtonId].anim.SetBool("Selected", true);
    }



    public Vector2 lastInput;

    private void OnMoveInput(Vector2 moveInput)
    {
        if (buttonsEnabled == false) return;

        //math.sign so when pressing A+W the vector2 shich would be .7, .7 is used as 1, 1.
        moveInput = new Vector2(math.sign(moveInput.x), math.sign(moveInput.y));

        //only use fresh input.
        //when holding W and A and then realesing A will cause cause vector.x to still be 1, altough W is not freshly pressed. so set those "fake" inputs to 0
        if (moveInput.x == lastInput.x) moveInput.x = 0;
        if (moveInput.y == lastInput.y) moveInput.y = 0;

        lastInput = moveInput;


        //pre initialize int
        int connectedButtonId;

        //save old selectedButtonId
        int oldSelectedButtonId = selectedButtonId;

        //left
        if (moveInput.x < 0 && buttonAnims[selectedButtonId].TryGetConnection(0, out connectedButtonId))
        {
            selectedButtonId = connectedButtonId;
        }
        //right
        else if (moveInput.x > 0 && buttonAnims[selectedButtonId].TryGetConnection(1, out connectedButtonId))
        {
            selectedButtonId = connectedButtonId;
        }

        //up
        if (moveInput.y > 0 && buttonAnims[selectedButtonId].TryGetConnection(2, out connectedButtonId))
        {
            selectedButtonId = connectedButtonId;
        }
        //down
        else if (moveInput.y < 0 && buttonAnims[selectedButtonId].TryGetConnection(3, out connectedButtonId))
        {
            selectedButtonId = connectedButtonId;
        }

        //Trigger animations for old button (deselect) and new button (select wiggle with direction)
        SelectNewButton_FromMoveInput(oldSelectedButtonId, selectedButtonId, moveInput);
    }



    /// <summary>
    /// if a new button is selcted through Move input
    /// </summary>
    private void SelectNewButton_FromMoveInput(int oldButtonId, int newButtonId, Vector2 moveInput)
    {
        //deselect old button
        buttonAnims[oldButtonId].anim.SetBool("Selected", false);

        //set animId equivelant to the direction moveInput
        int animId = 0;
        if (moveInput.x > 0) animId = 1;
        if (moveInput.y > 0) animId = 2;
        if (moveInput.y < 0) animId = 3;

        buttonAnims[newButtonId].anim.SetBool("Selected", true);
        buttonAnims[newButtonId].anim.SetInteger("WiggleId", animId);
    }




    private void OnConfirmInput()
    {
        if (buttonsEnabled == false) return;

        OnConfirm?.Invoke(selectedButtonId);

        buttonAnims[selectedButtonId].OnConfirm?.Invoke();
    }




    private void OnDestroy()
    {
        for (int i = 0; i < buttonAnims.Length; i++)
        {
            buttonAnims[i].CleanUpEventData(SelectNewButton_FromMouseInput);
        }
    }
}