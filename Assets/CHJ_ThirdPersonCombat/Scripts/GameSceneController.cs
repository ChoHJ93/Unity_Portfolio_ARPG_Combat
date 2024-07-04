using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] public CameraController cameraController;
    [SerializeField] public InputController inputController;
    [SerializeField] public Unit playerUnit;

    private void Awake()
    {
        GameManager.Instance.StartGame(this);
    }
}
