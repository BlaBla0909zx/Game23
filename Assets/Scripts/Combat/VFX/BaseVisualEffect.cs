using UnityEngine;
using UnityEngine.Pool;

public abstract class BaseVisualEffect : MonoBehaviour
{
    protected ObjectPool<BaseVisualEffect> pool;
    public VisualEffectID VfxID { get; protected set; }
    protected ParticleSystem ps;

    protected bool _isStopped = false;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }


    public abstract void Initialize();

    public virtual void Play(float duration = -1f)
    {
        if (_isStopped)
        {
            _isStopped = false;
            foreach (var p in GetComponentsInChildren<ParticleSystem>(true))
            {
                var emission = p.emission;
                emission.enabled = true;
                p.Play(true);
            }
        }

        float originalLifetime = GetParticleLifetime();

        if (duration < 0f) duration = originalLifetime;

        // Scale thời gian
        float scale = originalLifetime / duration;
        foreach (var p in GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = p.main;
            main.simulationSpeed = scale;
        }

        ReturnToPool(duration);
    }
    public void SetPool(ObjectPool<BaseVisualEffect> pool)
    {
        this.pool = pool;
    }
    public virtual void ReturnToPool()
    {
        pool?.Release(this);
    }

    public virtual void ReturnToPool(float duration)
    {
        Invoke(nameof(ReturnToPool), duration);
    }

    public virtual void SetParticleQuantity(int quantity = 5)
    {
        var main = ps.main;
        main.maxParticles = quantity * 2;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.burstCount = quantity; // emit 50 particles per second
    }

    public virtual void SetLifeTime(float lifeTime = 0.65f)
    {
        var main = ps.main;
        main.startLifetime = lifeTime;
    }

    public virtual void SetSpeed(float startSpeed = 3.5f, float endSpeed = 5.5f)
    {
        var main = ps.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed, endSpeed);
    }

    public void StopEmit()
    {
        foreach (var p in GetComponentsInChildren<ParticleSystem>(true))
        {
            p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            var emission = p.emission;
            emission.enabled = false;
        }

        _isStopped = true;
    }
    public float GetParticleLifetime()
    {
        float maxDuration = 0f;

        // Lấy tất cả ParticleSystem (bao gồm cả con)
        foreach (var p in GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = p.main;

            // thời gian chạy hệ thống + thời gian sống tối đa của hạt
            float duration = main.duration + main.startLifetime.constantMax;

            if (duration > maxDuration)
                maxDuration = duration;
        }

        return maxDuration;
    }

}