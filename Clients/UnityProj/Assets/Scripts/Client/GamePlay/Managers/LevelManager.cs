using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BiangStudio.Singleton;
using MS.Framework.Serialize;
using UnityEngine;

public class LevelManager : TSingletonBaseManager<LevelManager>
{
    private SortedDictionary<int, RhythmRecord> RhythmRecordDict = new SortedDictionary<int, RhythmRecord>();
    private SortedDictionary<int, RhythmRecord> RhythmRecordDict_Record = new SortedDictionary<int, RhythmRecord>();

    private SortedDictionary<int, RhythmRecord> BrickAnimationRecordDict = new SortedDictionary<int, RhythmRecord>();

    private string ExportFilePath = Application.streamingAssetsPath + "/Record/Export.txt";
    private string AnimationExportFilePath = Application.streamingAssetsPath + "/Record/AnimationExport.txt";

    private string EasyFilePath = Application.streamingAssetsPath + "/Easy/Export.txt";
    private string EasyAnimationFilePath = Application.streamingAssetsPath + "/Easy/AnimationExport.txt";

    private string HardFilePath = Application.streamingAssetsPath + "/Hard/Export.txt";
    private string HardAnimationFilePath = Application.streamingAssetsPath + "/Hard/AnimationExport.txt";

    public SortedDictionary<int, List<BrickDance>> BrickDanceGroupDict = new SortedDictionary<int, List<BrickDance>>();
    public SortedDictionary<int, BrickDance> BrickDanceDict = new SortedDictionary<int, BrickDance>();

    public static float WaitTimeDuringHighlight = 1f;

    public override void Awake()
    {
        base.Awake();
        if (File.Exists(EasyFilePath))
        {
            RhythmRecordDict = SerializeUtility.FromFile<SortedDictionary<int, RhythmRecord>>(EasyFilePath);
            RhythmRecordDict_Record = SerializeUtility.FromFile<SortedDictionary<int, RhythmRecord>>(EasyFilePath);
        }

        if (File.Exists(EasyAnimationFilePath))
        {
            BrickAnimationRecordDict = SerializeUtility.FromFile<SortedDictionary<int, RhythmRecord>>(EasyAnimationFilePath);
        }
    }

    public void SwitchToHard()
    {
        if (File.Exists(HardFilePath))
        {
            RhythmRecordDict = SerializeUtility.FromFile<SortedDictionary<int, RhythmRecord>>(HardFilePath);
            RhythmRecordDict_Record = SerializeUtility.FromFile<SortedDictionary<int, RhythmRecord>>(HardFilePath);
        }

        if (File.Exists(HardAnimationFilePath))
        {
            BrickAnimationRecordDict = SerializeUtility.FromFile<SortedDictionary<int, RhythmRecord>>(HardAnimationFilePath);
        }
    }

    public override void Update(float deltaTime)
    {
        float curTime = Time.timeSinceLevelLoad;
        foreach (KeyValuePair<int, RhythmRecord> kv in RhythmRecordDict)
        {
            RhythmRecord.RhythmStep[] RSs = kv.Value.RhythmSteps.Values.ToArray();
            for (int index = 0; index < RSs.Length; index++)
            {
                RhythmRecord.RhythmStep rs = RSs[index];
                if (rs.TimePoint < curTime + WaitTimeDuringHighlight && rs.TimePoint > curTime - Time.deltaTime + WaitTimeDuringHighlight)
                {
                    float duration = WaitTimeDuringHighlight;
                    if (index < RSs.Length - 1)
                    {
                        duration = Mathf.Min(WaitTimeDuringHighlight, RSs[index + 1].TimePoint - rs.TimePoint);
                    }

                    if (kv.Value.BrickGroupIndex != 0)
                    {
                        if (BrickDanceGroupDict.TryGetValue(kv.Value.BrickGroupIndex, out List<BrickDance> bds))
                        {
                            foreach (BrickDance bd in bds)
                            {
                                bd.Highlight(duration);
                            }
                        }
                    }
                    else
                    {
                        if (BrickDanceDict.TryGetValue(kv.Key, out BrickDance bd))
                        {
                            bd.Highlight(duration);
                            break;
                        }
                    }
                }
            }
        }

        foreach (KeyValuePair<int, RhythmRecord> kv in BrickAnimationRecordDict) // animation
        {
            RhythmRecord.RhythmStep[] RSs = kv.Value.RhythmSteps.Values.ToArray();
            for (int index = 0; index < RSs.Length; index++)
            {
                RhythmRecord.RhythmStep rs = RSs[index];
                if (rs.TimePoint < curTime && rs.TimePoint > curTime - Time.deltaTime)
                {
                    if (kv.Value.BrickGroupIndex != 0)
                    {
                        if (BrickDanceGroupDict.TryGetValue(kv.Value.BrickGroupIndex, out List<BrickDance> bds))
                        {
                            foreach (BrickDance bd in bds)
                            {
                                bd.HitAccuracy = -1;
                                bd.Jump();
                            }
                        }
                    }
                    else
                    {
                        if (BrickDanceDict.TryGetValue(kv.Key, out BrickDance bd))
                        {
                            bd.HitAccuracy = -1;
                            bd.Jump();
                            break;
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0) ||
            (Input.GetKey(KeyCode.Space) && curTime <= ClientGameManager.Instance.FirstStepTime && curTime >= ClientGameManager.Instance.FirstStepTime - Time.deltaTime))
        {
            OnClick(curTime);
        }
    }

    private void OnClick(float curTime)
    {
        Ray ray = CameraManager.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerManager.Instance.LayerMask_Tower))
        {
            BrickDance bd = hit.collider.gameObject.GetComponent<BrickDance>();
            if (bd)
            {
                bool groupJump = true;
                if (Input.GetKey(KeyCode.Space)) // record mode
                {
                    groupJump = true;
                    RhythmRecord.RhythmStep rs = new RhythmRecord.RhythmStep();
                    rs.JumpDirection = bd.BrickDirection;
                    rs.JumpDistance = 1f;
                    rs.TimePoint = Time.timeSinceLevelLoad;
                    if (!RhythmRecordDict_Record.ContainsKey(bd.BrickIndex)) RhythmRecordDict_Record.Add(bd.BrickIndex, new RhythmRecord(bd.BrickIndex, bd.BrickGroupIndex));
                    RhythmRecordDict_Record[bd.BrickIndex].RhythmSteps.Add(rs.TimePoint, rs);
                }
                else if (Input.GetKey(KeyCode.R)) // animation record mode single animation
                {
                    groupJump = false;
                    RhythmRecord.RhythmStep rs = new RhythmRecord.RhythmStep();
                    rs.JumpDirection = bd.BrickDirection;
                    rs.JumpDistance = 1f;
                    rs.TimePoint = Time.timeSinceLevelLoad;
                    if (!BrickAnimationRecordDict.ContainsKey(bd.BrickIndex)) BrickAnimationRecordDict.Add(bd.BrickIndex, new RhythmRecord(bd.BrickIndex, 0));
                    BrickAnimationRecordDict[bd.BrickIndex].RhythmSteps.Add(rs.TimePoint, rs);
                }
                else if (Input.GetKey(KeyCode.T)) // animation record mode + group animation
                {
                    groupJump = true;
                    RhythmRecord.RhythmStep rs = new RhythmRecord.RhythmStep();
                    rs.JumpDirection = bd.BrickDirection;
                    rs.JumpDistance = 1f;
                    rs.TimePoint = Time.timeSinceLevelLoad;
                    if (!BrickAnimationRecordDict.ContainsKey(bd.BrickIndex)) BrickAnimationRecordDict.Add(bd.BrickIndex, new RhythmRecord(bd.BrickIndex, bd.BrickGroupIndex));
                    BrickAnimationRecordDict[bd.BrickIndex].RhythmSteps.Add(rs.TimePoint, rs);
                }
                else if (Input.GetKey(KeyCode.Q))
                {
                    groupJump = true;
                    foreach (KeyValuePair<int, RhythmRecord> kv in RhythmRecordDict)
                    {
                        if ((kv.Value.BrickGroupIndex != 0 && kv.Value.BrickGroupIndex == bd.BrickGroupIndex) || kv.Value.BrickIndex == bd.BrickIndex)
                        {
                            List<RhythmRecord.RhythmStep> removeList = new List<RhythmRecord.RhythmStep>();
                            foreach (KeyValuePair<float, RhythmRecord.RhythmStep> _kv in kv.Value.RhythmSteps)
                            {
                                if (_kv.Key > curTime && _kv.Key < curTime + WaitTimeDuringHighlight)
                                {
                                    removeList.Add(_kv.Value);
                                }
                            }

                            foreach (RhythmRecord.RhythmStep rs in removeList)
                            {
                                kv.Value.RhythmSteps.Remove(rs.TimePoint);
                                Debug.Log($"Remove {rs.TimePoint}");
                            }
                        }
                    }
                }

                if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.T) || bd.JumpState == JumpState.Highlighting)
                {
                    if (groupJump && BrickDanceGroupDict.TryGetValue(bd.BrickGroupIndex, out List<BrickDance> bds))
                    {
                        foreach (BrickDance _bd in bds)
                        {
                            _bd.Jump();
                        }
                    }
                    else
                    {
                        bd.Jump();
                    }
                }
            }
        }
    }

    public void SetBrickInterval(float interval)
    {
        foreach (KeyValuePair<int, BrickDance> kv in BrickDanceDict)
        {
            kv.Value.SetBrickInterval(interval);
        }
    }

    public void SetBrickBorderColor(Color color)
    {
        foreach (KeyValuePair<int, BrickDance> kv in BrickDanceDict)
        {
            kv.Value.SetBrickBorderColor(color);
        }
    }

    public void ExportRecord()
    {
        SerializeUtility.ToFile(RhythmRecordDict_Record, ExportFilePath);
        SerializeUtility.ToFile(BrickAnimationRecordDict, AnimationExportFilePath);
    }

    public override void ShutDown()
    {
        base.ShutDown();
        BrickDanceGroupDict.Clear();
        BrickDanceDict.Clear();
    }
}

[Serializable]
public class RhythmRecord
{
    public int BrickIndex;
    public int BrickGroupIndex;
    public SortedDictionary<float, RhythmStep> RhythmSteps = new SortedDictionary<float, RhythmStep>();

    [Serializable]
    public class RhythmStep
    {
        public float TimePoint;
        public Vector3 JumpDirection;
        public float JumpDistance;
    }

    public RhythmRecord()
    {
    }

    public RhythmRecord(int brickIndex, int brickGroupIndex)
    {
        BrickIndex = brickIndex;
        BrickGroupIndex = brickGroupIndex;
    }
}