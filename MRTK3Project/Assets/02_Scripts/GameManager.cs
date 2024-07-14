using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


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

    // 겜 종료
    public void Quit()
    {
        Debug.Log("종료 눌림");
        Application.Quit();
    }

}
