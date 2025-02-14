using UnityEngine;
using UnityEngine.InputSystem;


public class MatchManager : MonoBehaviour
{
    public static MatchSettings matchSettings;

    [Header("UI Movement Keys")]
    public InputAction UIMoveInput;


    [Header("")]
    [SerializeField] private Animator[] buttonAnims;

    private int cTeamId;
    private int topRowLastSelectedTeamId;




    private void OnEnable()
    {
        UIMoveInput.Enable();
        UIMoveInput.performed += ctx => OnInputForSwapTeams(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        UIMoveInput.Disable();
        UIMoveInput.performed -= ctx => OnInputForSwapTeams(ctx.ReadValue<Vector2>());
    }


    public void OnInputForSwapTeams(Vector2 moveInput)
    {
        //save previous teamId
        int oldTeamId = cTeamId;

        //first move on X
        //keep track of swappedCount
        bool swapped = TrySwapTeam(new Vector2(moveInput.x, 0));

        //after X, calculate move on Y
        swapped = TrySwapTeam(new Vector2(0, moveInput.y)) || swapped;

        
        if (swapped)
        {
            SwapToTargetTeam(oldTeamId, cTeamId, moveInput);
        }
        //if there is no team swap after X and Y movement, call Fail method
        else
        {
            FailToFollowInputAnimation();
        }
    }

    private bool TrySwapTeam(Vector2 moveInput)
    {
        switch (cTeamId)
        {
            case 4: // Center Down
                if (moveInput.y > 0) { SetNewTeam(0); return true; } 
                if (moveInput.y < 0) { SetNewTeam(0); return true; }
                break;

            case 0: // Left
                if (moveInput.y != 0) { SetNewTeam(4); return true; }
                if (moveInput.x < 0) { SetNewTeam(3); return true; }
                if (moveInput.x > 0) { SetNewTeam(1); return true; }
                break;

            case 1: // Center Left
                if (moveInput.y != 0) { SetNewTeam(4); return true; }
                if (moveInput.x < 0) { SetNewTeam(0); return true; }
                if (moveInput.x > 0) { SetNewTeam(2); return true; }
                break;

            case 2: // Center Right
                if (moveInput.y != 0) { SetNewTeam(4); return true; }
                if (moveInput.x < 0) { SetNewTeam(1); return true; }
                if (moveInput.x > 0) { SetNewTeam(3); return true; }
                break;

            case 3: // Right
                if (moveInput.y != 0) { SetNewTeam(4); return true; }
                if (moveInput.x < 0) { SetNewTeam(2); return true; }
                if (moveInput.x > 0) { SetNewTeam(0); return true; }
                break;
        }

        return false;
    }

    private void SetNewTeam(int newTeam)
    {
        //when going to top row from no team button, re select last selected top row button
        if (cTeamId == 4)
        {
            newTeam = topRowLastSelectedTeamId;
        }
        //save last top row selected button
        else if(newTeam == 4)
        {
            topRowLastSelectedTeamId = cTeamId;
        }

        cTeamId = newTeam;
    }


    private void SwapToTargetTeam(int oldTeamId, int newTeamId, Vector2 moveInput)
    {
        int animId = 0;
        if (moveInput.x > 0)
        {
            animId = 1;
        }
        if (moveInput.y > 0)
        {
            animId = 2;
        }
        if (moveInput.y < 0)
        {
            animId = 3;
        }

        //deselect old button
        buttonAnims[oldTeamId].SetBool("Selected", false);

        //select new button
        buttonAnims[newTeamId].SetBool("Selected", true);
        buttonAnims[newTeamId].SetInteger("WiggleId", animId);
    }

    private void FailToFollowInputAnimation()
    {
        //update button animatio by shivering it
        buttonAnims[cTeamId].SetTrigger("MoveError");
    }
}
