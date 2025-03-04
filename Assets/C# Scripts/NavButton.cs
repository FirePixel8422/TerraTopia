using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.iOS;


public class NavButton : MonoBehaviour, IPointerClickHandler
{
    public Action<int> OnClick;

    [Header("Left, Right, Up, Down")]
    [SerializeField] private NavButton[] connections = new NavButton[4];

    [SerializeField] private int buttonId = 0;


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

    public Animator anim;



    //set buttonId and set action callaback reference
    public void Initialize(int _buttonId, Action<int> onclickCallbackMethod)
    {
        buttonId = _buttonId;

        OnClick += onclickCallbackMethod;

        anim = GetComponent<Animator>();


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        savedButtonId = _buttonId;
#endif
    }

    //destroy action callback reference
    public void CleanUpEventData(Action<int> onclickCallbackMethod)
    {
        OnClick -= onclickCallbackMethod;
    }



    /// <summary>
    /// Called when image is clicked on, just like a regular button
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick.Invoke(buttonId);
    }




#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private int savedButtonId;

    private void OnValidate()
    {
        if (buttonId != savedButtonId)
        {
            buttonId = savedButtonId;
        }

        if (connections.Length != 4)
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


            Vector3 dir = Vector3.zero;

            switch (i)
            {
                case 0:
                    dir = Vector3.left;
                    break;

                case 1:
                    dir = Vector3.right;
                    break;

                case 2:
                    dir = Vector3.up;
                    break;

                case 3:
                    dir = Vector3.down;
                    break;
            }

            Gizmos.DrawLine(transform.position + 0.15f * 150 * dir, transform.position + 0.3f * 180 * dir);
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
