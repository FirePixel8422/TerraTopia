using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


[BurstCompile]
public class CameraController : MonoBehaviour, ICustomUpdater
{
    [Header("Input Actions")]
    [SerializeField] private PlayerControlsMap inputActionMap;

    private Camera mainCam;

    private Transform rotationTransform;
    private Transform moveTransform;
    private Transform zoomTransform;




    [BurstCompile]
    private void Start()
    {
        mainCam = Camera.main;

        moveTransform = transform.GetChild(0);

        rotationTransform = transform;
        rotationTransform.position = new Vector3(GridManager.Instance._length * 0.5f, 5, GridManager.Instance._width * 0.5f);

        zoomTransform = mainCam.transform;
        newZoomTransformPos = zoomTransform.localPosition;

        Zoom(-100);

        Cursor.lockState = CursorLockMode.None;

        //initialize updateManager (once) so it functions
        CustomUpdaterManager.Initialize();

        CustomUpdaterManager.AddUpdater(this);
    }




    #region Camera Movement

    private bool posUpdated;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;

    [SerializeField] private float heightTimesSpeedMultiplier;
    [SerializeField] private float minSpeed, maxSpeed;

    private Vector2 moveDir;

    [BurstCompile]
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            moveDir = ctx.ReadValue<Vector2>();
            posUpdated = true;
        }

        if (ctx.canceled)
        {
            moveDir = Vector2.zero;
            posUpdated = false;
        }
    }



    private bool sprinting;

    [BurstCompile]
    public void OnSprintToggle(InputAction.CallbackContext ctx)
    {
        sprinting = ctx.ReadValueAsButton();
    }

    #endregion




    #region Camera Zoom

    private const float baseScrollMultiplier = 1f / 120f;

    [Header("Zoom")]
    [SerializeField] private int minScrolls, maxScrolls;
    private int cScrolls;

    [Header("How far does scroll go")]
    [SerializeField] private float scrollHeightMultiplier = 1;

    [Header("Time in seconds to scroll from A to B")]
    [SerializeField] private float scrollTime;

    private Vector3 newZoomTransformPos;


    private float newCameraPosDistance;
    private bool zoomUpdated;


    [BurstCompile]
    public void OnZoom(InputAction.CallbackContext ctx)
    {
        if (ctx.performed == false)
        {
            return;
        }


        float mouseScrolls = ctx.ReadValue<float>() * baseScrollMultiplier;

        Zoom(mouseScrolls);
    }

    private void Zoom(float mouseScrolls)
    {
        int prevScrolls = cScrolls;
        cScrolls = math.clamp(cScrolls + (int)mouseScrolls, minScrolls, maxScrolls);

        float scrollDifference = cScrolls - prevScrolls;

        if (scrollDifference != 0)
        {
            newZoomTransformPos += scrollDifference * scrollHeightMultiplier * zoomTransform.TransformDirection(Vector3.forward);

            newCameraPosDistance = Vector3.Distance(zoomTransform.localPosition, newZoomTransformPos);
            zoomUpdated = true;
        }
    }

    #endregion




    #region Camera Rotate

    private bool rightClickHeld;

    [BurstCompile]
    public void OnRightClick(InputAction.CallbackContext ctx)
    {
        rightClickHeld = ctx.ReadValueAsButton();

        //Cursor.lockState = rightClickHeld ? CursorLockMode.Locked : CursorLockMode.None;
    }

    [Header("Rotate")]
    [SerializeField] private float mouseRotSpeed;

    [SerializeField] private Vector2 mouseMovement;
    [SerializeField] private bool rotUpdated;


    [BurstCompile]
    public void OnMouseMove(InputAction.CallbackContext ctx)
    {
        if (rightClickHeld && ctx.performed)
        {
            mouseMovement = ctx.ReadValue<Vector2>();
            rotUpdated = true;
        }
        else
        {
            mouseMovement = Vector2.zero;
            rotUpdated = false;
        }
    }

    #endregion




    public bool RequireUpdate => true;

    [BurstCompile]
    public void OnUpdate()
    {
        //UpdateCamera();
    }

    private void Update()
    {
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        //update camera position
        if (posUpdated)
        {
            //multiply speed based on how far up you are, clamped between min and max
            float _moveSpeed = math.clamp(zoomTransform.position.y * heightTimesSpeedMultiplier, minSpeed, maxSpeed);

            //use sprint speed if sprinting, moveSpeed if not
            _moveSpeed *= sprinting ? sprintSpeed : moveSpeed;

            moveTransform.position += moveTransform.TransformDirection(_moveSpeed * Time.deltaTime * new Vector3(moveDir.x, 0, moveDir.y));
        }

        if (rotUpdated)
        {
            Quaternion rot = rotationTransform.rotation * Quaternion.Euler(-mouseMovement.y * mouseRotSpeed * Vector3.right + -mouseMovement.x * mouseRotSpeed * Vector3.up);
            rotationTransform.rotation = Quaternion.Euler(0, rot.eulerAngles.y, 0);
        }

        //update camera zoom in/out
        if (zoomUpdated)
        {
            zoomTransform.localPosition = Vector3.MoveTowards(zoomTransform.localPosition, newZoomTransformPos, newCameraPosDistance / scrollTime * Time.deltaTime);

            if (zoomTransform.localPosition == newZoomTransformPos)
            {
                zoomUpdated = false;
            }
        }
    }
}
