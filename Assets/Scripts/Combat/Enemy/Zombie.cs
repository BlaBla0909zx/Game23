using UnityEngine;
using System.Collections;

public class Zombie : EnemyBase
{
    public override void Initialize(EnemySO data, PlayerStats playerStats_)
    {
        base.Initialize(data, playerStats_);
        AddState(new MoveState());
        AddState(new AttackState());
        AddState(new DieState());
        ChangeState(EnemyStateID.Move);
        
    }

    private IEnumerator DissolveEffect()
    {
        float dissolveDuration = 2f;
        float elapsed = 0f;
        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float dissolveAmount = Mathf.Clamp01(elapsed / dissolveDuration);
            if (_renderer.Count > 0)
            {
                foreach (var rend in _renderer)
                {
                    rend.material.SetFloat("_DissolveAmount", dissolveAmount);
                }
            }
            yield return null;
        }
    }
    public override void PlayDieEffect()
    {
        base.PlayDieEffect();
        StartCoroutine(DissolveEffect());

    }

    public override void OnDeath()
    {
        // Implement zombie-specific death behavior
    }
}