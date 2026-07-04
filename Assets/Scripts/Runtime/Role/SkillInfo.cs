using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能目标类型：对敌 or 对队友
/// </summary>
public enum SkillTargetType
{
    /// <summary>对敌（伤害）</summary>
    Enemy,
    /// <summary>对队友（治疗）</summary>
    Ally
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill")]
public class SkillInfo : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    [TextArea(3, 10)]
    public string skillDescription;
    /// <summary>
    /// 威力倍率（基于角色攻击力的倍率，如 1.5 表示 150% 攻击力）
    /// 对敌时为伤害倍率，对队友时为治疗倍率
    /// </summary>
    public float Damage;
    public int mpCost;
    public bool isAOE;
    /// <summary>
    /// 技能目标类型：对敌（伤害）或对队友（治疗）
    /// </summary>
    public SkillTargetType targetType = SkillTargetType.Enemy;

    public bool hasBuff;
    public BuffInfo buff;
    //
    /*
    1. 攻击特效
    2. 受击特效
    */
    public GameObject attackEffect;
    public GameObject hitEffect;

    private const float MinEffectWaitTime = 1.5f;

    /// <summary>
    /// 获取技能特效的最大播放时间，若无法获取则返回最低 1.2s
    /// </summary>
    public static float GetEffectTime(SkillInfo skill)
    {
        if (skill == null) return MinEffectWaitTime;

        float maxTime = 0f;
        if (skill.attackEffect != null)
        {
            var ps = skill.attackEffect.GetComponent<ParticleSystem>();
            if (ps != null) maxTime = Mathf.Max(maxTime, ps.main.duration);
        }
        if (skill.hitEffect != null)
        {
            var ps = skill.hitEffect.GetComponent<ParticleSystem>();
            if (ps != null) maxTime = Mathf.Max(maxTime, ps.main.duration);
        }
        return maxTime > 0f ? maxTime : MinEffectWaitTime;
    }
}
