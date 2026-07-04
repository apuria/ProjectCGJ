using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill")]
public class SkillInfo : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    [TextArea(3, 10)]
    public string skillDescription;
    /// <summary>
    /// 伤害倍率（基于角色攻击力的倍率，如 1.5 表示 150% 攻击力）
    /// </summary>
    public float Damage;
    public int mpCost;
    public bool isAOE;

    public bool hasBuff;
    public BuffInfo buff;
    //TODO:
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
