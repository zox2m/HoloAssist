// Copyright (C) 2024 VIRNECT CO., LTD.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuButtonController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// called from button to go back to the main menu
    /// </summary>
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
