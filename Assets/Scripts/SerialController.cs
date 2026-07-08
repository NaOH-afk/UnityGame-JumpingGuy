using System.IO.Ports;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AllContorl;

public class SerialController : MonoBehaviour
{
    SerialPort serialPort;
    private PlayerMovement player;
    private float lastReceiveTime;

    [Header("串口")]
    [SerializeField] private string comPort = "COM7";
    [SerializeField] private int baudRate = 9600;
    [Tooltip("断线或连接失败后，隔多少秒再尝试打开串口")]
    [SerializeField] private float reconnectIntervalSeconds = 2f;

    [Header("发给开发板的指令（与固件约定一致）")]
    [SerializeField] private string trapDeathMessage = "[trap]";
    [SerializeField] private string livesResetMessage = "[R]";
    [Tooltip("若固件按行读取（如 readStringUntil('\\n')），请勾选；发送内容为上面字符串再追加换行")]
    [SerializeField] private bool appendNewlineToCommands = true;

    [Header("调试")]
    [SerializeField] private bool logSerialDiagnostics;

    private float lastReconnectAttemptTime = -999f;
    private bool pendingSceneBoardReset;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneEntryLifeAndBoardSync();
        TryFlushPendingSceneBoardReset("sceneLoaded");
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
        lastReceiveTime = Time.time;
        TryConnectPort();
        ApplySceneEntryLifeAndBoardSync();
        TryFlushPendingSceneBoardReset("Start");
    }

    /// <summary>
    /// 每条新场景：生命 3、退出超神；并排队向开发板发 [R]（Unity 首场景不会触发 sceneLoaded，故 Start 里也会调一次）。
    /// </summary>
    private void ApplySceneEntryLifeAndBoardSync()
    {
        GameManager.Instance.playerLives = 3;
        pendingSceneBoardReset = true;
        if (logSerialDiagnostics)
            Debug.Log($"[Serial] 场景复位：已排队发送「{livesResetMessage}」。");
    }

    private void Update()
    {
        bool portReady = serialPort != null && serialPort.IsOpen;

        if (!portReady)
        {
            if (Time.time - lastReconnectAttemptTime >= reconnectIntervalSeconds)
                TryConnectPort();
        }

        if (player != null && Time.time - lastReceiveTime > 0.1f)
            player.serialDirX = 0;

        TryFlushPendingSceneBoardReset("Update");

        if (!portReady || player == null)
            return;

        try
        {
            while (serialPort.BytesToRead > 0)
            {
                char c = (char)serialPort.ReadByte();
                Debug.Log("收到：" + c);
                lastReceiveTime = Time.time;

                if (c == 'A')
                    player.serialDirX = -1;
                else if (c == 'B')
                    player.serialDirX = 1;
                else if (c == '0')
                    player.serialDirX = 0;
                else if (c == 'C')
                    player.SerialJump();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("串口读取异常，将断开并重试: " + e.Message);
            MarkDisconnected();
        }
    }

    /// <summary>串口已打开则立即发 [R]；否则保持 pending，等连上后由 Update 再发。</summary>
    private void TryFlushPendingSceneBoardReset(string reason)
    {
        if (!pendingSceneBoardReset)
            return;

        bool portReady = serialPort != null && serialPort.IsOpen;
        if (!portReady)
        {
            if (logSerialDiagnostics)
                Debug.Log($"[Serial] 待发「{livesResetMessage}」但串口未就绪（{reason}），已排队等待连接。");
            return;
        }

        if (!SendToBoard(livesResetMessage))
            return;

        pendingSceneBoardReset = false;
        if (logSerialDiagnostics)
            Debug.Log($"[Serial] 复位指令已通过 SendToBoard 发出（{reason}）。");
    }

    private void TryConnectPort()
    {
        lastReconnectAttemptTime = Time.time;

        if (serialPort != null)
            ClosePortInstance();

        try
        {
            serialPort = new SerialPort(comPort, baudRate);
            serialPort.Open();
            lastReceiveTime = Time.time;
            Debug.Log("串口连接成功: " + comPort);
            TryFlushPendingSceneBoardReset("TryConnectPort");
        }
        catch (System.Exception e)
        {
            serialPort = null;
            Debug.LogWarning($"串口打开失败 ({comPort}): {e.Message}，{reconnectIntervalSeconds} 秒后重试。");
        }
    }

    private void MarkDisconnected()
    {
        lastReconnectAttemptTime = Time.time;
        ClosePortInstance();
        Debug.LogWarning("串口已断开，进入重连等待。");
    }

    private void ClosePortInstance()
    {
        if (serialPort == null)
            return;

        try
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("串口 Close 异常: " + e.Message);
        }

        try
        {
            serialPort.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("串口 Dispose 异常: " + e.Message);
        }

        serialPort = null;
    }

    public void NotifyPlayerLostLife()
    {
        SendToBoard(trapDeathMessage);
    }

    /// <summary>
    /// 生命在游戏内恢复为满（例如命用尽回出生点、场景进关）：向开发板发 [R]，让 LED 全部重新点亮。
    /// 串口已打开则立即发送；否则排队，连上后由 Update 自动补发。
    /// </summary>
    public void NotifyLivesReset()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            if (SendToBoard(livesResetMessage))
            {
                pendingSceneBoardReset = false;
                if (logSerialDiagnostics)
                    Debug.Log("[Serial] 生命已恢复为 3 条，已发送「" + livesResetMessage + "」点亮 LED。");
                return;
            }
        }

        pendingSceneBoardReset = true;
        TryFlushPendingSceneBoardReset("NotifyLivesReset");
    }

    /// <returns>是否已成功写入串口</returns>
    private bool SendToBoard(string message)
    {
        if (!TrySendBoardMessage(message))
            return false;
        if (logSerialDiagnostics)
            Debug.Log("[Serial] 已发送: " + message);
        return true;
    }

    private bool TrySendBoardMessage(string message)
    {
        if (serialPort == null || !serialPort.IsOpen || string.IsNullOrEmpty(message))
            return false;

        try
        {
            serialPort.Write(message);
            if (appendNewlineToCommands)
                serialPort.Write("\n");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("串口发送失败: " + e.Message);
            MarkDisconnected();
            return false;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ClosePortInstance();
    }

    private void OnApplicationQuit()
    {
        ClosePortInstance();
    }
}
