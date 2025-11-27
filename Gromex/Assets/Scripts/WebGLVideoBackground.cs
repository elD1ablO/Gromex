using UnityEngine;
using UnityEngine.Video;
using System.IO;

[RequireComponent(typeof(VideoPlayer))]
public class WebGLVideoBackground : MonoBehaviour
{
    [SerializeField] private string _fileName = "video_1280.mp4";

    private void Start()
    {
        var vp = GetComponent<VideoPlayer>();

        string path = Path.Combine(Application.streamingAssetsPath, _fileName);
        path = path.Replace("\\", "/");

        vp.source = VideoSource.Url;
        vp.url = path;
        vp.isLooping = true;

        vp.audioOutputMode = VideoAudioOutputMode.None;

        vp.prepareCompleted += OnPrepared;
        vp.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
    }
}

