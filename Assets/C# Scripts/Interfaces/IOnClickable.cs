using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnClickable 
{
    public void OnClick(int playerId);

    public void OnDifferentClickableClicked(GameObject newlyClickedObject);
}
