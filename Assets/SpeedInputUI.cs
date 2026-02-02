using UnityEngine;
using UnityEngine.UI;

public class SpeedInputUI : MonoBehaviour
{
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Supercyan.FreeSample.SimpleSampleCharacterControl player;
    [SerializeField] private float maxSpeed = 10f;

    void Update()
    {
        float speed = speedSlider.value * maxSpeed;
        player.SetForwardSpeed(speed);
    }
}