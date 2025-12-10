using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class TilemapController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float moveSpeed = 5f;

    GameObject[] boxes;
    GameObject[] goals;
    public Tilemap collisions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMove(InputValue inputValue)
    {
        Vector2 movementVector = inputValue.Get<Vector2>();
        //rb.linearVelocity = movementVector * moveSpeed;
        if (isValidMove(movementVector))
        {
            transform.position += (Vector3)movementVector;
        }
        
    }

    private bool isValidMove(Vector2 movementVector)
    {
        if (collisions.HasTile(collisions.WorldToCell(transform.position + (Vector3)movementVector)))
        {
            return false; // Invalid move
        }

        return true; // Valid move
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collided with " + collision.gameObject.name);    
    }
}
