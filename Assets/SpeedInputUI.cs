using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;

public class SpeedInputUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Supercyan.FreeSample.SimpleSampleCharacterControl player;

    [Header("Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private int port = 5005; // Must match UNITY_PORT in Python

    // Data received from Python (0 to 50 based on Python code)
    private float receivedBetaValue = 0f;
    private bool isClientConnected = false;
    private float lastPacketTime = 0f;
    [SerializeField] private float connectionTimeout = 1.0f; // seconds

    // UDP Variables
    private Thread receiveThread;
    private UdpClient client;
    private bool isRunning = true;

    void Start()
    {
        // Start the background thread to listen for Python
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        // Check connection timeout
        if (Time.time - lastPacketTime > connectionTimeout)
        {
            isClientConnected = false;
        }

        if (isClientConnected)
        {
            // Map Python range (0 to 50) to Unity Slider range (0 to 1)
            float targetSliderValue = Mathf.Clamp01(receivedBetaValue / 50.0f);

            // Smoothly move the slider for visual clarity
            speedSlider.value = Mathf.Lerp(speedSlider.value, targetSliderValue, Time.deltaTime * 5f);
        }
        // else: slider is controlled manually by the user

        // Apply actual speed to character (always from slider)
        float speed = speedSlider.value * maxSpeed;
        if (player != null)
        {
            player.SetForwardSpeed(speed);
        }
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (isRunning)
        {
            try
            {
                // Receive bytes
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // Convert bytes to string (e.g., "15.50")
                string text = Encoding.UTF8.GetString(data);

                // Parse string to float safely
                // CultureInfo.InvariantCulture ensures "." is treated as decimal point regardless of region
                if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
                {
                    receivedBetaValue = parsedValue;
                    isClientConnected = true;
                    lastPacketTime = Time.time;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"UDP Error: {e.Message}");
            }
        }
    }

    void OnApplicationQuit()
    {
        // Cleanup thread and socket on exit
        isRunning = false;
        if (client != null) client.Close();
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Abort();
    }
}