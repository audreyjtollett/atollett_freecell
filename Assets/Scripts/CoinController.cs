using UnityEngine;

public class CoinController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            //GameObject.Find("Player")
            pc.coinsCollected++;
            //GetComponent<ParticleSystem>().Play(); // To get this to work, you must wait to destroy gameobject until after the particle system finishes
            Destroy(gameObject);
        }
    }
}
