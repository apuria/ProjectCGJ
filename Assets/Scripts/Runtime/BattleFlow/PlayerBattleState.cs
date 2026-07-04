using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UniFramework.Machine;
using UnityEngine;

public class PlayerBattleState : BaseBattleState
{
    public PlayerBattleInfo info;

    // 待执行的技能（等选敌人目标）
    private SkillInfo pendingSkill;

    private const float MinWaitTime = 1.5f;

    public override void OnCreate(StateMachine machine, IStateData stateData)
    {
        base.OnCreate(machine, stateData);
        info = (PlayerBattleInfo)stateData;
    }

    public override void OnEnter()
    {
        pendingSkill = null;

        // 注册技能/敌人选择/队友选择/防御监听
        eventGroup.AddListener<BattleEventDefine.SelectSkill>(OnHandleSelectSkill);
        eventGroup.AddListener<BattleEventDefine.SelectEnemy>(OnHandleSelectEnemy);
        eventGroup.AddListener<BattleEventDefine.SelectAlly>(OnHandleSelectAlly);
        eventGroup.AddListener<BattleEventDefine.PlayerDefend>(OnHandleDefend);

        // 发送 UI 更新事件
        BattleEventDefine.UpdateActionSide.SendEventMessage(
            roleList: info.roleList,
            enemyList: info.enemyList,
            actionQueue: info.actionQueue
        );
    }

    public override void OnExit()
    {
        eventGroup.RemoveAllListener();
    }

    public override void OnUpdate()
    {

    }

    public override void OnDispose()
    {
        base.OnDispose();
    }

    /// <summary>
    /// 选择了技能：检查 MP → AOE 直接执行，否则等待选敌人
    /// </summary>
    private void OnHandleSelectSkill(IEventMessage message)
    {
        var msg = (BattleEventDefine.SelectSkill)message;
        pendingSkill = msg.skill;

        if (pendingSkill == null) return;

        // 检查 MP 是否足够（普通攻击 mpCost 为 0 不受限）
        if (info.roleInfo.mp.value < pendingSkill.mpCost)
        {
            pendingSkill = null;
            BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
            return;
        }

        if (pendingSkill.isAOE)
        {
            if (pendingSkill.targetType == SkillTargetType.Ally)
            {
                ConsumeMpAndExecute(pendingSkill, () => ExecuteAOEHeal(pendingSkill));
            }
            else
            {
                ConsumeMpAndExecute(pendingSkill, () => ExecuteAOE(pendingSkill));
            }
            pendingSkill = null;
        }
    }

    /// <summary>
    /// 防御：恢复 5%~10% MP + 获得临时护盾（不再回复 HP）
    /// </summary>
    private void OnHandleDefend(IEventMessage message)
    {
        if (info.roleInfo == null)
        {
            BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
            return;
        }

        int actorIdx = info.roleList != null ? info.roleList.IndexOf(info.roleInfo) : -1;

        // 随机恢复 5%~10% MP（通过事件，BattleState 处理 + UI 刷新），最少恢复 1 点
        float mpRatio = Random.Range(0.05f, 0.10f);
        int mpRecover = Mathf.Max(1, (int)(mpRatio * info.roleInfo.maxMp.value));
        BattleEventDefine.RoleMpChange.SendEventMessage(actorIdx, -mpRecover);

        // 临时护盾（当前回合有效）
        info.roleInfo.tempDefense = info.roleInfo.shieldValue.value;

        // 发送防御特效事件（在玩家角色身上播放防御VFX）
        if (actorIdx >= 0)
            BattleEventDefine.DefendEffect.SendEventMessage(true, actorIdx);

        BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
    }

    /// <summary>
    /// 选择了敌人：结合待执行技能执行攻击
    /// </summary>
    private void OnHandleSelectEnemy(IEventMessage message)
    {
        var msg = (BattleEventDefine.SelectEnemy)message;

        if (pendingSkill != null)
        {
            ConsumeMpAndExecute(pendingSkill, () => ExecuteAttack(pendingSkill, msg.enemyIndex));
            pendingSkill = null;
        }
        else
        {
            // 防御性处理：没有待执行技能时（如面板残留点击），推进回合避免卡死
            BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
        }
    }

    /// <summary>
    /// 选择了队友：结合待执行技能执行治疗
    /// </summary>
    private void OnHandleSelectAlly(IEventMessage message)
    {
        var msg = (BattleEventDefine.SelectAlly)message;

        if (pendingSkill != null)
        {
            ConsumeMpAndExecute(pendingSkill, () => ExecuteHealAlly(pendingSkill, msg.allyIndex));
            pendingSkill = null;
        }
        else
        {
            // 防御性处理：没有待执行技能时（如面板残留点击），推进回合避免卡死
            BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
        }
    }

    /// <summary>
    /// 扣除 MP 并执行行动，发送延迟事件
    /// </summary>
    private void ConsumeMpAndExecute(SkillInfo skill, System.Action executeAction)
    {
        int actorIdx = info.roleList != null ? info.roleList.IndexOf(info.roleInfo) : -1;

        // 扣除 MP（通过事件由 BattleState 统一处理，避免双重扣除）
        if (skill.mpCost > 0)
        {
            BattleEventDefine.RoleMpChange.SendEventMessage(actorIdx, skill.mpCost);
        }

        // 播放技能音效（配置了才播放）
        PlaySkillSFX(skill);

        executeAction();

        float delay = SkillInfo.GetEffectTime(skill);
        BattleEventDefine.EnemyActionDelay.SendEventMessage(delay);
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

    /// <summary>
    /// 单攻：对指定敌人造成伤害（伤害 = 角色攻击力 × 技能倍率）
    /// </summary>
    private void ExecuteAttack(SkillInfo skill, int enemyIdx)
    {
        int damage = Mathf.RoundToInt(info.roleInfo.attack.value * skill.Damage);
        int actorIdx = info.roleList != null ? info.roleList.IndexOf(info.roleInfo) : 0;
        BattleEventDefine.EnemyHpChange.SendEventMessage(enemyIdx, damage,
            attackerIdx: actorIdx,
            attackEffect: skill.attackEffect,
            hitEffect: skill.hitEffect);
    }

    /// <summary>
    /// AOE：对所有存活敌人造成伤害（伤害 = 角色攻击力 × 技能倍率）
    /// </summary>
    private void ExecuteAOE(SkillInfo skill)
    {
        int damage = Mathf.RoundToInt(info.roleInfo.attack.value * skill.Damage);
        int actorIdx = info.roleList != null ? info.roleList.IndexOf(info.roleInfo) : 0;
        for (int i = 0; i < info.enemyList.Count; i++)
        {
            if (info.enemyList[i] != null && info.enemyList[i].hp.value > 0)
            {
                BattleEventDefine.EnemyHpChange.SendEventMessage(i, damage,
                    attackerIdx: actorIdx,
                    attackEffect: skill.attackEffect,
                    hitEffect: skill.hitEffect);
            }
        }
    }

    /// <summary>
    /// 对指定队友治疗（治疗量 = 角色攻击力 × 技能倍率）
    /// </summary>
    private void ExecuteHealAlly(SkillInfo skill, int allyIdx)
    {
        int actorIdx = info.roleList != null ? info.roleList.IndexOf(info.roleInfo) : 0;
        int healValue = Mathf.RoundToInt(info.roleInfo.attack.value * skill.Damage);

        // 通过 RoleHpChange 发送负值表示治疗
        BattleEventDefine.RoleHpChange.SendEventMessage(allyIdx, -healValue,
            attackerIdx: actorIdx,
            attackEffect: skill.attackEffect,
            hitEffect: skill.hitEffect);

        // 发送治疗特效事件
        BattleEventDefine.HealEffect.SendEventMessage(
            casterIsPlayer: true, casterIdx: actorIdx,
            targetIsPlayer: true, targetIdx: allyIdx,
            healValue: healValue,
            attackEffect: skill.attackEffect,
            hitEffect: skill.hitEffect);
    }

    /// <summary>
    /// AOE治疗：对所有存活队友治疗（治疗量 = 角色攻击力 × 技能倍率）
    /// </summary>
    private void ExecuteAOEHeal(SkillInfo skill)
    {
        int actorIdx = info.roleList != null ? info.roleList.IndexOf(info.roleInfo) : 0;
        int healValue = Mathf.RoundToInt(info.roleInfo.attack.value * skill.Damage);

        for (int i = 0; i < info.roleList.Count; i++)
        {
            if (info.roleList[i] != null && info.roleList[i].hp.value > 0)
            {
                BattleEventDefine.RoleHpChange.SendEventMessage(i, -healValue,
                    attackerIdx: actorIdx,
                    attackEffect: skill.attackEffect,
                    hitEffect: skill.hitEffect);

                // 发送治疗特效事件
                BattleEventDefine.HealEffect.SendEventMessage(
                    casterIsPlayer: true, casterIdx: actorIdx,
                    targetIsPlayer: true, targetIdx: i,
                    healValue: healValue,
                    attackEffect: skill.attackEffect,
                    hitEffect: skill.hitEffect);
            }
        }
    }
}
