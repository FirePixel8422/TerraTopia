using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnTurnChangable 
{
    public int TurnActionsLeft { get; set; }
    public int AltTurnActionsLeft { get; set; }
    public void OnTurnChange(int turn);
}
