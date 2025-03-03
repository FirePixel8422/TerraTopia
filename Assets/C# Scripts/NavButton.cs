using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NavButton : MonoBehaviour, IPointerClickHandler
{
    private Action<int> OnClick;

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
