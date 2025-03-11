using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class CharacterShowcaser : MonoBehaviour
{
    [Header("Mouse Input")]
    [SerializeField] private InputAction mouseMoveInput;
    [Space]
    [SerializeField] private InputAction mouseClickInput;

    [Space]

    [SerializeField] private float turnSpeed;

    private Transform modelTransform;

    [SerializeField] private bool mouseHeld;

    [SerializeField] private int cUnitId;
    [SerializeField] private int cTribeId;



    private void Start()
    {
        UpdatePreviewModel(0, 0);
    }


    private void OnEnable()
    {
        mouseMoveInput.Enable();
        mouseMoveInput.performed += (InputAction.CallbackContext ctx) => OnMouseMovement(ctx.ReadValue<Vector2>());

        mouseClickInput.Enable();
        mouseClickInput.performed += (InputAction.CallbackContext ctx) => OnMouseHeldChanged(true);
        mouseClickInput.canceled += (InputAction.CallbackContext ctx) => OnMouseHeldChanged(false);
    }

    private void OnDestroy()
    {
        mouseMoveInput.Disable();
        mouseMoveInput.performed -= (InputAction.CallbackContext ctx) => OnMouseMovement(ctx.ReadValue<Vector2>());

        mouseClickInput.Disable();
        mouseClickInput.performed -= (InputAction.CallbackContext ctx) => OnMouseHeldChanged(true);
        mouseClickInput.canceled -= (InputAction.CallbackContext ctx) => OnMouseHeldChanged(false);
    }




    private void OnMouseMovement(Vector2 mouseMovement)
    {
        if (mouseHeld == false)
        {
            return;
        }

        float rotY = modelTransform.localEulerAngles.y - mouseMovement.x * turnSpeed;

        modelTransform.localEulerAngles = new Vector3(0, rotY, 0);
    }

    private void OnMouseHeldChanged(bool newState)
    {
        if (newState == false)
        {
            mouseHeld = false;
            return;
        }


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.transform.HasComponent<CharacterShowcaser>())
            {
                mouseHeld = true;
            }
        }
    }




    public void NextOrPreviousUnit(int change)
    {
        int newUnitId = cUnitId + change;

        int totalUnits = TribeSelecter.Instance.tribeData[cTribeId].unitSpawnData.Length;

        if (newUnitId == -1)
        {
            newUnitId = totalUnits;
        }
        else if (newUnitId == totalUnits)
        {
            newUnitId = 0;
        }

        UpdatePreviewModel(cTribeId, newUnitId);
    }

    public void SelectNewTribe(int tribeId)
    {
        if(TribeSelecter.Instance.tribeData.Length <= tribeId)
        {
            Debug.LogError("Tribe " + tribeId + " does not exist yet");
            return;
        }

        UpdatePreviewModel(tribeId, 0);
    }


    private void UpdatePreviewModel(int tribeId, int unitId)
    {
        cTribeId = tribeId;
        cUnitId = unitId;

        if (modelTransform != null)
        {
            Destroy(modelTransform.gameObject);
        }

        modelTransform = InstantiateUnit_Locally(tribeId, unitId);
    }

    private Transform InstantiateUnit_Locally(int tribeId, int unitId)
    {
        UnitSpawnData unitData = TribeSelecter.Instance.tribeData[tribeId].unitSpawnData[unitId];

        //spawn unit (locally on server)
        UnitBase spawnedUnit = Instantiate(unitData.body, transform);
        Instantiate(unitData.head, spawnedUnit.headTransform);

        spawnedUnit.colorRenderer.material = unitData.colorMaterials[tribeId];

        return spawnedUnit.transform;
    }
}
