using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class FileManger : MonoBehaviour
{
    //public Object panel;
    public GameObject videoPanel;
    public GameObject textPanel;

    private Video_Manager vm;
    private Text_Manager tm;
    string object_string = "test";
    // Start is called before the first frame update
    
    void Start()
    {
        tm = new Text_Manager();
        vm = new Video_Manager();
    }
    
    public void OnDetected(string object_string, GameObject targetObject)
    {
        textPanel = targetObject.transform.GetChild(1).gameObject;
        videoPanel = targetObject.transform.GetChild(2).gameObject;

        // tm : 글자 가져와서 표시 
        textPanel.GetComponent<TextMeshProUGUI>().text = tm.get_Text(object_string);

        // vm : 영상 가져와서 표시 
        videoPanel.GetComponent<VideoPlayer>().clip = vm.get_Video(object_string);
        videoPanel.GetComponent<VideoPlayer>().Prepare();
        videoPanel.GetComponent<VideoPlayer>().Play();

    }
}
