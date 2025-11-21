using UnityEngine;

public class AttackState : EnemyState
{
    public override EnemyStateID GetID() => EnemyStateID.Attack;
    private float _timer;

    public override void Enter(EnemyBase enemy)
    {
        _timer = 0f;

    }

    public override void Update(EnemyBase enemy)
    {
        _timer += Time.deltaTime;
        if (_timer >= enemy.enemyData.attackSpeed)
        {
            enemy.PlayeAttackAnimation();
            _timer -= enemy.enemyData.attackSpeed;
            if (!enemy.IsInAttackRange())
            {
                enemy.ChangeState(EnemyStateID.Move);
            }
        }
    }

    public override void Exit(EnemyBase enemy) { }
}