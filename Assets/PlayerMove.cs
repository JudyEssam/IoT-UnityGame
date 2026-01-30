using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Get keyboard input
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right keys
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down keys

        // Move the player
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        transform.position += move * speed * Time.deltaTime;
    }
}
