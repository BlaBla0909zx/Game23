using System.Collections;
using UnityEngine;
public class MoveState : EnemyState
{
    public override EnemyStateID GetID() => EnemyStateID.Move;

    public override void Enter(EnemyBase enemy)
    {
        SetAnimationSpeed(1.0f, enemy.animator);
    }

    public override void Update(EnemyBase enemy)
    {
        if (enemy.IsInAttackRange())
        {
            enemy.ChangeState(EnemyStateID.Attack);
        }
        else
        {
            Vector3 direction = (enemy.playerStats.transform.position - enemy.transform.position).normalized;
            enemy.transform.position += direction * enemy.enemyData.moveSpeed * Time.deltaTime;
            enemy.transform.LookAt(new Vector3(enemy.playerStats.transform.position.x, enemy.transform.position.y, enemy.playerStats.transform.position.z));
        }
    }

    public override void Exit(EnemyBase enemy)
    {
    }

    public void SetAnimationSpeed(float speed, Animator animator)
    {
        animator.SetFloat("Speed", speed);
    }
  
}