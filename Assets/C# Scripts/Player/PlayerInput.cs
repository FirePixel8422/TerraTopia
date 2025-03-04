using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance;
    [SerializeField] private bool _shouldSyncHoverObject;
    [SerializeField] private GameObject _hoverObject;

    [SerializeField] private BuildingHandler _buildingHandler;

    public GameObject lastHitObject { get; private set; }

    public GameObject selectedObject { get; private set; }
    public GameObject currentBuildingTile { get; private set; }

    private GraphicRaycaster gfxRayCaster;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        gfxRayCaster = FindObjectOfType<GraphicRaycaster>(true);
    }
    private void Start()
    {
        //Initialize the _hoverObject
        _hoverObject = Instantiate(_hoverObject);
        _hoverObject.SetActive(false);
    }
    public void OnMouseMove(InputAction.CallbackContext ctx)
    {
        //Check whether or not the cursor is hovering over any source of UI to prevent sending raycasts
        if (IsHoveringOverUI()) { return; }

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

    private bool IsHoveringOverUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        var results = new List<RaycastResult>();
        gfxRayCaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            return true;
        }
        return false;
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
        selectedObject = lastHitObject;

        if (selectedObject.TryGetComponent(out IOnClickable IOC))
        {
            IOC.OnClick();
        }

        if (selectedObject.TryGetComponent(out IBuildable IB))
        {
            // First, check if the object has any available buildings
            List<Building> buildings = IB.AvailableBuildings();
            if (EventSystem.current.IsPointerOverGameObject()) { return; }

            // Hide the building panel if no buildings are available
            if (buildings.Count == 0)
            {
                _buildingHandler.HideBuildingPanel();
                return;
            }

            if (currentBuildingTile == null) { currentBuildingTile = gameObject; }

            if (selectedObject.GetInstanceID() == currentBuildingTile.GetInstanceID())
            {
                _buildingHandler.HideBuildingPanel();
                currentBuildingTile = null;
            }
            else
            {
                _buildingHandler.ShowBuildingPanel(IB);
                currentBuildingTile = selectedObject;
            }
        }
        else
        {
            // If it's not a buildable object, hide the building panel
            _buildingHandler.HideBuildingPanel();
        }
    }

}
