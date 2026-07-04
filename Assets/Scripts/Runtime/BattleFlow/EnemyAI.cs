using System.Collections.Generic;
using UnityEngine;

public class EnemyAI
{
    public EnemyActionType actionType;
    public int targetIndex;

    // 最低等待时间
    private const float MinWaitTime = 1.5f;

    // 技能使用概率追踪（所有敌人共享）
    private static int turnsSinceSkill = 0;
    private const float SkillBaseChance = 0.15f;
    private const float SkillGrowthPerTurn = 0.1f;

    /// <summary>
    /// 重置技能概率（新战斗开始时调用）
    /// </summary>
    public static void Reset()
    {
        turnsSinceSkill = 0;
    }

    /// <summary>
    /// 执行敌方回合：决策 + 执行 + 结束
    /// </summary>
    public static EnemyAI DoTurn(EnemyBattleInfo info)
    {
        EnemyAI ai = new EnemyAI();

        if (info.enemyInfo == null || info.roleList == null)
        {
            BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
            return ai;
        }

        // 自身已死亡则跳过
        if (info.enemyInfo.hp.value <= 0)
        {
            BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
            return ai;
        }

        // 获取存活角色列表
        List<int> livingRoleIndices = new List<int>();
        for (int i = 0; i < info.roleList.Count; i++)
        {
            if (info.roleList[i] != null && info.roleList[i].hp.value > 0)
                livingRoleIndices.Add(i);
        }

        if (livingRoleIndices.Count == 0)
        {
            BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
            return ai;
        }

        // 随机选择一个存活角色作为目标
        ai.targetIndex = livingRoleIndices[Random.Range(0, livingRoleIndices.Count)];

        // 决策行动类型
        ai.actionType = DecideAction(info);

        // 执行行动（传入 roleList/enemyList 用于 AOE 和防御回血定位），返回技能特效最大播放时间
        float effectTime = ExecuteAction(info.enemyInfo, ai, info.roleList, info.enemyList);

        // 发送敌人行动提示事件
        string enemyName = info.enemyInfo != null ? info.enemyInfo.Name : "???";
        BattleEventDefine.EnemyActionToast.SendEventMessage(enemyName, ai.actionType);

        // 等待特效播放后再进入下一回合
        BattleEventDefine.EnemyActionDelay.SendEventMessage(effectTime);

        return ai;
    }

    /// <summary>
    /// 决策行动类型
    /// </summary>
    private static EnemyActionType DecideAction(EnemyBattleInfo info)
    {
        // 1~2回合内：只攻击或防御
        if (info.round <= 2)
        {
            return Random.value < 0.7f ? EnemyActionType.Attack : EnemyActionType.Defence;
        }

        // 2回合后：技能概率逐渐增加
        float skillChance = SkillBaseChance + turnsSinceSkill * SkillGrowthPerTurn;
        float roll = Random.value;

        if (info.enemyInfo.skills != null && info.enemyInfo.skills.Count > 0 && roll < skillChance)
        {
            // 使用技能 → 重置概率
            turnsSinceSkill = 0;
            return EnemyActionType.Skill;
        }

        // 未触发技能 → 攻击或防御，技能概率累积
        turnsSinceSkill++;
        return Random.value < 0.5f ? EnemyActionType.Attack : EnemyActionType.Defence;
    }

    /// <summary>
    /// 执行行动，返回特效最长播放时间
    /// </summary>
    private static float ExecuteAction(EnemyInfo enemy, EnemyAI ai, List<RoleInfo> roleList, List<EnemyInfo> enemyList)
    {
        switch (ai.actionType)
        {
            case EnemyActionType.Attack:
                DoAttack(enemy, ai.targetIndex, enemyList);
                return SkillInfo.GetEffectTime(enemy.normalAttack);

            case EnemyActionType.Skill:
                return DoSkill(enemy, ai.targetIndex, roleList, enemyList);

            case EnemyActionType.Defence:
                return DoDefence(enemy, enemyList);
        }
        return MinWaitTime;
    }

    /// <summary>
    /// 普通攻击（伤害 = 敌人攻击力 × 普攻倍率）
    /// </summary>
    private static void DoAttack(EnemyInfo enemy, int targetIdx, List<EnemyInfo> enemyList)
    {
        // 播放普攻音效（配置了才播放）
        PlaySkillSFX(enemy.normalAttack);

        int damage = enemy.normalAttack != null
            ? Mathf.RoundToInt(enemy.attack.value * enemy.normalAttack.Damage)
            : Mathf.RoundToInt(enemy.attack.value * 1f);

        int enemyIdx = enemyList != null ? enemyList.IndexOf(enemy) : 0;
        GameObject atkEff = enemy.normalAttack != null ? enemy.normalAttack.attackEffect : null;
        GameObject hitEff = enemy.normalAttack != null ? enemy.normalAttack.hitEffect : null;
        BattleEventDefine.RoleHpChange.SendEventMessage(targetIdx, damage,
            attackerIdx: enemyIdx,
            attackEffect: atkEff,
            hitEffect: hitEff);
    }

    /// <summary>
    /// 防御：恢复 5%~10% MP + 获得临时护盾（不再回复 HP）
    /// </summary>
    private static float DoDefence(EnemyInfo enemy, List<EnemyInfo> enemyList)
    {
        // 临时护盾（当前回合有效）
        enemy.tempDefense = enemy.shieldValue.value;

        // 随机恢复 5%~10% MP，最少恢复 1 点
        float mpRatio = Random.Range(0.05f, 0.10f);
        int mpRecover = Mathf.Max(1, (int)(mpRatio * enemy.maxMp.value));
        enemy.mp.value = Mathf.Min(enemy.mp.value + mpRecover, enemy.maxMp.value);

        if (enemyList != null)
        {
            int enemyIdx = enemyList.IndexOf(enemy);
            if (enemyIdx >= 0)
            {
                // 发送防御特效事件（在敌人身上播放防御VFX）
                BattleEventDefine.DefendEffect.SendEventMessage(false, enemyIdx);
            }
        }

        return MinWaitTime;
    }

    /// <summary>
    /// 使用技能（随机选一个技能，AOE 打全体/奶全体，敌人不耗蓝）
    /// 返回特效最长播放时间
    /// </summary>
    private static float DoSkill(EnemyInfo enemy, int targetIdx, List<RoleInfo> roleList, List<EnemyInfo> enemyList)
    {
        if (enemy.skills == null || enemy.skills.Count == 0)
        {
            DoAttack(enemy, targetIdx, enemyList);
            return MinWaitTime;
        }

        SkillInfo skill = enemy.skills[Random.Range(0, enemy.skills.Count)];

        // 播放技能音效（配置了才播放）
        PlaySkillSFX(skill);

        int enemyIdx = enemyList != null ? enemyList.IndexOf(enemy) : 0;

        // 根据技能目标类型分支处理
        if (skill.targetType == SkillTargetType.Ally)
        {
            // 对队友技能：治疗敌人队友
            return DoSkillHealAlly(enemy, skill, enemyList);
        }
        else
        {
            // 对敌技能：攻击玩家角色
            if (skill.isAOE && roleList != null)
            {
                int damage = Mathf.RoundToInt(enemy.attack.value * skill.Damage);
                for (int i = 0; i < roleList.Count; i++)
                {
                    if (roleList[i] != null && roleList[i].hp.value > 0)
                    {
                        BattleEventDefine.RoleHpChange.SendEventMessage(i, damage,
                            attackerIdx: enemyIdx,
                            attackEffect: skill.attackEffect,
                            hitEffect: skill.hitEffect);
                    }
                }
            }
            else
            {
                int damage = Mathf.RoundToInt(enemy.attack.value * skill.Damage);
                BattleEventDefine.RoleHpChange.SendEventMessage(targetIdx, damage,
                    attackerIdx: enemyIdx,
                    attackEffect: skill.attackEffect,
                    hitEffect: skill.hitEffect);
            }
        }

        return SkillInfo.GetEffectTime(skill);
    }

    /// <summary>
    /// 敌人对队友使用治疗技能
    /// 单体：随机选择一个存活敌人队友治疗；AOE：治疗所有存活敌人队友
    /// </summary>
    private static float DoSkillHealAlly(EnemyInfo caster, SkillInfo skill, List<EnemyInfo> enemyList)
    {
        int casterIdx = enemyList != null ? enemyList.IndexOf(caster) : 0;
        int healValue = Mathf.RoundToInt(caster.attack.value * skill.Damage);

        // 收集存活敌人队友（排除自身）
        List<int> livingAllyIndices = new List<int>();
        if (enemyList != null)
        {
            for (int i = 0; i < enemyList.Count; i++)
            {
                if (enemyList[i] != null && enemyList[i].hp.value > 0 && i != casterIdx)
                    livingAllyIndices.Add(i);
            }
        }

        if (skill.isAOE)
        {
            // AOE 治疗：治疗所有存活敌人队友（含自身）
            if (enemyList != null)
            {
                for (int i = 0; i < enemyList.Count; i++)
                {
                    if (enemyList[i] != null && enemyList[i].hp.value > 0)
                    {
                        BattleEventDefine.EnemyHpChange.SendEventMessage(i, -healValue,
                            attackerIdx: casterIdx,
                            attackEffect: skill.attackEffect,
                            hitEffect: skill.hitEffect);

                        BattleEventDefine.HealEffect.SendEventMessage(
                            casterIsPlayer: false, casterIdx: casterIdx,
                            targetIsPlayer: false, targetIdx: i,
                            healValue: healValue,
                            attackEffect: skill.attackEffect,
                            hitEffect: skill.hitEffect);
                    }
                }
            }
        }
        else if (livingAllyIndices.Count > 0)
        {
            // 单体治疗：随机选择一个存活敌人队友
            int targetIdx = livingAllyIndices[Random.Range(0, livingAllyIndices.Count)];

            BattleEventDefine.EnemyHpChange.SendEventMessage(targetIdx, -healValue,
                attackerIdx: casterIdx,
                attackEffect: skill.attackEffect,
                hitEffect: skill.hitEffect);

            BattleEventDefine.HealEffect.SendEventMessage(
                casterIsPlayer: false, casterIdx: casterIdx,
                targetIsPlayer: false, targetIdx: targetIdx,
                healValue: healValue,
                attackEffect: skill.attackEffect,
                hitEffect: skill.hitEffect);
        }
        // 如果没有存活队友可治疗，技能仍然消耗（特效播放但无目标）

        return SkillInfo.GetEffectTime(skill);
    }

    /// <summary>
    /// 播放技能音效（未配置则不播放）
    /// </summary>
    private static void PlaySkillSFX(SkillInfo skill)
    {
        if (skill != null && !string.IsNullOrEmpty(skill.sfxName))
        {
            MusicEventDefine.PlaySFX.SendEventMessage(skill.sfxName);
        }
    }
}
