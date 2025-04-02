using System.Collections;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private void Awake()
    {
        Instance = this;
    }



    private AudioSource source;

    [SerializeField] private SerializableKeyValueList<string, MusicPlayListDataSO> playlists;

    private MusicPlayListDataSO selectedPlayList;
    private int selectedTrackId;



    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;

        //get first playlist.
        playlists.TryGetValue(playlists.GetKeyById(0), out selectedPlayList);

        //play first track of first playlist.
        PlayTrack(0);

        DontDestroyOnLoad(gameObject);
    }

    public void PlayTrack(int newTrackId, bool shufflePlayList = true)
    {
        if (shufflePlayList)
        {
            ShufflePlaylist(selectedPlayList);
        }

        source.clip = selectedPlayList.clips[newTrackId];
        source.Play();

        StopAllCoroutines();
        StartCoroutine(AutoStartNextTrackTimer());
    }

    public void PlayTrack(string playListName, int newTrackId, bool shufflePlayList = true)
    {
        if (playlists.TryGetValue(playListName, out selectedPlayList))
        {
            if (shufflePlayList)
            {
                ShufflePlaylist(selectedPlayList);
            }

            source.clip = selectedPlayList.clips[newTrackId];
            source.Play();

            StopAllCoroutines();
            StartCoroutine(AutoStartNextTrackTimer());
        }
    }

    private void ShufflePlaylist(MusicPlayListDataSO playList)
    {
        int n = playList.clips.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1); // Generate random index

            // Swap playList[i] with array[j]
            AudioClip temp = playList.clips[i];
            playList.clips[i] = playList.clips[j];
            playList.clips[j] = temp;
        }
    }

    private IEnumerator AutoStartNextTrackTimer()
    {
        yield return new WaitForSeconds(source.clip.length);

        selectedTrackId++;

        if (selectedTrackId >= selectedPlayList.clips.Length)
        {
            selectedTrackId = 0;
        }

        PlayTrack(selectedTrackId);
    }
}
