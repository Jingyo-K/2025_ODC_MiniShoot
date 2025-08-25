using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextStage : MonoBehaviour
{
    public void OnNextStageButtonClicked()
    {

        GameSceneController.Instance.StartGame();
    }
}
