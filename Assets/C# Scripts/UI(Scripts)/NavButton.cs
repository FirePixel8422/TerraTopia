using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class NavButton : MonoBehaviour, IPointerClickHandler
{
    [Space(15)]

    [SerializeField]
    public UnityEvent OnClick, OnConfirm;

    private Action<int> OnClickNavManagerCallback;

    [Header("Left, Right, Up, Down")]
    [SerializeField] private NavButton[] connections = new NavButton[4];


    /// <summary>
    /// Try Get Connected Tile its id, if that connection exists.
    /// </summary>
    /// <param name="connectionId">Left 0, Right 1, Up 2, Down 3</param>
    /// <returns>wheather the connection exists</returns>
    public bool TryGetConnection(int connectionId, out int connectedButtonId)
    {
        NavButton connection = connections[connectionId];

        if (connection == null)
        {
            connectedButtonId = -1;
            return false;
        }
        else
        {
            connectedButtonId = connection.buttonId;
            return true;
        }
    }


    public Animator Anim { get; private set; }
    private int buttonId = 0;



    //set buttonId and set action callback reference
    public void Initialize(int _buttonId, Action<int> onclickCallbackMethod)
    {
        buttonId = _buttonId;

        OnClickNavManagerCallback = onclickCallbackMethod;

        Anim = GetComponent<Animator>();
    }

    //destroy action callback reference
    public void CleanUpEventData(Action<int> onclickCallbackMethod)
    {
        OnClickNavManagerCallback -= onclickCallbackMethod;
    }



    /// <summary>
    /// Called when image is clicked on, just like a regular button
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
        OnClickNavManagerCallback?.Invoke(buttonId);
    }




#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private void OnValidate()
    {
        if (connections.Length != 0)
        {
            DEBUG_Force4Connections();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] == null)
            {
                continue;
            }

            Gizmos.color = Color.red;


            //if the connection of this button also has a connection back to this button, make gizmos Green
            if (connections[i].connections[Invert(i)] != null && connections[i].connections[Invert(i)].buttonId == connections[i].buttonId)
            {
                Gizmos.color = Color.green;
            }



            Vector3 worldDir = (transform.position - connections[i].transform.position).normalized;
            Vector3 connectionDir = Vector3.zero;

            switch (i)
            {
                case 0:
                    connectionDir = Vector3.left;
                    break;

                case 1:
                    connectionDir = Vector3.right;
                    break;

                case 2:
                    connectionDir = Vector3.up;
                    break;

                case 3:
                    connectionDir = Vector3.down;
                    break;
            }

            if (Vector3.Dot(worldDir, connectionDir) / 360 < 1)
            {
                Gizmos.DrawLine(transform.position, connections[i].transform.position - worldDir);   
            }
            else
            {
                Gizmos.DrawLine(transform.position + connectionDir * 150 * 0.1f, transform.position + connectionDir * 150 * 0.3f);
            }
        }
    }

    private int Invert(int i)
    {
        switch (i)
        {
            case 0:
                return 1;

            case 1:
                return 0;

            case 2:
                return 3;

            case 3:
                return 2;

            default:
                return -1;
        }
    }


    private void DEBUG_Force4Connections()
    {
        NavButton[] prevConnections = connections;

        connections = new NavButton[4];

        for (int i = 0; i < math.min(prevConnections.Length, 4); i++)
        {
            connections[i] = prevConnections[i];
        }
    }

#endif
}
