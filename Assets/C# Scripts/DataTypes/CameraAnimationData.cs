using UnityEngine;


[System.Serializable]
public struct CameraAnimationData
{
    public Transform toLookAtTransform;

    public float travelTime;
    public float waitTime;
}