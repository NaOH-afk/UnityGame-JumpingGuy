using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    [SerializeField] private GameObject[] waypoints;    //存储路径点的数组
    private int currentWaypointIndex = 0;

    [SerializeField] private float speed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //如果物体接近路径点
        if(Vector2.Distance(waypoints[currentWaypointIndex].transform.position,transform.position)<.1f)
        {
            currentWaypointIndex++; //移动至下一路径点
            if (currentWaypointIndex >= waypoints.Length)   //到达最后路径点
            {
                currentWaypointIndex = 0;   //回到开始位置
            }
        }

        //朝向当前路径点移动
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, Time.deltaTime * speed);
    }
}
