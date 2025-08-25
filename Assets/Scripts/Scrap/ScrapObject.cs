using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapObject : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isGoToPlayer = false;
    private GameObject player;
    public int scrapAmount = 1; // Amount of scrap to collect
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameSceneController.Instance.playerInstance;
    }

    public void SetGoToPlayer(bool value)
    {
        isGoToPlayer = value;
        if (isGoToPlayer)
        {
            rb.velocity = Vector2.zero; // Stop any existing movement
            GetComponent<BoxCollider2D>().isTrigger = true; // Disable collider while moving
            StartCoroutine(MoveToPlayer());
        }
    }

    private IEnumerator MoveToPlayer()
    {
        while (isGoToPlayer)
        {
            Vector2 playerPosition = player.transform.position;
            Vector2 direction = (playerPosition - rb.position).normalized;
            rb.velocity = direction * 10f; // Move towards the player at a speed of 5 units per second

            // Check if close enough to stop
            if (Vector2.Distance(rb.position, playerPosition) < 0.1f)
            {
                rb.velocity = Vector2.zero; // Stop moving when close enough
                isGoToPlayer = false; // Stop the coroutine
            }

            yield return null; // Wait for the next frame
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<ScrapSystem>() != null)
        {
            Debug.Log($"Scrap collected by player! Amount: {scrapAmount}");
            GameSceneController.Instance.playerInstance.GetComponent<ScrapSystem>().CollectScrap(scrapAmount);
            Destroy(gameObject); // Destroy the scrap object after collection
        }
    }
}
