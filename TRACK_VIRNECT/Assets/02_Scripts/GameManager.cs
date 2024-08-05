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


    public void LoadMainSceneCall()
    {
        StartCoroutine(LoadScene("Main")); //비동기이므로 코루틴으로 호출 
    }

    // 타이틀 씬에서 메인 씬으로 이동 
    private IEnumerator LoadScene(string sceneName)
    {
        Debug.Log("게임매니저 - 로드씬 실행 됨  ");

        // 비동기적으로 씬 로딩
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);


        // 씬 로딩이 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 바뀐 씬을 현재 씬으로 변경하고 초기화해줌

    }

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
