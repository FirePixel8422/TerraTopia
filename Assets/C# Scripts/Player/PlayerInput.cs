using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance;

    [SerializeField] private bool _shouldSyncHoverObject;
    [SerializeField] private GameObject _hoverObjectPrefab;
    [SerializeField] private BuildingHandler _buildingHandler;

    private GameObject _hoverObject;
    private GraphicRaycaster _gfxRayCaster;

    public GameObject LastHitObject { get; private set; }
    public GameObject SelectedObject { get; private set; }
    public GameObject CurrentBuildingTile { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _gfxRayCaster = FindObjectOfType<GraphicRaycaster>(true);
    }

    private void Start()
    {
        _hoverObject = Instantiate(_hoverObjectPrefab);
        _hoverObject.SetActive(false);
    }

    public void OnMouseMove(InputAction.CallbackContext ctx)
    {
        if (IsHoveringOverUI()) return;

        Raycast();

        if (_shouldSyncHoverObject) CheckForHoverable();
    }

    private Ray _Ray => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void Raycast()
    {
        if (Physics.Raycast(_Ray, out RaycastHit hit))
        {
            LastHitObject = hit.transform.gameObject;
        }
        else
        {
            LastHitObject = null;
        }
    }

    private bool IsHoveringOverUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        _gfxRayCaster.Raycast(pointerEventData, results);

        return results.Count > 0;
    }

    private void CheckForHoverable()
    {
        if (LastHitObject == null)
        {
            _hoverObject.SetActive(false);
            return;
        }

        if (LastHitObject.TryGetComponent(out IHoverable hoverable))
        {
            _hoverObject.SetActive(true);
            hoverable.OnHover(_hoverObject.transform);
        }
        else
        {
            _hoverObject.SetActive(false);
        }
    }

    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (IsHoveringOverUI() || LastHitObject == null || !ctx.performed) return; 

        if (SelectedObject != null && SelectedObject != LastHitObject)
        {
            if (SelectedObject.TryGetComponent(out IOnClickable previousIOC))
            {
                previousIOC.OnDifferentClickableClicked(LastHitObject);
            }
        }

        if (SelectedObject != null)
        {
            if (SelectedObject != LastHitObject)
            {
                if (LastHitObject.TryGetComponent(out IOnClickable IOC))
                {
                    IOC.OnClick();
                }
            }
        }


        SelectedObject = LastHitObject;


        if (SelectedObject.TryGetComponent(out IBuildable IB))
        {
            HandleBuildingPanel(IB);
        }
        else
        {
            _buildingHandler.HideBuildingPanel();
        }
    }

    private void HandleBuildingPanel(IBuildable buildable)
    {
        List<Building> buildings = buildable.AvailableBuildings();

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (buildings.Count == 0)
        {
            _buildingHandler.HideBuildingPanel();
            return;
        }

        if (CurrentBuildingTile == null)
        {
            CurrentBuildingTile = gameObject;
        }

        if (SelectedObject.GetInstanceID() == CurrentBuildingTile.GetInstanceID())
        {
            _buildingHandler.HideBuildingPanel();
            CurrentBuildingTile = null;
        }
        else
        {
            _buildingHandler.ShowBuildingPanel(buildable);
            CurrentBuildingTile = SelectedObject;
        }
    }
}
