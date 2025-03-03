using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance;
    [SerializeField] private bool _shouldSyncHoverObject;
    [SerializeField] private GameObject _hoverObject;

    [SerializeField] private BuildingHandler _buildingHandler;

    public GameObject lastHitObject { get; private set; }
    public GameObject currentBuildingTile { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
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
            lastHitObject = _hit.transform.gameObject;
        }
        else
        {
            lastHitObject = null;
        }
    }

    private void CheckForHoverable()
    {
        if (lastHitObject == null) { _hoverObject.SetActive(false); return; }

        //Check if the object has the IOnHover interface on it
        if (lastHitObject.transform.TryGetComponent(out IHoverable IH))
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
        if (lastHitObject == null || ctx.performed == false) return;

        if (lastHitObject == null) return;


        if (lastHitObject.TryGetComponent(out IOnClickable IOC))
        {
            IOC.OnClick();
        }

        if (EventSystem.current.IsPointerOverGameObject()) { return; }

        if (lastHitObject.TryGetComponent(out IBuildable IB))
        {
            if (currentBuildingTile == null) { currentBuildingTile = gameObject; }
            if (lastHitObject.GetInstanceID() == currentBuildingTile.GetInstanceID())
            {
                _buildingHandler.HideBuildingPanel();
                currentBuildingTile = null;
            }
            else
            {
                _buildingHandler.ShowBuildingPanel(IB);
                currentBuildingTile = lastHitObject;
            }
        }
    }
}
