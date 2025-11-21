using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StarterAssets;
public class PlayerControl : MonoBehaviour
{
    [Space]
    [Header("Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private ThirdPersonController thirdPersonController;
    // [SerializeField] private GameControl gameControl;

    [Space]
    [Header("Combat")]
    public Transform target;
    [SerializeField] private Transform attackPos;
    [Tooltip("Offset Stoping Distance")][SerializeField] private float quickAttackDeltaDistance;
    [Tooltip("Offset Stoping Distance")][SerializeField] private float heavyAttackDeltaDistance;
    [SerializeField] private bool useKnockback = true;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float airknockbackForce = 10f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float reachTime = 0.3f;
    [SerializeField] private LayerMask enemyLayer;
    bool isAttacking = false;
    private PlayerStats playerStats;

    [Space]
    [Header("Debug")]
    [SerializeField] private bool debug;


    //key input



    // Start is called before the first frame update

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        InputManager.Instance.RegisterKeyAction(KeyAction.LightAttack, () => Attack(0));
        InputManager.Instance.RegisterKeyAction(KeyAction.HeavyAttack, () => Attack(1));
    }


    // Update is called once per frame


    #region Attack, PerformAttack, Reset Attack, Change Target


    public void Attack(int attackState)
    {
        if (isAttacking)
        {
            return;
        }
      
        thirdPersonController.canMove = false;
        TargetDetectionControl.instance.canChangeTarget = false;
        RandomAttackAnim(attackState);

    }

    private void RandomAttackAnim(int attackState)
    {


        switch (attackState)
        {
            case 0: //Quick Attack

                QuickAttack();
                break;

            case 1:
                HeavyAttack();
                break;

        }



    }

    void QuickAttack()
    {
        int attackIndex = Random.Range(1, 4);
        switch (attackIndex)
        {
            case 1: //punch

                if (target != null)
                {
                    MoveTowardsTarget(target.position, quickAttackDeltaDistance, "punch");
                    isAttacking = true;
                    // Play punch attack sound
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlaySFX("Attack1", volumeMultiplier: 0.8f, pitchMultiplier: 1.0f);
                }
                else
                {
                    thirdPersonController.canMove = true;
                    TargetDetectionControl.instance.canChangeTarget = true;
                }

                break;

            case 2: //kick

                if (target != null)
                {
                    MoveTowardsTarget(target.position, quickAttackDeltaDistance, "kick");
                    isAttacking = true;
                    // Play kick attack sound
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlaySFX("Attack1", volumeMultiplier: 0.9f, pitchMultiplier: 1.1f);
                }
                else
                {
                    thirdPersonController.canMove = true;
                    TargetDetectionControl.instance.canChangeTarget = true;
                }


                break;

            case 3: //mmakick

                if (target != null)
                {
                    MoveTowardsTarget(target.position, quickAttackDeltaDistance, "mmakick");

                    isAttacking = true;
                    // Play MMA kick attack sound
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlaySFX("Attack1", volumeMultiplier: 1.0f, pitchMultiplier: 0.9f);
                }
                else
                {
                    thirdPersonController.canMove = true;
                    TargetDetectionControl.instance.canChangeTarget = true;
                }


                break;
        }
    }

    void HeavyAttack()
    {
        int attackIndex = Random.Range(1, 3);

        switch (attackIndex)
        {
            case 1: //heavyAttack1

                if (target != null)
                {
                    //MoveTowardsTarget(target.position, kickDeltaDistance, "heavyAttack1");
                    FaceThis(target.position);
                    anim.SetBool("heavyAttack1", true);
                    isAttacking = true;
                    // Play heavy attack 1 sound
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlaySFX("Attack2", volumeMultiplier: 1.0f, pitchMultiplier: 0.8f);

                }
                else
                {
                    TargetDetectionControl.instance.canChangeTarget = true;
                    thirdPersonController.canMove = true;
                }


                break;

            case 2: //heavyAttack2

                if (target != null)
                {
                    //MoveTowardsTarget(target.position, kickDeltaDistance, "heavyAttack2");
                    FaceThis(target.position);
                    anim.SetBool("heavyAttack2", true);
                    isAttacking = true;
                    // Play heavy attack 2 sound
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlaySFX("Attack2", volumeMultiplier: 1.1f, pitchMultiplier: 0.9f);
                }
                else
                {
                    thirdPersonController.canMove = true;
                    TargetDetectionControl.instance.canChangeTarget = true;
                }

                break;
        }
    }

    public void ResetAttack() // Animation Event ---- for Reset Attack
    {
        anim.SetBool("punch", false);
        anim.SetBool("kick", false);
        anim.SetBool("mmakick", false);
        anim.SetBool("heavyAttack1", false);
        anim.SetBool("heavyAttack2", false);
        thirdPersonController.canMove = true;
        TargetDetectionControl.instance.canChangeTarget = true;
        isAttacking = false;
    }

    public void PerformAttack() // Animation Event ---- for Attacking Targets
    {
        // Assuming we have a melee attack with a short range

        Collider[] hitEnemies = Physics.OverlapSphere(attackPos.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyRb != null)
            {
                Vector3 knockbackDirection = enemy.transform.position - transform.position;
                if (useKnockback)
                {
                    knockbackDirection.y = airknockbackForce; // Keep the knockback horizontal
                    enemyRb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode.Impulse);
                }
                else
                {

                }
                enemyBase.TakeDamage(playerStats.DealDamage(), enemyBase.transform.position);
                var vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Hit_1);
                vfx.transform.position = attackPos.position;
                vfx.Play();
                
                // Play hit sound effect
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("Hit", volumeMultiplier: 0.7f, pitchMultiplier: Random.Range(0.9f, 1.1f));
            }
        }
    }

    private EnemyBase oldTarget;
    private EnemyBase currentTarget;

    public void ChangeTarget(Transform target_)
    {

        target = target_;
        if (target_ == null)
        {
            oldTarget = null;
            currentTarget = null;
            return;
        }
        oldTarget = target_.GetComponent<EnemyBase>();
        currentTarget = target_.GetComponent<EnemyBase>();
    }


    #endregion


    #region MoveTowards, Target Offset and FaceThis
    public void MoveTowardsTarget(Vector3 target_, float deltaDistance, string animationName_)
    {

        PerformAttackAnimation(animationName_);
        FaceThis(target_);
        Vector3 finalPos = TargetOffset(target_, deltaDistance);
        finalPos.y = 0;
        transform.DOMove(finalPos, reachTime);

    }

    public void GetClose() // Animation Event ---- for Moving Close to Target
    {
        Vector3 getCloseTarget;
        if (target == null)
        {
            //check null old target
            if (oldTarget == null) return;
            getCloseTarget = oldTarget.transform.position;
        }
        else
        {
            getCloseTarget = target.position;
        }
        FaceThis(getCloseTarget);
        Vector3 finalPos = TargetOffset(getCloseTarget, 1.4f);
        finalPos.y = 0;
        transform.DOMove(finalPos, 0.2f);
    }

    void PerformAttackAnimation(string animationName_)
    {
        anim.SetBool(animationName_, true);
    }

    public Vector3 TargetOffset(Vector3 target, float deltaDistance)
    {
        Vector3 position;
        position = target;
        return Vector3.MoveTowards(position, transform.position, deltaDistance);
    }

    public void FaceThis(Vector3 target)
    {
        Vector3 target_ = new Vector3(target.x, target.y, target.z);
        Quaternion lookAtRotation = Quaternion.LookRotation(target_ - transform.position);
        lookAtRotation.x = 0;
        lookAtRotation.z = 0;
        transform.DOLocalRotateQuaternion(lookAtRotation, 0.2f);
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange); // Visualize the attack range
    }
}
