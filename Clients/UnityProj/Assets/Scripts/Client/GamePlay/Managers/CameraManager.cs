using BiangStudio.Singleton;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public FieldCamera FieldCamera;
    public Camera MainCamera;
    public Camera BattleUICamera;
}