using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class Test_Behaviour : MonoBehaviour
{
    public Object panel;
    private Video_Manager vm;
    private Text_Manager tm;
    string object_string = "test";
    // Start is called before the first frame update
    void Start()
    {
        tm = new Text_Manager();
        vm = new Video_Manager();
        Debug.Log(tm.get_Text(object_string));
        panel.GetComponent<VideoPlayer>().clip = vm.get_Video(object_string);
        panel.GetComponent<VideoPlayer>().Prepare();
        panel.GetComponent<VideoPlayer>().Play();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
