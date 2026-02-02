using UnityEngine;
using TMPro;

public class CoinCollect : MonoBehaviour
{
    static private int coincount;
    public TextMeshProUGUI coinText;
    public AudioSource collectSound;
    public CoinConcentrationChecker checker;

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
        // Check if the thing that touched the coin is the player
        if (other.CompareTag("Player"))

        {

            // Play collection sound
            if (collectSound != null)
            {
                collectSound.Play();
            }

            if (checker)
            {
                checker.AddCoin(1);
            }

            // Destroy the coin
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
}
