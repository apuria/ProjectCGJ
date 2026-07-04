using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UnityEngine;

public class BattleEventDefine : MonoBehaviour
{
//战斗事件
/*
1. 指定角色受到伤害
2. 指定敌人受到伤害
3. 更新UI
4. 战斗结束
5. 玩家选择技能 (普通攻击, 技能1, 技能2, 大招)
6. 玩家选择敌人 (敌人1, 敌人2, 敌人3)
*/

    public class NextTurn : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new NextTurn();
            UniEvent.SendMessage(msg);
        }
    }

    public class NextRound : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new NextRound();
            UniEvent.SendMessage(msg);
        }
    }

    public class EnemyHpChange : IEventMessage
    {
        public int idx;
        public int hurtValue;
        /// <summary>
        /// 攻击方索引（玩家角色在 roleList 中的索引，用于定位攻击特效的 VFXPoint）
        /// </summary>
        public int attackerIdx;
        /// <summary>
        /// 技能攻击特效（可选），攻击方播放
        /// </summary>
        public GameObject attackEffect;
        /// <summary>
        /// 技能受击特效（可选），受击方播放
        /// </summary>
        public GameObject hitEffect;

        public static void SendEventMessage(int idx, int hurtValue,
            int attackerIdx = 0,
            GameObject attackEffect = null,
            GameObject hitEffect = null)
        {
            var msg = new EnemyHpChange();
            msg.idx = idx;
            msg.hurtValue = hurtValue;
            msg.attackerIdx = attackerIdx;
            msg.attackEffect = attackEffect;
            msg.hitEffect = hitEffect;
            UniEvent.SendMessage(msg);
        }
    }

    public class RoleHpChange : IEventMessage
    {
        public int idx;
        public int hurtValue;
        /// <summary>
        /// 攻击方索引（敌人方在 enemyList 中的索引，用于定位攻击特效的 VFXPoint）
        /// </summary>
        public int attackerIdx;
        /// <summary>
        /// 技能攻击特效（可选），攻击方播放
        /// </summary>
        public GameObject attackEffect;
        /// <summary>
        /// 技能受击特效（可选），受击方播放
        /// </summary>
        public GameObject hitEffect;

        public static void SendEventMessage(int idx, int hurtValue,
            int attackerIdx = 0,
            GameObject attackEffect = null,
            GameObject hitEffect = null)
        {
            var msg = new RoleHpChange();
            msg.idx = idx;
            msg.hurtValue = hurtValue;
            msg.attackerIdx = attackerIdx;
            msg.attackEffect = attackEffect;
            msg.hitEffect = hitEffect;
            UniEvent.SendMessage(msg);
        }
    }

    public class RoleMpChange : IEventMessage
    {
        public int idx;
        public int changeValue;
        public static void SendEventMessage(int idx, int changeValue)
        {
            var msg = new RoleMpChange();
            msg.idx = idx;
            msg.changeValue = changeValue;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 初始化战斗UI事件（仅传入角色和敌人列表，不做行动方判断）
    /// 类似战斗开始前的伪加载，展示有哪些敌人和角色
    /// </summary>
    public class InitBattleUI : IEventMessage
    {
        public List<RoleInfo> roleList;
        public List<EnemyInfo> enemyList;

        public static void SendEventMessage(List<RoleInfo> roleList, List<EnemyInfo> enemyList)
        {
            var msg = new InitBattleUI();
            msg.roleList = roleList;
            msg.enemyList = enemyList;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 战斗结束事件：显示结算按钮
    /// </summary>
    public class BattleEnd : IEventMessage
    {
        /// <summary>
        /// true = 胜利, false = 失败
        /// </summary>
        public bool isWin;

        public static void SendEventMessage(bool isWin)
        {
            var msg = new BattleEnd();
            msg.isWin = isWin;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 战斗结束确认事件：玩家点击结算按钮后，BattleState 收到此事件执行 WinEvent/LoseEvent
    /// </summary>
    public class BattleEndConfirm : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new BattleEndConfirm();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 选择技能事件：玩家选择了技能（普通攻击、技能、大招）
    /// </summary>
    public class SelectSkill : IEventMessage
    {
        public SkillInfo skill;

        public static void SendEventMessage(SkillInfo skill)
        {
            var msg = new SelectSkill();
            msg.skill = skill;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 选择敌人事件：玩家选择了攻击目标
    /// </summary>
    public class SelectEnemy : IEventMessage
    {
        public int enemyIndex;

        public static void SendEventMessage(int enemyIndex)
        {
            var msg = new SelectEnemy();
            msg.enemyIndex = enemyIndex;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 玩家防御事件
    /// </summary>
    public class PlayerDefend : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new PlayerDefend();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 角色受伤效果事件
    /// </summary>
    public class RoleDamageEffect : IEventMessage
    {
        public int idx;
        public bool isDead;
        public int hurtValue;
        /// <summary>
        /// 攻击方索引（敌人方在 enemyList 中的索引）
        /// </summary>
        public int attackerIdx;
        /// <summary>
        /// 技能攻击特效（可选），放在攻击方 VFXPoint 立即播放
        /// </summary>
        public GameObject attackEffect;
        /// <summary>
        /// 技能受击特效（可选），延迟与伤害数字同时播放
        /// </summary>
        public GameObject hitEffect;

        public static void SendEventMessage(int idx, bool isDead, int hurtValue,
            int attackerIdx = 0,
            GameObject attackEffect = null,
            GameObject hitEffect = null)
        {
            var msg = new RoleDamageEffect();
            msg.idx = idx;
            msg.isDead = isDead;
            msg.hurtValue = hurtValue;
            msg.attackerIdx = attackerIdx;
            msg.attackEffect = attackEffect;
            msg.hitEffect = hitEffect;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 敌人受伤效果事件
    /// </summary>
    public class EnemyDamageEffect : IEventMessage
    {
        public int idx;
        public bool isDead;
        public int hurtValue;
        /// <summary>
        /// 攻击方索引（玩家角色在 roleList 中的索引）
        /// </summary>
        public int attackerIdx;
        /// <summary>
        /// 技能攻击特效（可选），放在攻击方 VFXPoint 立即播放
        /// </summary>
        public GameObject attackEffect;
        /// <summary>
        /// 技能受击特效（可选），延迟与伤害数字同时播放
        /// </summary>
        public GameObject hitEffect;

        public static void SendEventMessage(int idx, bool isDead, int hurtValue,
            int attackerIdx = 0,
            GameObject attackEffect = null,
            GameObject hitEffect = null)
        {
            var msg = new EnemyDamageEffect();
            msg.idx = idx;
            msg.isDead = isDead;
            msg.hurtValue = hurtValue;
            msg.attackerIdx = attackerIdx;
            msg.attackEffect = attackEffect;
            msg.hitEffect = hitEffect;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 敌方行动延迟事件：携带等待时间（秒），BattleState 收到后计时，到期调 EndTurn
    /// </summary>
    public class EnemyActionDelay : IEventMessage
    {
        public float delay;

        public static void SendEventMessage(float delay)
        {
            var msg = new EnemyActionDelay();
            msg.delay = delay;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 敌人行动提示事件：显示 "{敌人名称} 使用了 {行动类型}"
    /// </summary>
    public class EnemyActionToast : IEventMessage
    {
        public string enemyName;
        public EnemyActionType actionType;

        public static void SendEventMessage(string enemyName, EnemyActionType actionType)
        {
            var msg = new EnemyActionToast();
            msg.enemyName = enemyName;
            msg.actionType = actionType;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 更新行动方UI事件
    /// 传入行动队列（第一顺位为当前行动者），UI 侧从第一顺位类型自行判断阵营
    /// </summary>
    public class UpdateActionSide : IEventMessage
    {
        /// <summary>
        /// 所有角色列表
        /// </summary>
        public List<RoleInfo> roleList;
        /// <summary>
        /// 所有敌人列表
        /// </summary>
        public List<EnemyInfo> enemyList;
        /// <summary>
        /// 行动队列（第一顺位为当前行动者，后续为等待队列）
        /// </summary>
        public List<BaseInfo> actionQueue;

        public static void SendEventMessage(List<RoleInfo> roleList = null,
            List<EnemyInfo> enemyList = null, List<BaseInfo> actionQueue = null)
        {
            var msg = new UpdateActionSide();
            msg.roleList = roleList;
            msg.enemyList = enemyList;
            msg.actionQueue = actionQueue;
            UniEvent.SendMessage(msg);
        }
    }

}
