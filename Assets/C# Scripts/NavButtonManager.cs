using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class NavButtonManager : MonoBehaviour
{
    public UnityEvent<int> OnConfirm;

    [Header("UI Movement Keys")]
    [SerializeField] private InputAction UIMoveInput;

    [Space(10)]

    [Header("UI Confirm Keys")]
    [SerializeField] private InputAction UIConfirmInput;

    [Space(10)]

    [SerializeField] private NavButton[] buttonAnims;
    [SerializeField] private int selectedButtonId;




    private void Start()
    {
        buttonAnims = GetComponentsInChildren<NavButton>();

        for (int i = 0; i < buttonAnims.Length; i++)
        {
            buttonAnims[i].Initialize(i, SelectNewButton_FromMouseInput);
        }

        buttonAnims[0].anim.SetTrigger("MoveError");
        buttonAnims[0].anim.SetBool("Selected", true);
    }



    #region Setup Input Events

    private void OnEnable()
    {
        StartCoroutine(InputEnablementDelay());
    }

    private IEnumerator InputEnablementDelay()
    {
        yield return new WaitForEndOfFrame();
        yield return null;

        LateEnable();
    }

    /// <summary>
    /// setup input events
    /// </summary>
    private void LateEnable()
    {
        UIMoveInput.Enable();
        UIConfirmInput.Enable();

        UIMoveInput.performed += (InputAction.CallbackContext ctx) => OnMoveInput(ctx.ReadValue<Vector2>());
        UIMoveInput.canceled += _ => lastInput = Vector2.zero;

        UIConfirmInput.performed += _ => OnConfirmInput();
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
    /// if a new button is selcted through MouseClick input
    /// </summary>
    private void SelectNewButton_FromMouseInput(int newButtonId)
    {
        //deselect old button
        buttonAnims[selectedButtonId].anim.SetBool("Selected", false);


        //select new button
        selectedButtonId = newButtonId;

        buttonAnims[newButtonId].anim.SetTrigger("MoveError");
        buttonAnims[newButtonId].anim.SetBool("Selected", true);
    }



    public Vector2 lastInput;

    private void OnMoveInput(Vector2 moveInput)
    {
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