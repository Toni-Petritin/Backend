using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{

    public static bool InMenu { get; private set; } = false;
    [SerializeField] private GameObject gameMenu;
    [SerializeField] private TextMeshProUGUI score;

    private void Update()
    {
        // gameMenu can be opened only if you are online.
        if (Input.GetKeyDown(KeyCode.Escape) && restClient.loggedIn)
        {
            InMenu = !InMenu;
            gameMenu.SetActive(!gameMenu.activeSelf);
        }

        // Update score on UI.
        if (restClient.loggedIn || restClient.offlinePlay)
        {
            score.text = "Score: " + GridLogic.PlayerScore;
        }
    }

}
