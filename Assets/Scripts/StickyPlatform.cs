using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyPlatform : MonoBehaviour
{

    //如果人物触碰到了碰撞体，那么人物成为该碰撞体的子类（或碰撞体成为人物的父类）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //如果人物触碰到了碰撞体，那么人物成为该碰撞体的子类（或碰撞体成为人物的父类），
        //子类会做与父类一样的运动
        if (collision.gameObject.name == "Player")
        {
            collision.gameObject.transform.SetParent(transform);    //设置人物的父对象为transform
        }
    }

    //解除人物成为碰撞体的子类（或清除人物的父类）
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            collision.gameObject.transform.SetParent(null); //设置人物的父对象为空
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
