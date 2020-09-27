using System.Collections;
using System.Collections.Generic;
using BiangStudio;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

//[ExecuteInEditMode]
public class BrickDance : MonoBehaviour
{
    private static int GUIDGenerator = 0;

    private int GetGUID()
    {
        return GUIDGenerator++;
    }

    public int BrickIndex;
    public int BrickGroupIndex = 0;

    public MeshFilter MeshFilter;
    public Mesh MeshSource;
    public MeshRenderer MeshRenderer;
    public Vector3 BrickDirection;

    [LabelText("低准确特效")]
    public ProjectileType HitProjectileType_LowAccuracy;

    [LabelText("低准确特效尺寸")]
    public float HitProjectileScale_LowAccuracy;

    [LabelText("中准确特效")]
    public ProjectileType HitProjectileType_MidAccuracy;

    [LabelText("中准确特效尺寸")]
    public float HitProjectileScale_MidAccuracy;

    [LabelText("高准确特效")]
    public ProjectileType HitProjectileType_HighAccuracy;

    [LabelText("高准确特效尺寸")]
    public float HitProjectileScale_HighAccuracy;

    int[][] faceVerts = new int[6][];
    Vector3[][] faceVerts_OriPos = new Vector3[6][];

    internal Vector3 DefaultLocalPosition_Static;
    private Vector3 DefaultLocalPosition;

    public Color JumpColor = Color.red;
    public Gradient WaitColor;

    private MaterialPropertyBlock MPB;

    public JumpState JumpState;

    private Coroutine jumpCoroutine;
    private Coroutine highlightCoroutine;

    public float JumpDistance = 1f;

    internal float HitAccuracy = 0f;

    internal Color BorderColor;

    void Awake()
    {
        //BrickIndex = GetGUID();
        DefaultLocalPosition_Static = transform.localPosition;
        MPB = new MaterialPropertyBlock();
    }

    void Start()
    {
        CloneMesh();
        GetFaceVerts();
    }

    //void Update()
    //{
    //    MeshFilter = GetComponent<MeshFilter>();
    //    MeshRenderer = GetComponent<MeshRenderer>();
    //}

    private void CloneMesh()
    {
        MeshFilter.mesh = Instantiate(MeshSource);
    }

    private void GetFaceVerts()
    {
        SortedDictionary<float, List<int>> dict_Z = new SortedDictionary<float, List<int>>();
        SortedDictionary<float, List<int>> dict_Y = new SortedDictionary<float, List<int>>();
        SortedDictionary<float, List<int>> dict_X = new SortedDictionary<float, List<int>>();
        for (int index = 0; index < MeshFilter.mesh.vertices.Length; index++)
        {
            Vector3 meshVertex = MeshFilter.mesh.vertices[index];
            Vector3 normal = MeshFilter.mesh.normals[index];
            if (normal.x.Equals(0) && normal.y.Equals(0) && !normal.z.Equals(0))
            {
                if (!dict_Z.ContainsKey(meshVertex.z))
                {
                    dict_Z.Add(meshVertex.z, new List<int>());
                }

                dict_Z[meshVertex.z].Add(index);
            }

            if (normal.x.Equals(0) && !normal.y.Equals(0) && normal.z.Equals(0))
            {
                if (!dict_Y.ContainsKey(meshVertex.y))
                {
                    dict_Y.Add(meshVertex.y, new List<int>());
                }

                dict_Y[meshVertex.y].Add(index);
            }

            if (!normal.x.Equals(0) && normal.y.Equals(0) && normal.z.Equals(0))
            {
                if (!dict_X.ContainsKey(meshVertex.x))
                {
                    dict_X.Add(meshVertex.x, new List<int>());
                }

                dict_X[meshVertex.x].Add(index);
            }
        }

        int faceIndex = 0;
        foreach (KeyValuePair<float, List<int>> kv in dict_Z)
        {
            faceVerts[faceIndex] = kv.Value.ToArray();
            faceVerts_OriPos[faceIndex] = new Vector3[4];
            for (int index = 0; index < kv.Value.Count; index++)
            {
                int i = kv.Value[index];
                faceVerts_OriPos[faceIndex][index] = MeshFilter.mesh.vertices[i];
            }

            if (faceVerts[faceIndex].Length != 4)
            {
                Debug.LogError("面顶点数量不为4");
            }

            faceIndex++;
        }

        foreach (KeyValuePair<float, List<int>> kv in dict_Y)
        {
            faceVerts[faceIndex] = kv.Value.ToArray();
            faceVerts_OriPos[faceIndex] = new Vector3[4];
            for (int index = 0; index < kv.Value.Count; index++)
            {
                int i = kv.Value[index];
                faceVerts_OriPos[faceIndex][index] = MeshFilter.mesh.vertices[i];
            }

            if (faceVerts[faceIndex].Length != 4)
            {
                Debug.LogError("面顶点数量不为4");
            }

            faceIndex++;
        }

        foreach (KeyValuePair<float, List<int>> kv in dict_X)
        {
            faceVerts[faceIndex] = kv.Value.ToArray();
            faceVerts_OriPos[faceIndex] = new Vector3[4];
            for (int index = 0; index < kv.Value.Count; index++)
            {
                int i = kv.Value[index];
                faceVerts_OriPos[faceIndex][index] = MeshFilter.mesh.vertices[i];
            }

            if (faceVerts[faceIndex].Length != 4)
            {
                Debug.LogError("面顶点数量不为4");
            }

            faceIndex++;
        }
    }

    public void SetBrickInterval(float interval)
    {
        DefaultLocalPosition = DefaultLocalPosition_Static * (1f + interval);
        if (JumpState == JumpState.Static || JumpState == JumpState.Highlighting)
        {
            transform.localPosition = DefaultLocalPosition;
        }
    }

    public void SetBrickBorderColor(Color color)
    {
        BorderColor = color;
        if (!Application.isPlaying || (JumpState == JumpState.Static))
        {
            if (MPB == null) MPB = new MaterialPropertyBlock();
            MPB.SetColor("_BaseColor", BorderColor);
            MeshRenderer.SetPropertyBlock(MPB, 2);
        }
    }

    public void Jump()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            ShrinkMesh(1);
        }

        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            transform.DOPause();
            transform.localPosition = DefaultLocalPosition;
        }

        jumpCoroutine = StartCoroutine(Co_Jump(BrickDirection, JumpDistance, 0.1f, 0.3f));
    }

    IEnumerator Co_Jump(Vector3 direction, float jumpDistance, float jumpOutDuration, float returnDuration)
    {
        JumpState = JumpState.Jumping;
        MeshRenderer.GetPropertyBlock(MPB);
        MPB.SetColor("_BaseColor", JumpColor);
        MeshRenderer.SetPropertyBlock(MPB, 0);
        MPB.SetColor("_BaseColor", JumpColor);
        MeshRenderer.SetPropertyBlock(MPB, 1);
        MPB.SetColor("_BaseColor", JumpColor);
        MeshRenderer.SetPropertyBlock(MPB, 2);

        if (HitAccuracy < 0)
        {
            // pass
        }

        if (HitAccuracy < 0.5f)
        {
            ProjectileFlash flash = ProjectileManager.Instance.PlayProjectileFlash(HitProjectileType_LowAccuracy, transform.position + 0.25f * BrickDirection);
            flash.transform.localScale = HitProjectileScale_LowAccuracy * Vector3.one;
            flash.transform.rotation = Quaternion.Euler(0, 1, 0);
            flash.transform.parent = transform;
        }
        else if (HitAccuracy < 0.75f)
        {
            ProjectileFlash flash = ProjectileManager.Instance.PlayProjectileFlash(HitProjectileType_MidAccuracy, transform.position + 0.25f * BrickDirection);
            flash.transform.localScale = HitProjectileScale_MidAccuracy * Vector3.one;
            flash.transform.rotation = Quaternion.Euler(0, 1, 0);
            flash.transform.parent = transform;
        }
        else
        {
            ProjectileFlash flash = ProjectileManager.Instance.PlayProjectileFlash(HitProjectileType_HighAccuracy, transform.position + 0.25f * BrickDirection);
            flash.transform.localScale = HitProjectileScale_HighAccuracy * Vector3.one;
            flash.transform.rotation = Quaternion.Euler(0, 1, 0);
            flash.transform.parent = transform;
        }

        HitAccuracy = 0;

        transform.DOLocalMove(DefaultLocalPosition + direction * jumpDistance, jumpOutDuration);
        yield return new WaitForSeconds(jumpOutDuration);
        JumpState = JumpState.Returning;
        transform.DOLocalMove(DefaultLocalPosition, returnDuration);

        float durationTick = 0f;
        while (durationTick < returnDuration)
        {
            durationTick += Time.deltaTime;
            MPB.SetColor("_BaseColor", Color.Lerp(JumpColor, Color.black, durationTick / returnDuration));
            MeshRenderer.SetPropertyBlock(MPB, 0);
            MPB.SetColor("_BaseColor", Color.Lerp(JumpColor, Color.white, durationTick / returnDuration));
            MeshRenderer.SetPropertyBlock(MPB, 1);
            MPB.SetColor("_BaseColor", Color.Lerp(JumpColor, BorderColor, durationTick / returnDuration));
            MeshRenderer.SetPropertyBlock(MPB, 2);
            yield return null;
        }

        JumpState = JumpState.Static;
    }

    public void Highlight(float duration)
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            ShrinkMesh(0);
        }

        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            transform.DOPause();
            transform.localPosition = DefaultLocalPosition;
        }

        highlightCoroutine = StartCoroutine(Co_Highlight(duration));
    }

    IEnumerator Co_Highlight(float waitDuration)
    {
        JumpState = JumpState.Highlighting;

        float durationTick = 0f;
        while (durationTick < waitDuration)
        {
            HitAccuracy = durationTick / waitDuration;
            durationTick += Time.deltaTime;
            MPB.SetColor("_BaseColor", WaitColor.Evaluate(durationTick / waitDuration));
            MeshRenderer.SetPropertyBlock(MPB, 0);
            MPB.SetColor("_BaseColor", WaitColor.Evaluate(durationTick / waitDuration));
            MeshRenderer.SetPropertyBlock(MPB, 1);
            MPB.SetColor("_BaseColor", WaitColor.Evaluate(durationTick / waitDuration));
            MeshRenderer.SetPropertyBlock(MPB, 2);
            ShrinkMesh(Mathf.Min(1, durationTick / waitDuration + 0.25f));
            yield return null;
        }

        MPB.SetColor("_BaseColor", Color.black);
        MeshRenderer.SetPropertyBlock(MPB, 0);
        MPB.SetColor("_BaseColor", Color.white);
        MeshRenderer.SetPropertyBlock(MPB, 1);
        MPB.SetColor("_BaseColor", BorderColor);
        MeshRenderer.SetPropertyBlock(MPB, 2);

        yield return new WaitForSeconds(0.15f);
        JumpState = JumpState.Static;
        LevelManager.Instance.AddHitQuality(HitQuality.Miss);
    }

    private void ShrinkMesh(float scale)
    {
        Vector3[] vertices = MeshFilter.mesh.vertices;
        for (int i = 0; i < faceVerts.Length; i++)
        {
            int[] faceIndices = faceVerts[i];
            Vector3 faceCenter = Vector3.zero;
            foreach (int index in faceIndices)
            {
                faceCenter += vertices[index];
            }

            faceCenter /= faceIndices.Length;
            for (int index1 = 0; index1 < faceIndices.Length; index1++)
            {
                int index = faceIndices[index1];
                vertices[index] = faceCenter + (faceVerts_OriPos[i][index1] - faceCenter) * scale;
            }
        }

        MeshFilter.mesh.vertices = vertices;
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Handles.Label(transform.position, BrickGroupIndex.ToString());
    }
#endif
    //#if UNITY_EDITOR
    //    void OnDrawGizmos()
    //    {
    //        Handles.matrix = transform.localToWorldMatrix;
    //        List<(int, Vector3)> vertexList = new List<(int, Vector3)>();
    //        for (int index = 0; index < MeshFilter.sharedMesh.vertices.Length; index++)
    //        {
    //            Vector3 normal = MeshFilter.sharedMesh.normals[index];
    //            //if (
    //            //    (normal.x.Equals(0) && normal.y.Equals(0) && !normal.z.Equals(0)) ||
    //            //    (!normal.x.Equals(0) && normal.y.Equals(0) && normal.z.Equals(0)) ||
    //            //    (normal.x.Equals(0) && !normal.y.Equals(0) && normal.z.Equals(0))
    //            //)
    //            //{
    //            Vector3 point = MeshFilter.sharedMesh.vertices[index];
    //            vertexList.Add((index, point));
    //            //}
    //        }

    //        foreach ((int, Vector3) valueTuple in vertexList)
    //        {
    //            Handles.Label(valueTuple.Item2, valueTuple.Item1.ToString());
    //        }
    //    }
    //#endif
}

public enum BrickDirection
{
    Static = 0,
    X_Positive = 1,
    X_Negative = 2,
    Y_Positive = 3,
    Y_Negative = 4,
    Z_Positive = 5,
    Z_Negative = 6,
}

public enum JumpState
{
    Static,
    Highlighting,
    Jumping,
    Returning,
}

public enum HitQuality
{
    Miss,
    Good,
    Great,
    Perfect
}