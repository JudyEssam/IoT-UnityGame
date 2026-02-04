using UnityEngine;
using TMPro;
// 1. Add these namespaces for Networking
using System.Net.Sockets;
using System.Text;

public class CoinCollect : MonoBehaviour
{
    static private int coincount;
    public TextMeshProUGUI coinText;
    public AudioSource collectSound;
    public CoinConcentrationChecker checker;

    // 2. Define the target port (Must match PYTHON_PORT in eeg_simulator.py)
    private int pythonPort = 5006;
    private string pythonIP = "127.0.0.1";

    private void Start()
    {
        coincount = 0;
        UpdateCoinUI();
    }

    private void Awake()
    {
        if (checker == null)
        {
            checker = FindObjectOfType<CoinConcentrationChecker>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSound != null)
            {
                collectSound.Play();
            }

            if (checker)
            {
                checker.AddCoin(1);
            }

            // ---------------------------------------------------------
            // 3. SEND UDP MESSAGE TO PYTHON
            // ---------------------------------------------------------
            SendFeedbackToPython();

            Destroy(gameObject);
            coincount++;
            UpdateCoinUI();
        }
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + coincount;
        }
    }

    // 4. Helper function to send the specific message
    private void SendFeedbackToPython()
    {
        try
        {
            // We create a temporary client for this one-off message
            UdpClient udpClient = new UdpClient();

            byte[] data = Encoding.UTF8.GetBytes("COIN_COLLECTED");

            // Send to localhost:5006
            udpClient.Send(data, data.Length, pythonIP, pythonPort);

            // Close immediately to free resources
            udpClient.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("UDP Send Error: " + e.Message);
        }
    }
}