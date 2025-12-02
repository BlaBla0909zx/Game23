using UnityEngine;

public class MoveState : EnemyState
{
    private float _currentSpeed;
    private const float ACCELERATION = 8.0f;   // cho nhỏ thôi để bạn nhìn rõ nó “lên từ từ”

    public override EnemyStateID GetID() => EnemyStateID.Move;

    public override void Enter(EnemyBase enemy)
    {
        _currentSpeed = 0f;
        enemy.animator.SetFloat("Speed", 0f);   // Animator param = 0
    }

    public override void Update(EnemyBase enemy)
    {
        if (enemy.IsInAttackRange())
        {
            enemy.ChangeState(EnemyStateID.Attack);
            return;
        }

        // Tăng tốc thật ngoài world: 0 -> 1.8
        _currentSpeed = Mathf.MoveTowards(
            _currentSpeed,
            enemy.enemyData.moveSpeed,          // 1.8
            ACCELERATION * Time.deltaTime
        );

        // Di chuyển
        Vector3 dir = (enemy.playerStats.transform.position - enemy.transform.position).normalized;
        enemy.transform.position += dir * _currentSpeed * Time.deltaTime;

        enemy.transform.LookAt(new Vector3(
            enemy.playerStats.transform.position.x,
            enemy.transform.position.y,
            enemy.playerStats.transform.position.z
        ));

        // Gửi giá trị đã CHUẨN HOÁ (0 -> 1) vào Animator
        float normalizedSpeed = _currentSpeed / enemy.enemyData.moveSpeed; // 0..1
        enemy.animator.SetFloat("Speed", normalizedSpeed);
    }

    public override void Exit(EnemyBase enemy) { }
}
