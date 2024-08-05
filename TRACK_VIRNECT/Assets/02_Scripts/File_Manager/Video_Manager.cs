using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class Video_Manager
{
    private VideoClip video;

    public Video_Manager()
    {
        video = null;
    }

    public VideoClip get_Video(string object_string)
    {
        string file_path = "Mp4_Folder/" + object_string;
        Debug.Log(file_path);

        video = Resources.Load<VideoClip>(file_path);
        if(video == null )
        {
            Debug.Log("No video");
            return null;
        }

        return video;
    }
}