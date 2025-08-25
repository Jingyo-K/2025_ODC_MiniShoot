using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerExit : MonoBehaviour
{
    public bool canExit = false;
    public ExitZone exitZone;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<ExitZone>() != null)
        {
            canExit = true;
            exitZone = collision.GetComponent<ExitZone>();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<ExitZone>() != null)
        {
            canExit = false;
            exitZone = null;
        }
    }

    public void OnExitButton(CallbackContext context)
    {
        if (canExit && context.performed && exitZone != null)
        {
            if (GetComponent<ScrapSystem>() != null)
            {
                if (GetComponent<ScrapSystem>().scrapCount >= exitZone.exitScrap)
                {
                    GetComponent<ScrapSystem>().UseScrap(exitZone.exitScrap);
                    GameSceneController.Instance.NextStage();
                }
                else
                {
                    Debug.LogWarning("Not enough scrap to exit the room!");
                }
            }
        }
    }
}
