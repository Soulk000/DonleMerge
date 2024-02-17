using System.Collections;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public GameManager gameManager;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Dongle")
        {
            gameManager.GameOver();
        }
    }
}
