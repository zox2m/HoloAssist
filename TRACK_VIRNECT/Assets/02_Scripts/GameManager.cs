using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    
    public GameObject clepsydraPrefab;
    private GameObject trackedTarget;
    public TextMeshProUGUI infoText;
    public GameObject[] trackTargets; // 타겟들 옵젝 모아두는 배열. 일단은 인스펙터에서 연결 . 0퓨쳐셀프 1씨름 2 

    public FileManger fm; // 인스펙터에서 할당 

    private string targetName;

    // book target 찾았을 때 콜백 
    public void Callback_StartTarget(int targetIndex)
    {

        //targetTransform = GameObject.FindWithTag("TrackTarget").transform.position;
        trackedTarget = trackTargets[targetIndex];

        infoText = trackedTarget.transform.GetChild(1).GetComponent<TextMeshProUGUI>(); // 해당 타겟의 text 


        debugText.text = targetIndex + ": Detected";
        debugText.color = Color.green;

        // target 위치에 모델 생성 
        //Instantiate(clepsydraPrefab, targetTransform, Quaternion.identity); // 약간 오프셋을 줌

        // 안내 UI 표시
        //infoText.enabled = true;
        infoText.transform.position = trackedTarget.transform.GetChild(0).position; // 타겟의 위치로 이동 
        infoText.transform.rotation = trackedTarget.transform.GetChild(0).rotation;

        // 안내 음성 재생 

        // 해당 위치에 귀칼 재생.. 일단 데모해야하니까 존나 하드코딩할게요 ㅈㅅㅈㅅ
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
                Debug.Log(" 해당하는 타겟을 찾지 못함");
                break;
        }

        fm.OnDetected(targetName);


    }

    public void Callback_StopBookTarget()
    {
        debugText.text = "Searching...";
        debugText.color = Color.yellow;
    }

    // 겜 종료
    public void Quit()
    {
        Debug.Log("종료 눌림");
        Application.Quit();
    }

}
