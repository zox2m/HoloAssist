using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    #region SingleTon Pattern
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // Set this as the instance and ensure it persists across scenes
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    public TextMeshProUGUI debugText;
    public GameObject[] trackTargets; // Ÿ�ٵ� ���� ��Ƶδ� �迭. �ϴ��� �ν����Ϳ��� ���� . 0ǻ�ļ��� 1���� 2 


    private GameObject trackedTarget;
    public TextMeshProUGUI infoText;
    public VideoPlayer infoVideo;

    public FileManger fm; // �ν����Ϳ��� �Ҵ� 

    private string targetName;

    // book target ã���� �� �ݹ� 
    public void Callback_StartTarget(int targetIndex)
    {

        //targetTransform = GameObject.FindWithTag("TrackTarget").transform.position;
        trackedTarget = trackTargets[targetIndex];

        infoText = trackedTarget.transform.GetChild(1).GetComponent<TextMeshProUGUI>(); // �ش� Ÿ���� text 
        infoVideo = trackedTarget.transform.GetChild(2).GetComponent<VideoPlayer>(); // �ش� Ÿ���� video

        //debugText.text = targetIndex + ": Detected";
        //debugText.color = Color.green;

        // target ��ġ�� �� ���� 
        //Instantiate(clepsydraPrefab, targetTransform, Quaternion.identity); // �ణ �������� ��

        // �ȳ� UI ǥ��
        infoText.transform.position = trackedTarget.transform.GetChild(0).position; // Ÿ���� ��ġ�� �̵� 
        infoText.transform.rotation = trackedTarget.transform.GetChild(0).rotation; // Ÿ���� �������� ȸ�� 

        // �ȳ� video ǥ��
        infoVideo.Prepare();
        infoVideo.Play();
        
        // �ȳ� ���� ��� 

        // �ش� ��ġ�� ��Į ���.. �ϴ� �����ؾ��ϴϱ� ���� �ϵ��ڵ��ҰԿ� ��������
        switch (targetIndex)
        {
            case 1:
                // MD
                targetName = "MD";
                break;
            case 2:
                // SS
                targetName = "SS";

                break;
            case 3:
                // JM
                targetName = "JM";

                break;
            default:
                Debug.Log(" �ش��ϴ� Ÿ���� ã�� ����");
                break;
        }

        // file manager �κ�, ���𶧴� ���� �׳� �ϵ��ڵ��� 
        //fm.OnDetected(targetName, trackedTarget);


    }

    public void Callback_StopBookTarget()
    {
        //debugText.text = "Searching...";
        //debugText.color = Color.yellow;

    }

    // �� ����
    public void Quit()
    {
        Debug.Log("���� ����");
        Application.Quit();
    }

}
