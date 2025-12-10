using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed;
    private Material material;
    private SpriteRenderer[] renderers;
    [HideInInspector] public bool isSetForDestruction = false;

    void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        if (isSetForDestruction)
        {
            speed = 0f;
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }
            float dissolveAmount = material.GetFloat("_Fade");
            dissolveAmount -= Time.deltaTime; // Increase dissolve amount over time
            material.SetFloat("_Fade", dissolveAmount);
            foreach (SpriteRenderer sr in renderers)
            {
                sr.material.SetFloat("_Fade", dissolveAmount);
            }
            if (dissolveAmount <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy_Barrier"))
        {
            speed = -speed;
            Vector3 scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && !isSetForDestruction)
        {
            Debug.Log("Player hit an enemy!");
            // if collider type is EdgeCollider2D
            // isSetForDestruction = true;
        }
    }
}
