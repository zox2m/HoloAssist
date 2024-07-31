using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region SingleTon Pattern
    public static UIManager Instance { get; private set; }
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

    public string filePath; // 텍스트 파일의 경로
    public TextMeshProUGUI textUI; //  Text UI
    public TextMeshProUGUI videoUI; //  Video UI


    // 현재는 파일을 직접 참조하게 되어있지만, 텍스트 매니저를 만들면 이를 참조하도록.
    void ReadTextFile()
    {
        if (File.Exists(filePath))
        {
            string text = File.ReadAllText(filePath);
            textUI.text = text;
        }
        else
        {
            Debug.LogError("파일을 찾을 수 없습니다: " + filePath);
        }
    }

    // 현재는 파일을 직접 참조하게 되어있지만, 비디오 매니저를 만들면 이를 참조하도록.
    void ReadVideoFile()
    {

    }
}
