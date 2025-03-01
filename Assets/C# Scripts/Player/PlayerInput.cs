using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private bool _shouldSyncHoverObject;
    [SerializeField] private GameObject _hoverObject;

    [SerializeField] private BuildingHandler _buildingHandler;
    private GameObject _lastHitObject;
    private GameObject _currentBuildingTile;
    private void Start()
    {
        //Initialize the _hoverObject
        _hoverObject = Instantiate(_hoverObject);
        _hoverObject.SetActive(false);
    }
    public void OnMouseMove(InputAction.CallbackContext ctx)
    {
        //Raycast before any checks to make sure they use the updated gameobject and not the previous one
        Raycast();

        if (_shouldSyncHoverObject) { CheckForHoverable(); }
    }

    private Ray _Ray => Camera.main.ScreenPointToRay(Input.mousePosition);
    private RaycastHit _hit;
    private void Raycast()
    {
        if (Physics.Raycast(_Ray, out _hit))
        {
            _lastHitObject = _hit.transform.gameObject;
        }
        else
        {
            _lastHitObject = null;
        }
    }

    private void CheckForHoverable()
    {
        if (_lastHitObject == null) { _hoverObject.SetActive(false); return; }

        //Check if the object has the IOnHover interface on it
        if (_lastHitObject.transform.TryGetComponent(out IHoverable IH))
        {
            OnHoveringOverObject(IH);
        }
        else
        {
            _hoverObject.SetActive(false);
        }

    }
    private void OnHoveringOverObject(IHoverable IH)
    {
        _hoverObject.SetActive(true);
        IH.OnHover(_hoverObject.transform);
    }

    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (_lastHitObject == null) return;


        if (_lastHitObject.TryGetComponent(out IOnClickable IOC))
        {
            IOC.OnClick();
        }


        if (_lastHitObject.TryGetComponent(out IBuildable IB))
        {
            if(_currentBuildingTile == null) { _currentBuildingTile = gameObject; }
            if (_lastHitObject.GetInstanceID() == _currentBuildingTile.GetInstanceID())
            {
                _buildingHandler.HideBuildingPanel();
                _currentBuildingTile = null;
            }
            else
            {
                _buildingHandler.ShowBuildingPanel(IB);
                _currentBuildingTile = _lastHitObject;
            }
        }
    }
}
