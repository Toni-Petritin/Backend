using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementInput : MonoBehaviour
{
    [SerializeField] private GridLogic gridLogic;

    private void Update()
    {
        if(!UIHandler.InMenu)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                gridLogic.MovePlayer(0, 1);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                gridLogic.MovePlayer(0, -1);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gridLogic.MovePlayer(1, 0);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gridLogic.MovePlayer(-1, 0);
            }
        }
    }
}
