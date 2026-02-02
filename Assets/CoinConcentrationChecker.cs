using UnityEngine;
using TMPro;

public class CoinConcentrationChecker : MonoBehaviour
{
    [SerializeField] private float checkInterval = 5f; // seconds
    [SerializeField] private int coinThreshold = 10;
    [SerializeField] private TextMeshProUGUI alertText; // optional UI Text to show message

    private int coinsCollectedInPeriod = 0;
    private float timer = 0f;

    public bool isConcentrationGood { get; private set; } = false;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            CheckConcentration();
            coinsCollectedInPeriod = 0; // reset for next period
            timer = 0f;
        }
    }

    public void AddCoin(int amount = 1)
    {
        coinsCollectedInPeriod += amount;
    }

    private void CheckConcentration()
    {
        if (coinsCollectedInPeriod >= coinThreshold)
        {
            isConcentrationGood = true;
            if (alertText != null)
                alertText.text = "Concentration is on point!";
            Debug.Log("Concentration is on point!");
        }
        else
        {
            isConcentrationGood = false;
            if (alertText != null)
                alertText.text = "Concentration is poor!";
            Debug.Log("Concentration is poor!");
        }
    }
}