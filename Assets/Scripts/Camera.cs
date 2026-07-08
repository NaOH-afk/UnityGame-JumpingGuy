using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private Transform player;  //Transform 是摄像机的组件，控制其旋转和缩放

    void Update()
    {
        //摄像机位置：跟随一个三维向量/坐标，由player的x和y，以及摄像机自己的z坐标组成
        transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
    }
}
