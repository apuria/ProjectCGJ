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
    /// 每回合都需要执行的 buff 分支列表（isEveryRound = true 的 MoreItemBranch）
    /// </summary>
    private List<MoreItemBranch> everyRoundBuffs = new();

    /// <summary>
    /// 当前回合的 round++ / ApplyEveryRoundBuffs 是否已执行。
    /// 用于防止 NextRound() 被中途事件中断后、返回再次执行 NextRound 时重复递增回合和重复应用 buff。
    /// </summary>
    private bool roundAdvanced = false;

    /// <summary>
    /// 角色初始属性快照，用于百分比 buff 计算的基础参考值（避免每回合叠加时复合膨胀）
    /// </summary>
    private struct InitialStatSnapshot
    {
        public float maxHp;
        public float maxMp;
        public float attack;
        public float shieldValue;
        public float speed;
    }

    private Dictionary<RoleInfo, InitialStatSnapshot> roleInitialStats = new();

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
        eventGroup.AddListener<BattleEventDefine.RestartBattle>(OnHandleEventMessage);

        // 玩家输入事件统一由 BattleState 监听，转发给当前活跃的 PlayerBattleState
        // （避免 PlayerBattleState 实例切换时监听器残留导致的叠加 bug）
        eventGroup.AddListener<BattleEventDefine.SelectSkill>(OnHandlePlayerInputEvent);
        eventGroup.AddListener<BattleEventDefine.SelectEnemy>(OnHandlePlayerInputEvent);
        eventGroup.AddListener<BattleEventDefine.SelectAlly>(OnHandlePlayerInputEvent);
        eventGroup.AddListener<BattleEventDefine.PlayerDefend>(OnHandlePlayerInputEvent);

        UIMgr.Instance.ShowPanel<BattlePanel>(isSync: true);
    }

    public override void OnEnter()
    {
        // 播放战斗专属背景音乐，未配置则播放默认音乐
        string bgm = string.IsNullOrEmpty(battleSetting.bgmName) ? "DefaultBGM" : battleSetting.bgmName;
        MusicEventDefine.PlayBGM.SendEventMessage(bgm);

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
                // 仅当回合队列为空时才初始化新回合（首次进入）
                // 从对话/分支恢复时队列中仍有未完成的行动者，直接调用 StartTurn 继续
                if (roundQueue.Count == 0)
                {
                    NextRound();
                }
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
        PlayerBattleState.Current = null;
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
            if (!isBattleEnded)
            {
                if (!isWaitingTurnDelay)
                {
                    isWaitingTurnDelay = true;
                    turnDelay = delayMsg.delay;
                }
                else
                {
                    // 已有延迟在等待中：取较大值，确保当前特效有足够时间播放
                    // 防止多个 EnemeyActionDelay 同时到达时回合过早推进
                    if (delayMsg.delay > turnDelay)
                        turnDelay = delayMsg.delay;
                }
            }
        }
        else if(message is BattleEventDefine.RestartBattle)
        {
            RestartBattle();
        }
    }

    /// <summary>
    /// 转发玩家输入事件给当前活跃的 PlayerBattleState。
    /// 统一由 BattleState 监听，避免 PlayerBattleState 实例切换时 EventGroup 残留导致叠加 bug。
    /// </summary>
    private void OnHandlePlayerInputEvent(IEventMessage message)
    {
        var current = PlayerBattleState.Current;
        if (current == null) return;

        if (message is BattleEventDefine.SelectSkill skillMsg)
            current.HandleSelectSkill(skillMsg.skill);
        else if (message is BattleEventDefine.SelectEnemy enemyMsg)
            current.HandleSelectEnemy(enemyMsg.enemyIndex);
        else if (message is BattleEventDefine.SelectAlly allyMsg)
            current.HandleSelectAlly(allyMsg.allyIndex);
        else if (message is BattleEventDefine.PlayerDefend)
            current.HandleDefend();
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

        // 清空每回合 buff 列表，防止重复 InitBattle 调用导致累积
        everyRoundBuffs.Clear();

        foreach (var enemy in battleSetting.enemies)
        {
            if (enemy == null) continue;
            var instance = Object.Instantiate(enemy);
            EnsureStatDefaults(instance);
            enemyList.Add(instance);
        }

        if (GameManager.Instance.mainRole != null)
        {
            var mainRoleInst = Object.Instantiate(GameManager.Instance.mainRole);
            EnsureStatDefaults(mainRoleInst);
            roleList.Add(mainRoleInst);
        }
        else
        {
            Debug.LogError("BattleState.InitBattle: GameManager.Instance.mainRole 未配置！请在 GameManager 上拖入主角 RoleInfo 资产。");
        }

        var playerData = GameManager.Instance.PlayerData;
        var branchList = playerData?.branchList;

        if (playerData == null)
        {
            Debug.LogError("BattleState.InitBattle: PlayerData 未初始化，无法处理多角色分支。");
        }
        else
        {
            foreach(var roleBranch in battleSetting.moreRoleBranches)
            {
                if (roleBranch == null) continue;

                var branches = playerData.branchList;
                bool hasKey = branches.ContainsKey(roleBranch.branchId);
                string branchVal = hasKey ? branches[roleBranch.branchId] : null;

                // 边界检查 roleIndex
                if (roleBranch.roleIndex < 0 || roleBranch.roleIndex >= GameManager.Instance.roleList.Count)
                {
                    Debug.LogError($"BattleState.InitBattle: moreRoleBranches 的 roleIndex={roleBranch.roleIndex} 越界，GameManager.roleList 只有 {GameManager.Instance.roleList.Count} 个元素。");
                    continue;
                }

                var roleAsset = GameManager.Instance.roleList[roleBranch.roleIndex];
                if (roleAsset == null)
                {
                    Debug.LogError($"BattleState.InitBattle: GameManager.roleList[{roleBranch.roleIndex}] 为 null。");
                    continue;
                }

                // 两个都为空直接添加，否则按选择值匹配
                if (string.IsNullOrEmpty(branchVal) && string.IsNullOrEmpty(roleBranch.choose))
                {
                    var inst = Object.Instantiate(roleAsset);
                    EnsureStatDefaults(inst);
                    roleList.Add(inst);
                }
                else if (hasKey && branchVal == roleBranch.choose)
                {
                    var inst = Object.Instantiate(roleAsset);
                    EnsureStatDefaults(inst);
                    roleList.Add(inst);
                }
            }
        }

        // === 快照角色初始属性，用于百分比 buff 计算 ===
        // 在应用任何 buff 之前保存，避免每回合叠加时基于已被修改的值计算导致复合膨胀
        foreach (var role in roleList)
        {
            if (role != null && !roleInitialStats.ContainsKey(role))
            {
                roleInitialStats[role] = new InitialStatSnapshot
                {
                    maxHp       = role.maxHp.value,
                    maxMp       = role.maxMp.value,
                    attack      = role.attack.value,
                    shieldValue = role.shieldValue.value,
                    speed       = role.speed.value,
                };
            }
        }

        // === 处理道具分支（moreItemBranches）：根据分支选择为角色添加 Buff ===
        // 放在 else 外面，因为无条件 buff（branchId 为空）即使 PlayerData 为 null 也应生效
        foreach (var itemBranch in battleSetting.moreItemBranches)
        {
            if (itemBranch == null || itemBranch.buffInfo == null) continue;

            if (!IsItemBranchMatch(itemBranch, branchList)) continue;

            if (itemBranch.isEveryRound)
            {
                // 每回合 buff：只注册到列表，不立即应用。
                // 统一由 NextRound() → ApplyEveryRoundBuffs() 负责每回合应用，
                // 避免 InitBattle 立即应用 + 首次 NextRound 再次应用导致重复。
                everyRoundBuffs.Add(itemBranch);
            }
            else
            {
                // 非每回合 buff：仅在战斗初始化时应用一次，持续生效
                ApplyMoreItemBranchBuff(itemBranch);
            }
        }

        if (roleList.Count == 0)
        {
            Debug.LogError("BattleState.InitBattle: 没有成功加载任何角色！请检查 GameManager 的 mainRole 和 roleList 配置，以及 BattleSetting 的 moreRoleBranches。");
        }

        // 重置敌方 AI 概率
        EnemyAI.Reset();

        // 发送初始化战斗UI事件：展示角色和敌人，隐藏按钮和Action界面
        BattleEventDefine.InitBattleUI.SendEventMessage(roleList, enemyList, battleSetting.Background);
    }

    /// <summary>
    /// 为未在 Editor 中配置数值的 Stat 提供默认值，
    /// 避免因 Stat 未序列化导致 hp/speed 全为 0 使战斗无法进行
    /// </summary>
    private static void EnsureStatDefaults(BaseInfo info)
    {
        if (info == null) return;
        // maxHp/hp：0 或负值视为未配置（0 血角色无意义），始终重置为满血
        if (info.maxHp.value <= 0)  info.maxHp.value = 100;
        info.hp.value = info.maxHp.value;
        // 其余属性：仅负值视为未配置，0 是合法值（如无 MP、0 攻击、0 护盾），始终重置为满蓝
        if (info.maxMp.value < 0)  info.maxMp.value = 50;
        info.mp.value = info.maxMp.value;
        if (info.attack.value < 0) info.attack.value = 20;
        if (info.speed.value < 0)  info.speed.value = 10;
        if (info.shieldValue.value < 0) info.shieldValue.value = 10;
    }

    /// <summary>
    /// 开始当前顺位的行动（Peek，不 Dequeue），自动清理所有已死亡的行动者
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

            // 清理队列中所有已死亡的角色/敌人（包括非队首位置的中途死亡者）
            int beforeCount = roundQueue.Count;
            PurgeDeadFromQueue();
            if (roundQueue.Count != beforeCount)
            {
                // 有死者被清除 → 更新完整队列（UI 不再显示死者）
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

        // 清理队列中在本回合内死亡的角色/敌人
        PurgeDeadFromQueue();

        // 同步更新完整队列，确保传给下一个 BattleState 的 actionQueue 反映当前状态
        currentFullQueue = new List<BaseInfo>(roundQueue);

        StartTurn();
    }

    /// <summary>
    /// 从回合队列中移除所有已死亡的角色/敌人（本回合中途有单位死亡时清理队列）
    /// </summary>
    private void PurgeDeadFromQueue()
    {
        var alive = new Queue<BaseInfo>();
        while (roundQueue.Count > 0)
        {
            var info = roundQueue.Dequeue();
            bool isDead = (info is RoleInfo r && r.hp.value <= 0) ||
                          (info is EnemyInfo e && e.hp.value <= 0);
            if (!isDead)
                alive.Enqueue(info);
        }
        roundQueue = alive;
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
        1. 递增回合并应用每回合 buff（仅当本回合尚未递增过时执行）
        2. 检查中途事件
        3. 清空回合队列
        4. 计算角色和敌人的行动顺序
        5. 构建完整行动队列
        6. 调用 StartTurn()
        */

        // 仅当本轮尚未递增时才执行，防止中途事件中断 NextRound 后、
        // 再次进入 NextRound 时重复递增回合和重复应用 buff
        if (!roundAdvanced)
        {
            round++;
            ApplyEveryRoundBuffs();
            roundAdvanced = true;
        }

        CheckMidEvent();
        // 若 CheckMidEvent 触发了中途事件（状态切换），roundAdvanced 保持 true，
        // 当前 NextRound 的后续代码不会执行。事件返回后将重新进入 NextRound，
        // 此时 !roundAdvanced 为 false，跳过重复递增。

        roundAdvanced = false;

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

    #region MoreItemBranch Buff 系统

    /// <summary>
    /// 检查 MoreItemBranch 的分支条件是否满足
    /// </summary>
    /// <param name="itemBranch">道具分支配置</param>
    /// <param name="branchList">玩家分支选择记录</param>
    /// <returns>true = 满足条件，应添加 buff</returns>
    private bool IsItemBranchMatch(MoreItemBranch itemBranch, Dictionary<string, string> branchList)
    {
        // branchId 为空 → 无条件添加 buff（规则5，优先级最高）
        if (string.IsNullOrEmpty(itemBranch.branchId)) return true;

        // 有 branchId 但没有 PlayerData → 无法匹配
        if (branchList == null) return false;

        bool hasKey = branchList.ContainsKey(itemBranch.branchId);
        string branchVal = hasKey ? branchList[itemBranch.branchId] : null;

        // 两个都为空 → 直接匹配（规则1）
        if (string.IsNullOrEmpty(branchVal) && string.IsNullOrEmpty(itemBranch.choose))
            return true;

        // 选择值匹配 → 匹配（规则1）
        if (hasKey && branchVal == itemBranch.choose)
            return true;

        return false;
    }

    /// <summary>
    /// 根据 MoreItemBranch 配置为目标角色应用 buff
    /// 如果 isForAllRole 为 true，为全体友军添加；否则按 roleIndex 为特定角色添加
    /// </summary>
    private void ApplyMoreItemBranchBuff(MoreItemBranch itemBranch)
    {
        if (itemBranch.buffInfo == null) return;

        if (itemBranch.isForAllRole)
        {
            // 规则2：为全体友军增加 buff
            foreach (var role in roleList)
            {
                if (role != null && role.hp.value > 0)
                    ApplyBuffToRole(role, itemBranch.buffInfo);
            }
        }
        else
        {
            // 规则3：按 roleIndex 为特定角色添加 buff
            if (itemBranch.roleIndex < 0 || itemBranch.roleIndex >= roleList.Count)
            {
                Debug.LogError($"BattleState.ApplyMoreItemBranchBuff: roleIndex={itemBranch.roleIndex} 越界，roleList 只有 {roleList.Count} 个元素。buffName={itemBranch.buffInfo.buffName}");
                return;
            }

            var role = roleList[itemBranch.roleIndex];
            if (role != null && role.hp.value > 0)
                ApplyBuffToRole(role, itemBranch.buffInfo);
        }
    }

    /// <summary>
    /// 为单个角色应用 buff 效果。
    /// HP/MP 类型：通过事件回血/回蓝/扣血/扣蓝（正值恢复、负值削减，支持每回合重复执行）。
    /// 其他类型（ATK/DEF/SPD/MAXHP/MAXMP）：直接修改属性值，
    /// 绕过 AddBuff 的 Dictionary 去重，使 isEveryRound 与 HP/MP 行为对齐。
    /// </summary>
    private void ApplyBuffToRole(RoleInfo role, BuffInfo buff)
    {
        if (role == null || buff == null) return;

        int idx = roleList.IndexOf(role);
        if (idx < 0) return;

        float actualValue = buff.isPercentage
            ? GetBuffPercentageValue(role, buff)
            : buff.value;

        switch (buff.buffType)
        {
            case StatType.HP:
            {
                int changeValue = Mathf.RoundToInt(actualValue);
                // 正值 = 治疗，负值 = 伤害（如中毒扣百分比血量）
                if (changeValue != 0)
                    BattleEventDefine.RoleHpChange.SendEventMessage(idx, -changeValue);
                break;
            }
            case StatType.MP:
            {
                int changeValue = Mathf.RoundToInt(actualValue);
                // 正值 = 回蓝，负值 = 扣蓝
                if (changeValue != 0)
                    BattleEventDefine.RoleMpChange.SendEventMessage(idx, -changeValue);
                break;
            }
            default:
                // ATK / DEF / SPD / MAXHP / MAXMP：
                // 直接修改属性值，不经过 AddBuff（其 Dictionary 会阻止同 buff 二次生效），
                // 使 isEveryRound 的 stat buff 能每回合正常应用。
                role.BuffChangeValue(buff.buffType, actualValue);
                break;
        }
    }

    /// <summary>
    /// 计算百分比 buff 的实际数值。
    /// 所有属性均基于战斗开始时的初始属性快照，避免每回合叠加时复合膨胀。
    /// </summary>
    private float GetBuffPercentageValue(RoleInfo role, BuffInfo buff)
    {
        return buff.buffType switch
        {
            StatType.HP or StatType.MAXHP => (roleInitialStats.TryGetValue(role, out var snap)
                ? snap.maxHp
                : role.maxHp.value) * buff.value,
            StatType.MP or StatType.MAXMP => (roleInitialStats.TryGetValue(role, out var snap)
                ? snap.maxMp
                : role.maxMp.value) * buff.value,
            StatType.ATK => (roleInitialStats.TryGetValue(role, out var snap)
                ? snap.attack
                : role.attack.value) * buff.value,
            StatType.DEF => (roleInitialStats.TryGetValue(role, out var snap)
                ? snap.shieldValue
                : role.shieldValue.value) * buff.value,
            StatType.SPD => (roleInitialStats.TryGetValue(role, out var snap)
                ? snap.speed
                : role.speed.value) * buff.value,
            _ => buff.value,
        };
    }

    /// <summary>
    /// 每回合开始时，应用所有 isEveryRound 的 buff（规则4）
    /// 在 NextRound() 中调用
    /// </summary>
    private void ApplyEveryRoundBuffs()
    {
        if (everyRoundBuffs.Count == 0) return;

        foreach (var itemBranch in everyRoundBuffs)
        {
            if (itemBranch == null || itemBranch.buffInfo == null) continue;
            ApplyMoreItemBranchBuff(itemBranch);
        }
    }

    #endregion

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

    /// <summary>
    /// 重新开始当场战斗（仅重置战斗状态，不经过 NodeGameEvent 整个节点流程）。
    /// 用于"逃跑"按钮和战斗失败后的"重来"按钮。
    /// </summary>
    private void RestartBattle()
    {
        // 1. 清理旧的子状态机（防止残留的 PlayerBattleState/EnemyBattleState 实例）
        //    同时清除静态 Current 引用，防止旧 PlayerBattleState 实例无法被 GC
        PlayerBattleState.Current = null;
        battleMachine = new StateMachine();

        // 2. 清空战斗数据
        roleList.Clear();
        enemyList.Clear();
        roundQueue.Clear();
        roundList.Clear();
        currentFullQueue = null;

        // 3. 清空 buff / 中途事件状态
        everyRoundBuffs.Clear();
        roleInitialStats.Clear();
        triggeredMidEventIndices.Clear();
        EnemyAI.Reset();

        // 4. 重置所有状态标记
        round = 0;
        isBattleStart = false;
        isBattleEnded = false;
        isLoadPhase = false;
        isWaitingTurnDelay = false;
        roundAdvanced = false;
        turnDelay = 0f;
        loadDelay = 0f;

        // 5. 重新初始化战斗数据 + 更新立绘
        isBattleStart = true;
        InitBattle();

        // 6. 设置准备阶段计时器（与 OnEnter 首次进入流程对齐）
        isLoadPhase = true;
        loadDelay = 1.2f;

        // 7. 处理战斗开始事件（如对话/分支），
        //    事件返回后由 OnEnter else 分支恢复准备阶段
        if (battleSetting.startEvent != EBattleStartEvent.None)
        {
            StartEvent(battleSetting.startEvent);
        }
    }

#endregion
}