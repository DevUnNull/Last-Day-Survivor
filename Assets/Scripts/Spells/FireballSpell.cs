using System.Collections.Generic;
using UnityEngine;

public class FireballSpell : MonoBehaviour
{
    [Header("Spell Properties")]
    [SerializeField] private int currentLevel = 0; // 0 = not unlocked
    [SerializeField] private float baseCooldown = 2.0f;
    [SerializeField] private int baseDamage = 45;
    [SerializeField] private float baseRadius = 2.0f;
    [SerializeField] private float projectileSpeed = 11f;
    [SerializeField] private float targetingRange = 12f;
    [SerializeField] private int fireballCount = 1;

    public int CurrentLevel => currentLevel;
    public int MaxLevel => 5;

    private float nextCastTime;

    public void UnlockOrUpgrade()
    {
        if (currentLevel >= MaxLevel) return;
        currentLevel++;

        switch (currentLevel)
        {
            case 1:
                baseCooldown = 2.0f;
                baseDamage = 45;
                baseRadius = 2.0f;
                fireballCount = 1;
                break;
            case 2:
                baseDamage = 65;
                baseRadius = 2.6f;
                break;
            case 3:
                fireballCount = 2;
                break;
            case 4:
                baseCooldown = 1.35f;
                baseDamage = 80;
                break;
            case 5:
                fireballCount = 3;
                baseDamage = 110;
                baseRadius = 3.5f;
                break;
        }
    }

    private void Update()
    {
        if (currentLevel <= 0) return;

        if (Time.time >= nextCastTime)
        {
            CastFireball();
            float cdMod = PlayerStats.Instance != null ? PlayerStats.Instance.CooldownMultiplier : 1f;
            nextCastTime = Time.time + (baseCooldown * cdMod);
        }
    }

    private void CastFireball()
    {
        List<Transform> targets = FindEnemies(fireballCount);
        if (targets.Count == 0) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFireballLaunch();
        }

        float dmgMod = PlayerStats.Instance != null ? PlayerStats.Instance.MagicDamageMultiplier : 1f;
        int finalDamage = Mathf.RoundToInt(baseDamage * dmgMod);

        for (int i = 0; i < targets.Count; i++)
        {
            Transform target = targets[i];
            if (target == null) continue;

            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            FireballProjectile p = FireballProjectile.Create(transform.position);
            p.Setup(dir, projectileSpeed, finalDamage, baseRadius);
        }
    }

    private List<Transform> FindEnemies(int count)
    {
        List<Transform> list = new List<Transform>();
        EnemyHealth[] all = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        if (all == null || all.Length == 0) return list;

        // Sort by distance
        var sorted = new List<EnemyHealth>();
        foreach (var e in all)
        {
            if (e != null && e.gameObject.activeInHierarchy)
            {
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d <= targetingRange) sorted.Add(e);
            }
        }

        sorted.Sort((a, b) =>
        {
            float da = Vector2.Distance(transform.position, a.transform.position);
            float db = Vector2.Distance(transform.position, b.transform.position);
            return da.CompareTo(db);
        });

        for (int i = 0; i < Mathf.Min(count, sorted.Count); i++)
        {
            list.Add(sorted[i].transform);
        }

        // If fewer unique targets than count, can target the closest again
        while (list.Count < count && sorted.Count > 0)
        {
            list.Add(sorted[0].transform);
        }

        return list;
    }
}
