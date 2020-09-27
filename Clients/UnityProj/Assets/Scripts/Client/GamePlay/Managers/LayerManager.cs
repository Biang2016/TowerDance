using BiangStudio.Singleton;
using UnityEngine;

public class LayerManager : TSingletonBaseManager<LayerManager>
{
    public int LayerMask_UI;
    public int LayerMask_Tower;

    public int Layer_UI;
    public int Layer_Tower;

    public override void Awake()
    {
        LayerMask_UI = LayerMask.GetMask("UI");
        LayerMask_Tower = LayerMask.GetMask("Tower");

        Layer_UI = LayerMask.NameToLayer("UI");
        Layer_Tower = LayerMask.NameToLayer("Tower");
    }
}