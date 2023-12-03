namespace FreedLOW.FireAtTergets.Code.Target
{
    public interface IMilitaryTarget
    {
        TargetShootPointType TargetShootPointType { get; }
        
        void Damage(int damageAmount);
    }
}