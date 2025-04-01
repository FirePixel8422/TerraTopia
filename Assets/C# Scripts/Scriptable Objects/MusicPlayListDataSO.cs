using UnityEngine;


[CreateAssetMenu(fileName = "MusicPlayListData", menuName = "ScriptableObjects/MusicPlayList")]
public class MusicPlayListDataSO : ScriptableObject
{
    public AudioClip[] clips;
}