using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoverable
{
    public Transform hoverObjectHolder { get; set; }

    public void OnHover(Transform _hoverObject);
}
