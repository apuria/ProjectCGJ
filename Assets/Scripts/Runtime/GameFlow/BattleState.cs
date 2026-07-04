using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;

public class BattleState : BaseState
{
//战斗状态
/*
1. 战斗状态机
2. 检查是否有战斗开始事件
3. 检查是否有战斗结束事件
4. 每次切换战斗状态时，检查是否满足战斗中途事件的条件
5. 失败条件: 角色全部死亡
6. 胜利条件: 自行配置
    1. 敌人全部死亡
    2. 指定敌人死亡
    3. 指定回合数
7. 游戏结束事件差分: 战斗胜利和战斗失败

1. 从游戏管理器中读取玩家配置: 
    1. 有几个角色
    2. 角色信息
    3. 添加到战斗状态中
2. 从战斗配置表中读取敌人配置:
    1. 有几个敌人
    2. 敌人信息
    3. 添加到战斗状态中
*/

    public int round = 0;

    public bool isBattleStart = false;

    public BattleSetting battleSetting;

    public StateMachine battleMachine;

    /// <summary>
    /// 战斗回合队列
    /// </summary>
    public Queue<BaseInfo> roundQueue = new();
    public List<BaseInfo> roundList = new();

    /// <summary>
    /// 伪加载阶段计时器
    /// </summary>
    private float loadDelay = 0f;
    private bool isLoadPhase = false;

    /// <summary>
    /// 当前回合的完整行动队列（在 NextRound 中构建，供 PlayerBattleState/EnemyBattleState 使用）
    /// </summary>
    private List<BaseInfo> currentFullQueue;

    /// <summary>
    /// 战斗是否已结束（防止重复发送 BattleEnd 事件）
    /// </summary>
    private bool isBattleEnded = false;

    /// <summary>
    /// 已触发的中途事件索引集合（防止每回合重复触发）
    /// </summary>
    private HashSet<int> triggeredMidEventIndices = new();

    /// <summary>
    /// 敌方行动后等待特效播放的计时器
    /// </summary>
    private float turnDelay = 0f;
    private bool isWaitingTurnDelay = false;
    
    /// <summary>
    /// 角色列表
    /// </summary>
    public List<RoleInfo> roleList = new();
    public int liveRoleCount => roleList.FindAll(r => r.hp.value > 0).Count;
    /// <summary>
    /// 敌人列表
    /// </summary>
    public List<EnemyInfo> enemyList = new();
    public int liveEnemyCount => enemyList.FindAll(e => e.hp.value > 0).Count;

#region 生命周期
    public override void OnCreate(StateMachine machine, IStateData data)
    {
        base.OnCreate(machine, data);
        battleSetting = data as BattleSetting;
        battleMachine = new StateMachine();

        // 在 ShowPanel 之前注册事件监听，确保 BattleState 的 HP/MP 修改
        // 先于 BattlePanel 的 UI 刷新执行，避免 UI 显示滞后一帧
        eventGroup.AddListener<BattleEventDefine.NextTurn>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.NextRound>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.EnemyHpChange>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.RoleHpChange>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.RoleMpChange>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.BattleEndConfirm>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.EnemyActionDelay>(OnHandleEventMessage);

        UIMgr.Instance.ShowPanel<BattlePanel>(isSync: true);
    }

    public override void OnEnter()
    {
        if (!isBattleStart)
        {
            // === 首次进入：初始化并处理开始事件 ===
            isBattleStart = true;

            // 初始化战斗数据 + 更新立绘（仅展示阵容）
            InitBattle();

            // 设置准备阶段计时器 1.2s
            // 如果存在开始事件（如对话），先切换到对话状态，
            // 对话结束后状态机通过 OnEnter 再次进入（走 else 分支）
            isLoadPhase = true;
            loadDelay = 1.2f;

            if (battleSetting.startEvent != EBattleStartEvent.None)
            {
                StartEvent(battleSetting.startEvent);
            }
        }
        else
        {
            // === 从开始事件（对话/分支）返回，重新开始准备阶段倒计时 ===
            isLoadPhase = true;
            loadDelay = 1.2f;
        }
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {
        battleMachine.Update();

        // 加载等待阶段：1.2s 后正式开始
        if (isLoadPhase)
        {
            loadDelay -= Time.deltaTime;
            if (loadDelay <= 0f)
            {
                isLoadPhase = false;
                NextRound();
                StartTurn();
            }
        }
        // 使用 else if 防止同帧处理：StartTurn() 中首个敌人行动
        // 同步设置的 isWaitingTurnDelay 不会在当前帧被扣减
        else if (isWaitingTurnDelay)
        {
            turnDelay -= Time.deltaTime;
            if (turnDelay <= 0f)
            {
                isWaitingTurnDelay = false;
                if (!isBattleEnded)
                    EndTurn();
            }
        }
    }

    public override void OnDispose()
    {
        UIMgr.Instance.HidePanel<BattlePanel>(true);
        base.OnDispose();
    }

#endregion

#region 事件监听

    public override void OnHandleEventMessage(IEventMessage message)
    {
        if(message is BattleEventDefine.NextTurn)
        {
            EndTurn();
        }
        else if(message is BattleEventDefine.NextRound)
        {
            NextRound();
        }
        else if(message is BattleEventDefine.EnemyHpChange ehcMsg)
        {
            if (ehcMsg.idx >= 0 && ehcMsg.idx < enemyList.Count)
            {
                var enemy = enemyList[ehcMsg.idx];
                float remaining = ehcMsg.hurtValue;
                float actualDamage = 0f;

                // 先扣除临时防御值（护盾）
                if (remaining > 0 && enemy.tempDefense > 0)
                {
                    float absorbed = Mathf.Min(remaining, enemy.tempDefense);
                    enemy.tempDefense -= absorbed;
                    remaining -= absorbed;
                }

                if (remaining > 0)
                {
                    actualDamage = remaining;
                    enemy.hp.value -= remaining;
                    if (enemy.hp.value < 0) enemy.hp.value = 0;
                }
                else if (remaining < 0)
                {
                    // 治疗
                    enemy.hp.value -= remaining; // -= negative = add
                    if (enemy.hp.value > enemy.maxHp.value) enemy.hp.value = enemy.maxHp.value;
                }

                // 受伤特效（仅伤害时，治疗不触发），显示减免后的实际伤害 ×10
                if (ehcMsg.hurtValue > 0)
                {
                    bool isDead = enemy.hp.value <= 0;
                    int displayDamage = Mathf.RoundToInt(actualDamage * 10f);
                    BattleEventDefine.EnemyDamageEffect.SendEventMessage(ehcMsg.idx, isDead, displayDamage,
                        ehcMsg.attackerIdx,
                        ehcMsg.attackEffect,
                        ehcMsg.hitEffect);
                }
            }
            CheckIsEnd();
        }
        else if(message is BattleEventDefine.RoleHpChange rhcMsg)
        {
            if (rhcMsg.idx >= 0 && rhcMsg.idx < roleList.Count)
            {
                var role = roleList[rhcMsg.idx];
                float remaining = rhcMsg.hurtValue;
                float actualDamage = 0f;

                // 先扣除临时防御值（护盾）
                if (remaining > 0 && role.tempDefense > 0)
                {
                    float absorbed = Mathf.Min(remaining, role.tempDefense);
                    role.tempDefense -= absorbed;
                    remaining -= absorbed;
                }

                if (remaining > 0)
                {
                    actualDamage = remaining;
                    role.hp.value -= remaining;
                    if (role.hp.value < 0) role.hp.value = 0;
                }
                else if (remaining < 0)
                {
                    // 治疗
                    role.hp.value -= remaining;
                    if (role.hp.value > role.maxHp.value) role.hp.value = role.maxHp.value;
                }

                // 受伤特效（仅伤害时，治疗不触发），显示减免后的实际伤害 ×10
                if (rhcMsg.hurtValue > 0)
                {
                    bool isDead = role.hp.value <= 0;
                    int displayDamage = Mathf.RoundToInt(actualDamage * 10f);
                    BattleEventDefine.RoleDamageEffect.SendEventMessage(rhcMsg.idx, isDead, displayDamage,
                        rhcMsg.attackerIdx,
                        rhcMsg.attackEffect,
                        rhcMsg.hitEffect);
                }
            }
            CheckIsEnd();
        }
        else if(message is BattleEventDefine.RoleMpChange rmcMsg)
        {
            if (rmcMsg.idx >= 0 && rmcMsg.idx < roleList.Count)
            {
                var role = roleList[rmcMsg.idx];
                role.mp.value -= rmcMsg.changeValue; // -= negative = add
                if (role.mp.value < 0) role.mp.value = 0;
                if (role.mp.value > role.maxMp.value) role.mp.value = role.maxMp.value;
            }
        }
        else if(message is BattleEventDefine.BattleEndConfirm)
        {
            if (liveEnemyCount == 0)
                WinEvent();
            else if (liveRoleCount == 0)
                LoseEvent();
        }
        else if(message is BattleEventDefine.EnemyActionDelay delayMsg)
        {
            // 已在等待中则忽略后续延迟请求，防止双击等重复触发
            if (!isBattleEnded && !isWaitingTurnDelay)
            {
                isWaitingTurnDelay = true;
                turnDelay = delayMsg.delay;
            }
        }
    }

#endregion

#region 游戏逻辑
    /// <summary>
    /// 初始化战斗
    /// </summary>
    public void InitBattle()
    {
        //初始化战斗
        /*
        1. 记录本次战斗的角色列表
        2. 记录本次战斗的敌人列表
        3. UI更新战斗信息
        */
        foreach (var enemy in battleSetting.enemies)
        {
            var instance = Object.Instantiate(enemy);
            EnsureStatDefaults(instance);
            enemyList.Add(instance);
        }

        var mainRoleInst = Object.Instantiate(GameManager.Instance.mainRole);
        EnsureStatDefaults(mainRoleInst);
        roleList.Add(mainRoleInst);
        foreach(var roleBranch in battleSetting.moreRoleBranches)
        {
            var branches = GameManager.Instance.PlayerData.branchList;
            bool hasKey = branches.ContainsKey(roleBranch.branchId);
            string branchVal = hasKey ? branches[roleBranch.branchId] : null;

            // 两个都为空直接添加，否则按选择值匹配
            if (string.IsNullOrEmpty(branchVal) && string.IsNullOrEmpty(roleBranch.choose))
            {
                var inst = Object.Instantiate(GameManager.Instance.roleList[roleBranch.roleIndex]);
                EnsureStatDefaults(inst);
                roleList.Add(inst);
            }
            else if (hasKey && branchVal == roleBranch.choose)
            {
                var inst = Object.Instantiate(GameManager.Instance.roleList[roleBranch.roleIndex]);
                EnsureStatDefaults(inst);
                roleList.Add(inst);
            }
        }

        // 重置敌方 AI 概率
        EnemyAI.Reset();

        // 发送初始化战斗UI事件：展示角色和敌人，隐藏按钮和Action界面
        BattleEventDefine.InitBattleUI.SendEventMessage(roleList, enemyList);
    }

    /// <summary>
    /// 为未在 Editor 中配置数值的 Stat 提供默认值，
    /// 避免因 Stat 未序列化导致 hp/speed 全为 0 使战斗无法进行
    /// </summary>
    private static void EnsureStatDefaults(BaseInfo info)
    {
        if (info == null) return;
        // 仅当值未配置（≤0）时写入默认值；已在 Editor 中配置的值不受影响
        if (info.maxHp.value <= 0)  info.maxHp.value = 100;
        if (info.hp.value <= 0)     info.hp.value = info.maxHp.value;
        if (info.maxMp.value <= 0)  info.maxMp.value = 50;
        if (info.mp.value <= 0)     info.mp.value = info.maxMp.value;
        if (info.speed.value <= 0)  info.speed.value = 10;
        if (info.shieldValue.value <= 0) info.shieldValue.value = 10;
    }

    /// <summary>
    /// 开始当前顺位的行动（Peek，不 Dequeue），自动跳过已死亡的行动者
    /// </summary>
    private void StartTurn()
    {
        if (isBattleEnded)
            return;

        bool tryAgain = true;
        while (tryAgain)
        {
            tryAgain = false;
            // CheckMidEvent 在 NextRound() 中统一调用，此处不重复

            if (roundQueue.Count == 0)
            {
                NextRound();
            }

            // 跳过本回合内已死亡的行动者
            bool skippedDead = false;
            while (roundQueue.Count > 0)
            {
                BaseInfo info = roundQueue.Peek();
                bool isDead = (info is RoleInfo r && r.hp.value <= 0) ||
                              (info is EnemyInfo e && e.hp.value <= 0);
                if (!isDead)
                    break;
                roundQueue.Dequeue();
                skippedDead = true;
            }

            // 有死者被跳过 → 更新完整队列（UI 不再显示死者）
            if (skippedDead)
            {
                currentFullQueue = new List<BaseInfo>(roundQueue);
            }

            if (roundQueue.Count == 0)
            {
                NextRound();
                if (roundQueue.Count > 0)
                {
                    tryAgain = true;
                }
                continue;
            }

            BaseInfo actor = roundQueue.Peek();
            if (actor is RoleInfo roleInfo)
            {
                battleMachine.SwitchTo<PlayerBattleState>("PlayerBattleState",
                    new PlayerBattleInfo(roleInfo, roleList, enemyList, currentFullQueue));
            }
            else if (actor is EnemyInfo enemyInfo)
            {
                battleMachine.SwitchTo<EnemyBattleState>("EnemyBattleState",
                    new EnemyBattleInfo(enemyInfo, round, roleList, enemyList, currentFullQueue));
            }
        }
    }

    /// <summary>
    /// 结束当前行动（Dequeue），开始下一顺位
    /// </summary>
    private void EndTurn()
    {
        if (isBattleEnded)
            return;

        if (roundQueue.Count > 0)
            roundQueue.Dequeue();

        // 同步更新完整队列，确保传给下一个 BattleState 的 actionQueue 反映当前状态
        currentFullQueue = new List<BaseInfo>(roundQueue);

        StartTurn();
    }

    /// <summary>
    /// 下一个回合：重新构建回合队列和完整行动队列
    /// </summary>
    public void NextRound()
    {
        if (isBattleEnded)
            return;

        //下一个回合
        /*
        1. 清空回合队列
        2. 计算角色和敌人的行动顺序
        3. 构建完整行动队列
        4. 调用 StartTurn()
        */
        round++;
        CheckMidEvent();
        roundQueue.Clear();
        roundList.Clear();
        foreach (var role in roleList)
        {
            if(role.hp.value > 0)
                roundList.Add(role);
        }
        foreach (var enemy in enemyList)
        {
            if(enemy.hp.value > 0)
            {
                roundList.Add(enemy);
            }
        }

        // 按speed从大到小排序
        roundList.Sort((a, b) => b.speed.value.CompareTo(a.speed.value));
        // 将排序后的元素加入回合队列
        foreach (var info in roundList)
        {
            roundQueue.Enqueue(info);
        }

        // 构建完整行动队列（在当前回合内不变，供各 BattleState 使用）
        currentFullQueue = new List<BaseInfo>(roundQueue);

        // 新回合：清除所有临时防御值
        foreach (var role in roleList)
        {
            if (role != null)
                role.tempDefense = 0;
        }
        foreach (var enemy in enemyList)
        {
            if (enemy != null)
                enemy.tempDefense = 0;
        }
    }

#endregion

#region 事件处理

    public void CheckIsEnd()
    {
        if (isBattleEnded)
            return;

        if (liveRoleCount == 0)
        {
            isBattleEnded = true;
            BattleEventDefine.BattleEnd.SendEventMessage(false);
        }
        else if (liveEnemyCount == 0)
        {
            isBattleEnded = true;
            BattleEventDefine.BattleEnd.SendEventMessage(true);
        }
    }

    public void CheckMidEvent()
    {
        //检查战斗中途事件
        /*
        1. 检查是否满足战斗中途事件的条件
        2. 如果满足，调用MidEvent()
        */

        if (battleSetting == null || battleSetting.midEvents == null) return;

        for (int i = 0; i < battleSetting.midEvents.Count; i++)
        {
            var midEvent = battleSetting.midEvents[i];
            if (midEvent == null) continue;

            // 已触发过的事件不再重复触发
            if (triggeredMidEventIndices.Contains(i)) continue;

            switch(midEvent.triggerType)
            {
                case EMidEventTriggerType.CharacterHpThreshold:
                {
                    if (midEvent.targetIndex < 0 || midEvent.targetIndex >= roleList.Count) continue;
                    var role = roleList[midEvent.targetIndex];
                    if (role == null) continue;
                    if (role.hp.value <= role.maxHp.value * midEvent.hpPercentage * 0.01f)
                    {
                        triggeredMidEventIndices.Add(i);
                        MidEvent(midEvent);
                    }
                    break;
                }
                case EMidEventTriggerType.EnemyHpThreshold:
                {
                    if (midEvent.targetIndex < 0 || midEvent.targetIndex >= enemyList.Count) continue;
                    var enemy = enemyList[midEvent.targetIndex];
                    if (enemy == null) continue;
                    if (enemy.hp.value <= enemy.maxHp.value * midEvent.hpPercentage * 0.01f)
                    {
                        triggeredMidEventIndices.Add(i);
                        MidEvent(midEvent);
                    }
                    break;
                }
                case EMidEventTriggerType.RoundCount:
                    if(round == midEvent.roundCount)
                    {
                        triggeredMidEventIndices.Add(i);
                        MidEvent(midEvent);
                    }
                    break;
            }
        }
    }
    public void StartEvent(EBattleStartEvent _event)
    {
        switch(_event)
        {
            case EBattleStartEvent.None:
                // 无事件
                break;
            case EBattleStartEvent.PlayDialogue:
                // 播放对话
                StartDialogue();
                break;
            case EBattleStartEvent.StartBranch:
                // 开始分支选项
                StartBranchForStartEvent();
                break;
        }
    }

    public void WinEvent()
    {
        switch(battleSetting.winEvent)
        {
            case EBattleEndEvent.None:
                // 无结束事件：返回主菜单，经过加载界面，清除所有挂起状态
                GoToMainMenu();
                break;
            case EBattleEndEvent.NextDialogue:
                // 进入下一个对话
                NextDialogue(battleSetting.winDialogue);
                break;
            case EBattleEndEvent.GoBackToLastState:
                // 回到上一个状态
                GoBackToLastState();
                break;
            case EBattleEndEvent.StartBranch:
                // 开始新的分支选项
                StartBranch(battleSetting.winBranch);
                break;
            case EBattleEndEvent.GoBackToMap:
                // 回到地图
                GoBackToMap();
                break;
            case EBattleEndEvent.EndBranch:
                // 结束分支，回到地图界面，玩家数据进入下一个节点
                EndBranch();
                break;
            case EBattleEndEvent.End:
                // 进入 EndState，根据 EndSetting 中的分支条件决定后续流程
                WinEnd();
                break;
        }
    }

    public void LoseEvent()
    {
        switch(battleSetting.loseEvent)
        {
            case EBattleEndEvent.None:
                // 无结束事件：返回主菜单，经过加载界面，清除所有挂起状态
                GoToMainMenu();
                break;
            case EBattleEndEvent.NextDialogue:
                // 进入下一个对话
                NextDialogue(battleSetting.loseDialogue);
                break;
            case EBattleEndEvent.GoBackToLastState:
                // 回到上一个状态
                GoBackToLastState();
                break;
            case EBattleEndEvent.StartBranch:
                // 开始新的分支选项
                StartBranch(battleSetting.loseBranch);
                break;
            case EBattleEndEvent.GoBackToMap:
                // 回到地图
                GoBackToMap();
                break;
            case EBattleEndEvent.EndBranch:
                // 结束分支，回到地图界面，玩家数据进入下一个节点
                EndBranch();
                break;
            case EBattleEndEvent.End:
                // 进入 EndState，根据 EndSetting 中的分支条件决定后续流程
                LoseEnd();
                break;
        }
    }

    public void MidEvent(MidBattleEvent midEvent)
    {
        switch (midEvent.action)
        {
            case EMidEventAction.PlayDialogue:
                // 播放对话
                MidDialogue(midEvent);
                break;
            case EMidEventAction.StartBranch:
                // 开始新的分支选项
                MidBranch(midEvent);
                break;
        }
    }


    private void StartDialogue()
    {
        // 使用状态机切换到对话状态，播放战斗开始对话
        StateEventDefine.ChangeState.SendEventMessage<DialogueState>("LogState", battleSetting.startDialogue,false);
    }

    private void StartBranchForStartEvent()
    {
        // 使用状态机切换到分支选项状态
        StateEventDefine.ChangeState.SendEventMessage<BranchState>("BranchState", battleSetting.startBranch, false);
    }

    private void NextDialogue(DialogueSetting dialogue)
    {
        // 使用状态机切换到对话状态
        StateEventDefine.ChangeState.SendEventMessage<DialogueState>("LogState", dialogue);
    }

    private void GoBackToLastState()
    {
        // 使用状态机切换到上一个状态
        StateEventDefine.BackToPrevState.SendEventMessage();
    }

    private void StartBranch(BranchSetting branch)
    {
        // 使用状态机切换到分支选项状态
        StateEventDefine.ChangeState.SendEventMessage<BranchState>("BranchState", branch, false);
    }

    private void GoBackToMap()
    {
        // 使用状态机切换到地图状态
        StateEventDefine.ChangeState.SendEventMessage<MapState>("MapState");
    }

    private void MidDialogue(MidBattleEvent midEvent)
    {
        // 使用状态机切换到对话状态，播放战斗中对话
        StateEventDefine.ChangeState.SendEventMessage<DialogueState>("LogState", midEvent.dialogue, false);
    }

    private void MidBranch(MidBattleEvent midEvent)
    {
        // 使用状态机切换到分支选项状态
        StateEventDefine.ChangeState.SendEventMessage<BranchState>("BranchState", midEvent.branch, false);
    }

    private void EndBranch()
    {
        // 结束当前一段剧情，玩家数据进入下一个节点，然后回到地图界面
        // 调用UI显示剧情结束过渡效果
        GameManager.Instance.NextNode();
        GoBackToMap();
    }

    private void WinEnd()
    {
        // 切换到 EndState，销毁当前 BattleState
        StateEventDefine.ChangeState.SendEventMessage<EndState>("EndState", battleSetting.winEndSetting, true);
    }

    private void LoseEnd()
    {
        // 切换到 EndState，销毁当前 BattleState
        StateEventDefine.ChangeState.SendEventMessage<EndState>("EndState", battleSetting.loseEndSetting, true);
    }

    /// <summary>
    /// 返回主菜单：清除所有挂起状态，经过 LoadingState 加载界面切换到 GameStart
    /// </summary>
    private void GoToMainMenu()
    {
        Machine.ClearSuspendedNodes();
        StateEventDefine.ChangeState.SendEventMessage<GameStart>("GameStart");
    }

#endregion
}