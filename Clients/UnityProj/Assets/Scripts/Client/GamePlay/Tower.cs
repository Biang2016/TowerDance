using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class Tower : MonoBehaviour
{
    public float BrickInterval;
    public Color BrickBorderColor;

    private BrickDance[] bds;

    void Awake()
    {
        GetAllBDs();
    }

    [Button("GetAllBDs")]
    private void GetAllBDs()
    {
        bds = GetComponentsInChildren<BrickDance>();
        foreach (BrickDance bd in bds)
        {
            bd.DefaultLocalPosition_Static = bd.transform.localPosition;
            bd.SetBrickInterval(0);
            if (bd.BrickGroupIndex != 0)
            {
                if (!LevelManager.Instance.BrickDanceGroupDict.ContainsKey(bd.BrickGroupIndex))
                {
                    LevelManager.Instance.BrickDanceGroupDict.Add(bd.BrickGroupIndex, new List<BrickDance>());
                }

                LevelManager.Instance.BrickDanceGroupDict[bd.BrickGroupIndex].Add(bd);
            }

            if (!LevelManager.Instance.BrickDanceDict.ContainsKey(bd.BrickIndex))
            {
                LevelManager.Instance.BrickDanceDict.Add(bd.BrickIndex, bd);
            }
        }
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            LevelManager.Instance.SetBrickInterval(BrickInterval);
            LevelManager.Instance.SetBrickBorderColor(BrickBorderColor);
        }
        else
        {
            foreach (BrickDance bd in bds)
            {
                bd.SetBrickInterval(BrickInterval);
                bd.SetBrickBorderColor(BrickBorderColor);
            }
        }
    }
}