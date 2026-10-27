using UnityEngine;

public class BossHealth : EnemyHealth
{
    private BossController bossController;
    private bool isInvulnerable = false;

    public bool IsInvulnerable => isInvulnerable;

    public void Initialize(int startingMaxHp, BossController controller)
    {
        this.maxHealth = startingMaxHp;
        this.currentHealth = startingMaxHp;
        this.bossController = controller;
        this.expDropValue = controller != null && controller.BossConfig != null ? controller.BossConfig.expReward : 500;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetInvulnerable(bool invulnerable)
    {
        this.isInvulnerable = invulnerable;
    }

    public override void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || isInvulnerable) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (bossController != null)
        {
            bossController.OnTakeDamage(damage, currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        if (bossController != null)
        {
            bossController.OnBossKilled();
        }
        else
        {
            base.Die();
        }
    }
}
