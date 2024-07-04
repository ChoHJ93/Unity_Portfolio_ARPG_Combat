using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>   
{
    private CameraController _cameraController;
    private InputController _inputController;
    private Unit _playerUnit;
    private bool _isGameStart = false;

    public CameraController CameraController => _cameraController;
    public InputController InputController => _inputController;
    public Unit PlayerUnit => _playerUnit;
    public bool IsGameStart => _isGameStart;

    public void StartGame(GameSceneController gameScene)
    {
        _cameraController = gameScene.cameraController;
        _inputController = gameScene.inputController;
        _playerUnit = gameScene.playerUnit;

        CheckForStartGame();

        _isGameStart = true;
    }

    private void CheckForStartGame() 
    {
        foreach (var camControl in FindObjectsOfType<CameraController>()) 
        {
            if(camControl != _cameraController) 
            {
                Destroy(camControl.gameObject);
            }
        }

        foreach (var inputControl in FindObjectsOfType<InputController>()) 
        {
            if(inputControl != _inputController) 
            {
                Destroy(inputControl.gameObject);
            }
        }

        foreach (var playerUnit in FindObjectsOfType<Unit>()) 
        {
            if(playerUnit != _playerUnit) 
            {
                Destroy(playerUnit.gameObject);
            }
        }
    }
}
