using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AllContorl;

public class StartMenu : MonoBehaviour
{

    //¿ªÊ¼ÓÎÏ·
    public void StartGame()
    {
        GameManager.Instance.ResetRunState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
