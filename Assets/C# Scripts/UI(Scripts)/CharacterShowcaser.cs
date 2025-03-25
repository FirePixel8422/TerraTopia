using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


public class CharacterShowcaser : NetworkBehaviour
{
    [SerializeField] private InputAction mouseMoveInput;
    [Space]
    [SerializeField] private InputAction mouseClickInput;

    [Space]

    [SerializeField] private float turnSpeed;

    private Transform modelTransform;

    private bool mouseHeld;

    private int cUnitId;



    private void OnEnable()
    {
        mouseMoveInput.Enable();
        mouseMoveInput.performed += (InputAction.CallbackContext ctx) => OnMouseMovement(ctx.ReadValue<Vector2>());

        mouseClickInput.Enable();
        mouseClickInput.performed += (InputAction.CallbackContext ctx) => OnMouseHeldChanged(true);
        mouseClickInput.canceled += (InputAction.CallbackContext ctx) => OnMouseHeldChanged(false);
    }

    public override void OnDestroy()
    {
        mouseMoveInput.Disable();
        mouseMoveInput.performed -= (InputAction.CallbackContext ctx) => OnMouseMovement(ctx.ReadValue<Vector2>());

        mouseClickInput.Disable();
        mouseClickInput.performed -= (InputAction.CallbackContext ctx) => OnMouseHeldChanged(true);
        mouseClickInput.canceled -= (InputAction.CallbackContext ctx) => OnMouseHeldChanged(false);
    }


    private void Awake()
    {
        //NetworkManager.SceneManager.OnLoadEventCompleted += (_, _, _, _) => OnAllClientScenesLoaded();
    }

    private void OnAllClientScenesLoaded()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= (_, _, _, _) => OnAllClientScenesLoaded();

        UpdatePreviewModel(0, 0);
    }



    private void OnMouseMovement(Vector2 mouseMovement)
    {
        if (mouseHeld == false) return;

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

        int totalUnits = TribeSelecter.Instance.GetSelectedTribeData().Length;

        if (newUnitId == -1)
        {
            newUnitId = totalUnits - 1;
        }
        else if (newUnitId == totalUnits)
        {
            newUnitId = 0;
        }

        UpdatePreviewModel(TribeSelecter.selectedTribeId, newUnitId);
    }

    public void SelectNewTribe(int tribeId)
    {
        if (TribeSelecter.Instance.tribeData.Length <= tribeId)
        {
            Debug.LogError("Tribe " + tribeId + " does not exist yet");
            return;
        }

        UpdatePreviewModel(tribeId, cUnitId);
    }


    private void UpdatePreviewModel(int tribeId, int unitId)
    {
        TribeSelecter.selectedTribeId = tribeId;
        cUnitId = unitId;

        Quaternion rot = Quaternion.identity;

        if (modelTransform != null)
        {
            rot = modelTransform.localRotation;

            Destroy(modelTransform.gameObject);
        }

        modelTransform = InstantiateUnit_Locally(tribeId, unitId, rot);
    }


    private Transform InstantiateUnit_Locally(int tribeId, int unitId, Quaternion rot)
    {
        UnitSpawnData unitData = TribeSelecter.Instance.tribeData[tribeId].unitSpawnData[unitId];

        //spawn unit (locally on server)
        UnitBase spawnedUnit = Instantiate(unitData.body, transform).GetComponent<UnitBase>();
        NetworkObject unitHead = Instantiate(unitData.head, spawnedUnit.headTransform);

        //set rot equal to previous preview units rot
        spawnedUnit.transform.localRotation = rot;

#if UNITY_EDITOR
        if (overrideTeamColor != -1)
        {
            //set team color material
            spawnedUnit.colorRenderer.material = unitData.colorMaterials[overrideTeamColor];
        }
        else
        {
            //set team color material
            spawnedUnit.colorRenderer.material = unitData.colorMaterials[ClientManager.LocalClientGameId];
        }
#else
        //set team color material
        spawnedUnit.colorRenderer.material = unitData.colorMaterials[ClientManager.LocalClientGameId];
#endif

        return spawnedUnit.transform;
    }


#if UNITY_EDITOR
    [Range(-1, 7)]
    [SerializeField] private int overrideTeamColor;
#endif
}