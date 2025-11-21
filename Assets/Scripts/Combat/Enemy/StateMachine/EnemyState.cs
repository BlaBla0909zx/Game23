public abstract class EnemyState
{
    public abstract EnemyStateID GetID();
    public abstract void Enter(EnemyBase enemy);
    public abstract void Update(EnemyBase enemy);
    public abstract void Exit(EnemyBase enemy);
}