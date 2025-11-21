using UnityEngine;
public class DieState : EnemyState
{
    public override EnemyStateID GetID() => EnemyStateID.Die;

    public override void Enter(EnemyBase enemy)
    {
        enemy.animator.SetTrigger("Dead");
        enemy.UnregisterHealthBar();
        ObjectSpawner.Instance.SpawnCoin(enemy.transform.position + Vector3.up, enemy.enemyData.coinDrop);

    }

    public override void Update(EnemyBase enemy) { }
    public override void Exit(EnemyBase enemy) { }
}