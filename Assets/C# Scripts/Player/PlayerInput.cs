using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private GameObject _hoverObject;
    [SerializeField] private PlayerInputActions _input;
    private void Start()
    {
        //Initialize controls
        _input = new PlayerInputActions();

        //Initialize the _hoverObject
        _hoverObject = Instantiate(_hoverObject);
        _hoverObject.SetActive(false);

        //Initialize the raycasting
        _input.Mouse.OnMouseMove.performed += Raycast;
    }

    private Ray _Ray => Camera.main.ScreenPointToRay(Input.mousePosition);
    private RaycastHit _hit;
    private void Raycast(InputAction.CallbackContext obj)
    {
        if(Physics.Raycast(_Ray, out _hit))
        {
            //Check if the object has the IOnHover interface on it
            if(_hit.transform.TryGetComponent(out IHoverable ih))
            {
                OnHoverableObject(ih);
            }
            else
            {
                _hoverObject.SetActive(false);
            }
        }
        else
        {
            _hoverObject.SetActive(false);
        }
    }

    private void OnHoverableObject(IHoverable ih)
    {
        _hoverObject.SetActive(true);
        ih.OnHover(_hoverObject.transform);
    }
}
