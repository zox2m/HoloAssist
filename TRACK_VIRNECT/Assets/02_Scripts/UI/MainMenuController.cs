using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] GameObject infoPopUp;
    [SerializeField] string documentationURL;

    private void Start()
    {
        infoPopUp.SetActive(false);
    }

    /// <summary>
    /// called from button to load the standalone scene
    /// </summary>
    public void LoadStandaloneScene()
    {
        SceneManager.LoadScene("Standalone");
    }

    /// <summary>
    /// called from button to load the fusion tracker scene
    /// </summary>
    public void LoadFusionTrackerScene()
    {
        SceneManager.LoadScene("FusionTracker");
    }

    /// <summary>
    /// shows the info pop up
    /// </summary>
    public void OpenInformationPopUp()
    {
        infoPopUp.SetActive(true);
    }

    /// <summary>
    /// closes the info pop up
    /// </summary>
    public void CloseInformationPopUp()
    {
        infoPopUp.SetActive(false);
    }

    /// <summary>
    /// opens the documentation in the default browser
    /// </summary>
    public void GoToDocumentation()
    {
        Application.OpenURL(documentationURL);
    }
}
