using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float speed = 2f;  //锯子转速

    void Update()
    {
        transform.Rotate(0, 0, 360 * speed * Time.deltaTime);   //旋转，x，y轴不动，z轴旋转
    }
}

//如何快捷创建新的锯子？？选中打包好的Saws文件夹（里面必须包含锯子对象和脚本和碰撞体和路径点），
//按住Ctrl+D复制一个，松开后再按E，可以进行旋转，
//也可以在“Inspector”界面的“Transform”下的“Rotation”通过修改“z”的值来修改角度