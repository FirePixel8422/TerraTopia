using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnClickable 
{
    public void OnClick();

    public void OnDifferentClickableClicked(GameObject newlyClickedObject);
}
