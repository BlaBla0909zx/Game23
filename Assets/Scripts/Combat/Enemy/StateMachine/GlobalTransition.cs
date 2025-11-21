public class GlobalTransition
{
    public System.Func<bool> Condition;   
    public EnemyStateID TargetState;     

    public GlobalTransition(System.Func<bool> condition, EnemyStateID target)
    {
        Condition = condition;
        TargetState = target;
    }
}
