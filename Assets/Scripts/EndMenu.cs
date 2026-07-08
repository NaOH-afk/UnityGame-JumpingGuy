using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AllContorl;

public class EndMenu : MonoBehaviour
{

    //ÖØÐÂ¿ªÊ¼ÓÎÏ·
    public void ReloadGame()
    {
        GameManager.Instance.ResetRunState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 6);
    }

    //Ò²¿É»»³Équit()£¬²»¹ýÄÇÑùÊÇ¡°ÍË³öÓÎÏ·¡±
}
