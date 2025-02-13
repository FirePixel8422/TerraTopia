using UnityEngine;
using UnityEngine.InputSystem;


public class MatchManager : MonoBehaviour
{
    public static MatchSettings matchSettings;


    public void OnSwapTeams(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //read WASD input
            Vector2 moveInput = ctx.ReadValue<Vector2>();


            //first move on X
            //keep track of swappedCount
            bool swapped = TrySwapTeam(new Vector2(moveInput.x, 0));

            //after X, calculate move on Y
            swapped = TrySwapTeam(new Vector2(0, moveInput.y)) ? true : swapped;


            //if there is no team swap after X and Y movement, call Fail method
            if (swapped == false)
            {
                FailToFollowInputAnimation(moveInput);
            }
        }
    }


    private int cTeamId;

    private bool TrySwapTeam(Vector2 moveInput)
    {
        switch (cTeamId)
        {
            case -1: // Center
                if (moveInput.y > 0) { SetNewTeam(0); return true; } 
                if (moveInput.y < 0) { SetNewTeam(0); return true; }
                break;

            case 0: // LeftUp
                if (moveInput.y != 0) { SetNewTeam(-1); return true; }
                if (moveInput.x < 0) { SetNewTeam(3); return true; }
                if (moveInput.x > 0) { SetNewTeam(1); return true; }
                break;

            case 1: // RightUp
                if (moveInput.y != 0) { SetNewTeam(-1); return true; }
                if (moveInput.x < 0) { SetNewTeam(0); return true; }
                if (moveInput.x > 0) { SetNewTeam(2); return true; }
                break;

            case 2: // DownLeft
                if (moveInput.y != 0) { SetNewTeam(-1); return true; }
                if (moveInput.x < 0) { SetNewTeam(1); return true; }
                if (moveInput.x > 0) { SetNewTeam(3); return true; }
                break;

            case 3: // DownRight
                if (moveInput.y != 0) { SetNewTeam(-1); return true; }
                if (moveInput.x < 0) { SetNewTeam(2); return true; }
                if (moveInput.x > 0) { SetNewTeam(0); return true; }
                break;
        }

        return false;
    }

    private void SetNewTeam(int newTeam)
    {
        cTeamId = newTeam;
    }


    public void SwapToTargetTeam(int newTeam)
    {
        
    }

    public void FailToFollowInputAnimation(Vector2 moveInput)
    {

    }
}
