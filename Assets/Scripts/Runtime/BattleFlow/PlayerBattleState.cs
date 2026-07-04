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

        // 注册技能/敌人选择/防御监听
        eventGroup.AddListener<BattleEventDefine.SelectSkill>(OnHandleSelectSkill);
        eventGroup.AddListener<BattleEventDefine.SelectEnemy>(OnHandleSelectEnemy);
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
            ConsumeMpAndExecute(pendingSkill, () => ExecuteAOE(pendingSkill));
            pendingSkill = null;
        }
    }

    /// <summary>
    /// 防御：恢复 5%~10% HP + 获得临时护盾（不再回复 MP）
    /// </summary>
    private void OnHandleDefend(IEventMessage message)
    {
        if (info.roleInfo == null)
        {
            BattleEventDefine.EnemyActionDelay.SendEventMessage(MinWaitTime);
            return;
        }

        int actorIdx = info.roleList != null ? info.roleList.IndexOf(info.roleInfo) : -1;

        // 随机恢复 5%~10% HP（通过事件，BattleState 处理 + UI 刷新），最少恢复 1 点
        float hpRatio = Random.Range(0.05f, 0.10f);
        int hpRecover = Mathf.Max(1, (int)(hpRatio * info.roleInfo.maxHp.value));
        BattleEventDefine.RoleHpChange.SendEventMessage(actorIdx, -hpRecover);

        // 临时护盾（当前回合有效）
        info.roleInfo.tempDefense = info.roleInfo.shieldValue.value;

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

        executeAction();

        float delay = SkillInfo.GetEffectTime(skill);
        BattleEventDefine.EnemyActionDelay.SendEventMessage(delay);
    }

    /// <summary>
    /// 单攻：对指定敌人造成伤害（伤害 = 角色攻击力 × 技能倍率）
    /// </summary>
    private void ExecuteAttack(SkillInfo skill, int enemyIdx)
    {
        int damage = Mathf.RoundToInt(info.roleInfo.attack.value * skill.Damage);
        BattleEventDefine.EnemyHpChange.SendEventMessage(enemyIdx, damage,
            attackerIdx: 0,
            attackEffect: skill.attackEffect,
            hitEffect: skill.hitEffect);
    }

    /// <summary>
    /// AOE：对所有存活敌人造成伤害（伤害 = 角色攻击力 × 技能倍率）
    /// </summary>
    private void ExecuteAOE(SkillInfo skill)
    {
        int damage = Mathf.RoundToInt(info.roleInfo.attack.value * skill.Damage);
        for (int i = 0; i < info.enemyList.Count; i++)
        {
            if (info.enemyList[i] != null && info.enemyList[i].hp.value > 0)
            {
                BattleEventDefine.EnemyHpChange.SendEventMessage(i, damage,
                    attackerIdx: 0,
                    attackEffect: skill.attackEffect,
                    hitEffect: skill.hitEffect);
            }
        }
    }
}
