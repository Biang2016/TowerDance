using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class FieldCamera : MonoBehaviour
{
    public Camera Camera;
    public Transform CameraTarget;

    [SerializeField]
    private List<Transform> targetList = new List<Transform>();

    [LabelText("水平拖拽速度")]
    public float HorizontalDragSensitivity;

    [LabelText("竖直拖拽速度")]
    public float VerticalDragSensitivity;

    [LabelText("水平角")]
    public float HorizontalAngle;

    [LabelText("竖直角")]
    public float VerticalAngle;

    [LabelText("距离")]
    public float Distance;

    [LabelText("相机震屏力度曲线(x伤害y强度")]
    public AnimationCurve CameraShakeStrengthCurve;

    void Start()
    {
        AddTarget(CameraTarget);
        InitFocus();
    }

    public void InitFocus()
    {
        CameraLerp(false);
    }

    private void AddTarget(Transform transform)
    {
        targetList.Add(transform);
    }

    public float SmoothTime = 0.05f;
    Vector3 curVelocity = Vector3.zero;

    private Vector3 LastMousePosition;

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            LastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 mouseDiff = Input.mousePosition - LastMousePosition;
            HorizontalAngle += mouseDiff.x * HorizontalDragSensitivity;
            VerticalAngle += mouseDiff.y * VerticalDragSensitivity;
            VerticalAngle = Mathf.Clamp(VerticalAngle, -89.5f, 89.5f);
        }

        CameraLerp(true);
        LastMousePosition = Input.mousePosition;
    }

    private void CameraLerp(bool lerp)
    {
        Vector3 targetCenter = Vector3.zero;
        foreach (Transform trans in targetList)
        {
            targetCenter += trans.position;
        }

        if (targetList.Count != 0)
        {
            targetCenter /= targetList.Count;
        }

        Vector3 diff = Vector3.zero;
        diff.x = Distance * Mathf.Cos(VerticalAngle * Mathf.Deg2Rad) * Mathf.Sin(HorizontalAngle * Mathf.Deg2Rad);
        diff.y = Distance * Mathf.Sin(VerticalAngle * Mathf.Deg2Rad);
        diff.z = -Distance * Mathf.Cos(VerticalAngle * Mathf.Deg2Rad) * Mathf.Cos(HorizontalAngle * Mathf.Deg2Rad);
        Vector3 destination = targetCenter + diff;
        if (lerp)
        {
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref curVelocity, SmoothTime);
        }
        else
        {
            transform.position = destination;
        }

        transform.forward = targetCenter - destination;
    }

    public void CameraShake(int damage)
    {
        Camera.transform.DOShakePosition(0.1f, CameraShakeStrengthCurve.Evaluate(damage), 10, 90f);
    }
}