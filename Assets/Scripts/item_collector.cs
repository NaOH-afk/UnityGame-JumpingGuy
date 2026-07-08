using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AllContorl;

public class item_collector : MonoBehaviour
{
    //private int cherries = 0;   //初始 cherry 数量

    int cherries = GameManager.Instance.score;  //在同一场景下获取计分板数据


    [SerializeField] private Text cherriesText; //可修改文字
    [SerializeField] private AudioSource collectSoundEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Cherry"))
        {
            collectSoundEffect.Play();  //播放收集音效
            Destroy(collision.gameObject);
            cherries++; //收集
            cherriesText.text = "Cherries Collected:" + cherries;

            GameManager.Instance.score = cherries;

        }
    }
}
