using UnityEngine;

// 挂载在名为 Transmit 的方块上
public class Transmit : MonoBehaviour
{
    [Header("传送目标坐标（可在Inspector手动设置）")]
    // 公开变量，会自动显示在Inspector面板，可直接输入数值
    public Vector3 targetPosition;

    [Header("触发传送的标签（默认Player）")]
    public string playerTag = "Player";

    // 2D碰撞触发函数（必须配合碰撞体+Is Trigger勾选）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 判断碰到的是不是玩家
        if (collision.CompareTag(playerTag))
        {
            // 直接将玩家传送到目标坐标
            collision.transform.position = targetPosition;

            // 可选：传送时播放音效/特效，这里可以自己加
            // AudioSource.PlayClipAtPoint(传送音效, transform.position);
        }
    }
}