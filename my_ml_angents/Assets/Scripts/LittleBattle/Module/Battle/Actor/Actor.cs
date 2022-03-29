using System;
public class Actor
{


    #region 静态属性
    public string name => "Actor";
    public bool IsEntity { get; private set; }
    public int UUID { get; private set; }
    public int ActorId { get; private set; }
    public int ActorType { get; private set; }
    public int Layer { get; private set; }
    public int Tag { get; private set; }
    public int Faction { get; private set; }
    public int ComponentBits { get; private set; }
    #endregion

    public void Build(ActorConfig config)
    {
        
    }

    public void Start()
    {
        
    }
}
