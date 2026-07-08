using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    private AudioSource finishSound;    //终点音效

    private bool levelComplete = false;    //是否完成关卡

    // Start is called before the first frame update
    void Start()
    {
        finishSound = GetComponent<AudioSource>();  //挂载组件
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //如果Player且游戏完成
        if (collision.gameObject.name == "Player" && !levelComplete)
        {
            finishSound.Play(); //播放完成音效
            levelComplete = true;
            Invoke("CompleteLevel", 2f);    //2s后调用
        }
    }

    private void CompleteLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);   //场景跳转
    }
}
