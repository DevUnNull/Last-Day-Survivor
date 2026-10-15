using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Multipliers")]
    [SerializeField] private float moveSpeedMultiplier = 1f;
    [SerializeField] private float magicDamageMultiplier = 1f;
    [SerializeField] private float cooldownMultiplier = 1f; // Lower = faster
    [SerializeField] private float pickupRange = 3.5f;

    public float MoveSpeedMultiplier => moveSpeedMultiplier;
    public float MagicDamageMultiplier => magicDamageMultiplier;
    public float CooldownMultiplier => cooldownMultiplier;
    public float PickupRange => pickupRange;

    public event Action OnStatsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddDamageMultiplier(float amount)
    {
        magicDamageMultiplier += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddMoveSpeedMultiplier(float amount)
    {
        moveSpeedMultiplier += amount;
        OnStatsChanged?.Invoke();
    }

    public void ReduceCooldown(float percentReduction)
    {
        cooldownMultiplier *= (1f - percentReduction);
        if (cooldownMultiplier < 0.25f) cooldownMultiplier = 0.25f;
        OnStatsChanged?.Invoke();
    }

    public void AddPickupRange(float amount)
    {
        pickupRange += amount;
        OnStatsChanged?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
