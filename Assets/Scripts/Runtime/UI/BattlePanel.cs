using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : BasePanel
{
//
/*
1.
*/

#region 属性

    public Image BG;
    public Button btnSetting;
    public SettingPanel settingPanel;
    public Button btnState;
    public Button btnReturn;
    public Button btnQuit;
    public Button btnAttack;
    public Button btnDefend;
    public Button btnSkill;
    public Button btnRun;

    public Transform ActionControll;

    public Button btnBattleEnd;

    public ActionToast enemyActionToast;

    /// <summary>
    /// 记录战斗结果（true = 胜利, false = 失败）
    /// </summary>
    private bool battleIsWin;

    /// <summary>
    /// 战斗结束按钮的文本（TextMeshProUGUI），根据胜负显示不同文字
    /// </summary>
    private TextMeshProUGUI battleEndText;

    /// <summary>
    /// 战斗结束图标延迟显示的时间（秒），需大于加载阶段 1.2s
    /// </summary>
    private const float battleEndDelay = 2.0f;

    /// <summary>
    /// 待确认的技能（等待选择敌人目标）
    /// </summary>
    private SkillInfo pendingSkill;

    /// <summary>
    /// 当前行动的角色
    /// </summary>
    private RoleInfo currentActingRole;

    /// <summary>
    /// 当前行动角色在 roleList 中的索引
    /// </summary>
    private int currentActingRoleIdx = -1;

    /// <summary>
    /// 当前显示的角色列表（重排序后）和原始列表，用于 DamageEffect 定位正确 slot
    /// </summary>
    private List<RoleInfo> currentDisplayRoles;
    private List<RoleInfo> currentOriginalRoles;

    public HPandMP hpadnmp;
    public ActionList actionList;
    public Enemies enemies;
    public Roles roles;
    public Skills skills;
    public RoleState roleState;
    public EnemyChoose enemyChoose;

    public List<Slider> enemyHpSliders;

    /// <summary>
    /// 当前敌人列表（用于刷新 enemyHpSliders）
    /// </summary>
    private List<EnemyInfo> currentEnemyList;

    /// <summary>
    /// 当前队友列表（用于在队友选择模式下刷新 enemyHpSliders）
    /// </summary>
    private List<RoleInfo> currentAllyList;

    /// <summary>
    /// 防御特效预制体（在 Inspector 中拖入）
    /// </summary>
    public GameObject defendEffect;

    public Image EndIcon;

    public Sprite WinIcon;
    public Sprite LoseIcon;

    private GameObject hurtNumPrefab;

#endregion

#region 生命周期

    protected void Awake()
    {
        eventGroup = new();

        // 核心事件监听放在 Awake，确保 InitBattle 发 InitBattleUI 时已注册
        eventGroup.AddListener<BattleEventDefine.InitBattleUI>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.UpdateActionSide>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.BattleEnd>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.SelectSkill>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.SelectEnemy>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.SelectAlly>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.RoleDamageEffect>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.EnemyDamageEffect>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.HealEffect>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.EnemyActionToast>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.DefendEffect>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.RoleHpChange>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.RoleMpChange>(OnHandleEventMessage);
        eventGroup.AddListener<BattleEventDefine.EnemyHpChange>(OnHandleEventMessage);

        // 立即隐藏所有 UI，等正式开战再逐步显示
        btnSetting.gameObject.SetActive(false);
        btnState.gameObject.SetActive(false);
        btnReturn.gameObject.SetActive(false);
        btnQuit.gameObject.SetActive(false);
        if (ActionControll != null) ActionControll.gameObject.SetActive(false);
        btnBattleEnd.gameObject.SetActive(false);
        if (EndIcon != null) EndIcon.gameObject.SetActive(false);
        battleEndText = btnBattleEnd.GetComponentInChildren<TextMeshProUGUI>();
        if (actionList != null) actionList.gameObject.SetActive(false);
        if (enemyChoose != null) enemyChoose.gameObject.SetActive(false);
        if (skills != null) skills.gameObject.SetActive(false);
        if (hpadnmp != null) hpadnmp.gameObject.SetActive(false);
        if (roleState != null) roleState.gameObject.SetActive(false);
        if (enemyActionToast != null) enemyActionToast.gameObject.SetActive(false);

        if (roles != null) roles.gameObject.SetActive(false);

        
    }

    protected void Start()
    {

        btnBattleEnd.onClick.AddListener(OnClickBattleEnd);

        hurtNumPrefab = Resources.Load<GameObject>("UI/HurtNum");

        // btnSetting 先不配置

        // btnState: 切换 RoleState 界面显隐
        btnState.onClick.AddListener(() =>
        {
            roleState.gameObject.SetActive(!roleState.gameObject.activeSelf);
        });

        btnSetting.onClick.AddListener(() =>
        {
            settingPanel.gameObject.SetActive(true);
            settingPanel.ShowMe();
        });

        // btnRun: 逃跑按钮 → 重新开始当场战斗
        btnRun.onClick.AddListener(() =>
        {
            SceneEventDefine.NodeGame.SendEventMessage();
        });

        // btnReturn: 提示确认是否返回地图
        btnReturn.onClick.AddListener(() =>
        {
            TipPanelEventDefine.ShowTip.SendEventMessage("确定要返回地图吗？", "确认",
                () => StateEventDefine.ChangeState.SendEventMessage<MapState>("MapState"), "取消", null);
        });

        // btnQuit: 提示确认是否退出游戏
        btnQuit.onClick.AddListener(() =>
        {
            TipPanelEventDefine.ShowTip.SendEventMessage("确定要退出游戏吗？", "确认",
                () => GameEventDefine.QuitGame.SendEventMessage(), "取消", null);
        });

        // roleState / enemyChoose / skills 上的 controller 按钮：按下后隐藏自身
        if (roleState != null && roleState.controller != null)
            roleState.controller.onClick.AddListener(() => roleState.gameObject.SetActive(false));
        if (enemyChoose != null && enemyChoose.controller != null)
            enemyChoose.controller.onClick.AddListener(() =>
            {
                // 取消选择敌人：隐藏 enemyChoose，恢复操作按钮
                enemyChoose.gameObject.SetActive(false);
                if (ActionControll != null) ActionControll.gameObject.SetActive(true);
                pendingSkill = null;
            });
        if (skills != null && skills.controller != null)
            skills.controller.onClick.AddListener(() => skills.gameObject.SetActive(false));

        // ----- 行动按钮业务逻辑 -----

        // btnAttack: 普通攻击 → 非AOE则弹出敌人选择
        btnAttack.onClick.AddListener(() =>
        {
            if (currentActingRole?.normalAttack != null)
            {
                pendingSkill = currentActingRole.normalAttack;
                BattleEventDefine.SelectSkill.SendEventMessage(pendingSkill);
            }
        });

        // btnSkill: 打开技能选择面板（具体技能由 skillSlot 按钮确定）
        btnSkill.onClick.AddListener(() =>
        {
            if (skills != null) skills.gameObject.SetActive(true);
        });

        // btnDefend: 防御 → 由 PlayerBattleState 处理（回血 + 临时护盾）
        btnDefend.onClick.AddListener(() =>
        {
            HideActionButtons();
            BattleEventDefine.PlayerDefend.SendEventMessage();
        });

        // 设置异形按钮的 alphaHitTestMinimumThreshold，使透明通道部分不响应射线检测
        SetupAlphaHitThreshold(btnAttack);
        SetupAlphaHitThreshold(btnDefend);
        SetupAlphaHitThreshold(btnSkill);
        SetupAlphaHitThreshold(btnRun);

    }

    protected void OnDestroy()
    {
        eventGroup.RemoveAllListener();
    }
#endregion

#region 逻辑控制

    public override void HideMe()
    {

    }

    public override void ShowMe()
    {

    }

    /// <summary>
    /// 按 index 顺序刷新敌人血量 Slider
    /// </summary>
    private void RefreshEnemyHpSliders()
    {
        if (enemyHpSliders == null || currentEnemyList == null) return;

        for (int i = 0; i < enemyHpSliders.Count; i++)
        {
            if (enemyHpSliders[i] == null) continue;

            if (i < currentEnemyList.Count && currentEnemyList[i] != null)
            {
                var enemy = currentEnemyList[i];
                float hpPct = enemy.maxHp.value > 0 ? enemy.hp.value / enemy.maxHp.value : 0f;
                enemyHpSliders[i].value = hpPct;
                enemyHpSliders[i].gameObject.SetActive(enemy.hp.value > 0);
            }
            else
            {
                enemyHpSliders[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 按 index 顺序刷新队友血量 Slider（在队友选择模式下使用）
    /// </summary>
    private void RefreshAllyHpSliders()
    {
        if (enemyHpSliders == null || currentAllyList == null) return;

        for (int i = 0; i < enemyHpSliders.Count; i++)
        {
            if (enemyHpSliders[i] == null) continue;

            if (i < currentAllyList.Count && currentAllyList[i] != null)
            {
                var role = currentAllyList[i];
                float hpPct = role.maxHp.value > 0 ? role.hp.value / role.maxHp.value : 0f;
                enemyHpSliders[i].value = hpPct;
                enemyHpSliders[i].gameObject.SetActive(role.hp.value > 0);
            }
            else
            {
                enemyHpSliders[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 设置 Button 上 Image 的 alphaHitTestMinimumThreshold，使图片透明区域不响应射线检测
    /// </summary>
    /// <param name="btn">目标按钮</param>
    /// <param name="threshold">Alpha 阈值，默认 0.5</param>
    private void SetupAlphaHitThreshold(Button btn, float threshold = 0.5f)
    {
        if (btn == null) return;
        var image = btn.GetComponent<Image>();
        if (image != null)
        {
            image.alphaHitTestMinimumThreshold = threshold;
        }
    }

    /// <summary>
    /// 隐藏所有操作按钮（ActionControll 为所有操作按钮的父节点）
    /// 同时关闭 enemyChoose 和 skills 子面板以防残留
    /// </summary>
    private void HideActionButtons()
    {
        if (ActionControll != null)
            ActionControll.gameObject.SetActive(false);
        if (enemyChoose != null)
            enemyChoose.gameObject.SetActive(false);
        if (skills != null)
            skills.gameObject.SetActive(false);
    }

#endregion

#region 事件监听

    public void OnHandleEventMessage(IEventMessage message)
    {
        if (message is BattleEventDefine.InitBattleUI initMsg)
        {
            HandleInitBattleUI(initMsg);
        }
        else if (message is BattleEventDefine.UpdateActionSide actionSideMsg)
        {
            HandleUpdateActionSide(actionSideMsg);
        }
        else if (message is BattleEventDefine.BattleEnd battleEndMsg)
        {
            HandleBattleEnd(battleEndMsg);
        }
        else if (message is BattleEventDefine.SelectSkill selectSkillMsg)
        {
            HandleSelectSkill(selectSkillMsg);
        }
        else if (message is BattleEventDefine.SelectEnemy selectEnemyMsg)
        {
            HandleSelectEnemy(selectEnemyMsg);
        }
        else if (message is BattleEventDefine.SelectAlly selectAllyMsg)
        {
            HandleSelectAlly(selectAllyMsg);
        }
        else if (message is BattleEventDefine.RoleDamageEffect roleDmgMsg)
        {
            HandleRoleDamageEffect(roleDmgMsg);
        }
        else if (message is BattleEventDefine.EnemyDamageEffect enemyDmgMsg)
        {
            HandleEnemyDamageEffect(enemyDmgMsg);
        }
        else if (message is BattleEventDefine.HealEffect healEffectMsg)
        {
            HandleHealEffect(healEffectMsg);
        }
        else if (message is BattleEventDefine.EnemyActionToast actionToastMsg)
        {
            HandleEnemyActionToast(actionToastMsg);
        }
        else if (message is BattleEventDefine.DefendEffect defendEffectMsg)
        {
            HandleDefendEffect(defendEffectMsg);
        }
        else if (message is BattleEventDefine.RoleHpChange rhcMsg)
        {
            if (rhcMsg.idx == currentActingRoleIdx)
                RefreshHPandMP();
            // 同步刷新 RoleState 面板（非行动角色也需更新 HP）
            if (currentDisplayRoles != null && roleState != null)
                roleState.UpdateUI(currentDisplayRoles);
        }
        else if (message is BattleEventDefine.RoleMpChange rmcMsg)
        {
            if (rmcMsg.idx == currentActingRoleIdx)
                RefreshHPandMP();
            // 同步刷新 RoleState 面板（非行动角色也需更新 MP）
            if (currentDisplayRoles != null && roleState != null)
                roleState.UpdateUI(currentDisplayRoles);
        }
        else if (message is BattleEventDefine.EnemyHpChange)
        {
            // 敌人 HP 变更后刷新 HP Slider（BattleState 已先处理 HP 修改）
            RefreshEnemyHpSliders();
        }
    }

    /// <summary>
    /// 初始化战斗UI：展示角色和敌人列表，隐藏所有按钮和Action界面
    /// 类似战斗开始前的伪加载
    /// </summary>
    private void HandleInitBattleUI(BattleEventDefine.InitBattleUI msg)
    {
        // 1. 隐藏所有按钮
        btnSetting.gameObject.SetActive(false);
        btnState.gameObject.SetActive(false);
        btnReturn.gameObject.SetActive(false);
        btnQuit.gameObject.SetActive(false);
        if (ActionControll != null) ActionControll.gameObject.SetActive(false);
        btnBattleEnd.gameObject.SetActive(false);

        // 2. 隐藏Action相关界面
        if (actionList != null) actionList.gameObject.SetActive(false);
        if (enemyChoose != null) enemyChoose.gameObject.SetActive(false);
        if (skills != null) skills.gameObject.SetActive(false);
        if (hpadnmp != null) hpadnmp.gameObject.SetActive(false);
        if (roleState != null) roleState.gameObject.SetActive(false);

        // 3. 展示角色：按速度排序，第一顺位显示 CloseUp 特写
        if (msg.roleList != null && roles != null)
        {
            roles.gameObject.SetActive(true);
            var sortedRoles = new List<RoleInfo>(msg.roleList);
            sortedRoles.Sort((a, b) => b.speed.value.CompareTo(a.speed.value));
            roles.UpdateUIWithCloseUp(sortedRoles);
        }

        // 4. 展示敌人
        if (msg.enemyList != null && enemies != null)
            enemies.UpdateUI(msg.enemyList);

        // 5. 保存敌人列表并刷新 HP Slider
        currentEnemyList = msg.enemyList;
        RefreshEnemyHpSliders();

        // 6. 设置自定义背景图（为空则保留默认背景）
        if (msg.customBackground != null && BG != null)
            BG.sprite = msg.customBackground;

        // 7. 失活所有 Tip
        if (enemies != null) enemies.DeactivateAllTips();
    }

    /// <summary>
    /// 处理更新行动方UI事件
    /// 从 actionQueue 第一顺位类型自行判断阵营
    /// </summary>
    private void HandleUpdateActionSide(BattleEventDefine.UpdateActionSide msg)
    {
        if (msg.actionQueue == null || msg.actionQueue.Count == 0)
            return;

        BaseInfo currentActor = msg.actionQueue[0];

        // 行动开始时，恢复所有界面元素（btnBattleEnd 仅在战斗结束时显示）
        // RoleState/Skills/EnemyChoose 默认隐藏，由玩家通过对应按钮手动打开
        btnSetting.gameObject.SetActive(true);
        btnState.gameObject.SetActive(true);
        btnReturn.gameObject.SetActive(true);
        btnQuit.gameObject.SetActive(true);
        if (actionList != null) actionList.gameObject.SetActive(true);
        if (hpadnmp != null) hpadnmp.gameObject.SetActive(true);

        // 清除待确认状态
        pendingSkill = null;

        if (currentActor is RoleInfo actingRole)
        {
            HandlePlayerTurn(msg, actingRole);
        }
        else if (currentActor is EnemyInfo)
        {
            HandleEnemyTurn(msg);
        }
    }

    /// <summary>
    /// 我方行动时：显示按钮，更新角色立绘（第一位CloseUp），更新HP/MP/技能/RoleState
    /// </summary>
    private void HandlePlayerTurn(BattleEventDefine.UpdateActionSide msg, RoleInfo actingRole)
    {
        // 0. 停掉所有残留协程（敌方回合的抖动、伤害数字、死亡变黑等延迟特效），
        //    防止它们在我方操作界面弹出时干扰显示
        StopAllCoroutines();

        currentActingRole = actingRole;

        // 1. 显示所有操作按钮（ActionControll 为按钮父节点）
        if (ActionControll != null) ActionControll.gameObject.SetActive(true);

        // 2. 敌人选择默认隐藏（等选择非AOE技能后再显示）
        if (enemyChoose != null) enemyChoose.gameObject.SetActive(false);

        // 3. 构建角色显示列表：行动角色（ActionList第一顺位）始终在第一位
        currentOriginalRoles = msg.roleList;
        List<RoleInfo> reorderedRoles = BuildDisplayRoles(msg.roleList, actingRole);
        currentDisplayRoles = reorderedRoles;
        // currentActingRoleIdx 在 msg.roleList 中的原始索引（用于 HP/MP 变更事件定位）
        currentActingRoleIdx = msg.roleList != null ? msg.roleList.IndexOf(actingRole) : -1;
        RoleInfo firstRole = reorderedRoles.Count > 0 ? reorderedRoles[0] : null;

        // 4. 更新角色立绘：第一位用 CloseUp 特写，其他正常
        if (roles != null)
        {
            roles.gameObject.SetActive(true);
            roles.UpdateUIWithCloseUp(reorderedRoles);
        }

        // 5. 更新 HPandMP：显示第一位角色的 HP/MP
        if (firstRole != null && hpadnmp != null)
        {
            float hpPercentage = firstRole.maxHp.value > 0 ? firstRole.hp.value / firstRole.maxHp.value : 0f;
            float mpPercentage = firstRole.maxMp.value > 0 ? firstRole.mp.value / firstRole.maxMp.value : 0f;
            hpadnmp.UpdateUI(hpPercentage, mpPercentage);
        }

        // 6. 更新技能列表数据（默认隐藏，玩家点击 btnSkill 时打开）
        //    始终调用 UpdateUI 以清除上一角色的残留技能槽数据
        if (skills != null)
        {
            if (firstRole != null && firstRole.skills != null && firstRole.skills.Count > 0)
                skills.UpdateUI(firstRole.skills);
            else
                skills.UpdateUI(new List<SkillInfo>());
        }

        // 6.1 根据当前 MP 刷新攻击/技能按钮的可交互状态（蓝量不足则变灰不可点击）
        RefreshSkillButtonStates();

        // 7. 更新 RoleState：显示除第一位角色外的其他角色状态
        if (roleState != null) roleState.UpdateUI(reorderedRoles);

        // 8. 更新行动队列UI（跳过第一顺位）
        if (actionList != null) actionList.UpdateUI(msg.actionQueue);

        // 9. 更新敌人列表
        if (msg.enemyList != null)
        {
            if (enemies != null) enemies.UpdateUI(msg.enemyList);
            if (enemyChoose != null) enemyChoose.UpdateUI(msg.enemyList);
        }

        // 10. 保存敌人列表并刷新 HP Slider
        currentEnemyList = msg.enemyList;
        RefreshEnemyHpSliders();

        // 11. 失活所有 Tip（我方回合不需要显示敌人行动提示）
        if (enemies != null) enemies.DeactivateAllTips();
    }

    /// <summary>
    /// 敌方行动时：隐藏操作按钮和技能，敌方站位不变，激活当前行动敌人的 Tip
    /// </summary>
    private void HandleEnemyTurn(BattleEventDefine.UpdateActionSide msg)
    {
        // 1. 隐藏操作按钮及子面板
        HideActionButtons();

        // 2. 角色立绘保持不变（保持上一位行动角色在第一位的 CloseUp 状态）
        // 3. 更新敌人立绘
        if (msg.enemyList != null && enemies != null)
            enemies.UpdateUI(msg.enemyList);

        // 4. 保存敌人列表并刷新 HP Slider
        currentEnemyList = msg.enemyList;
        RefreshEnemyHpSliders();

        // 5. 更新行动队列UI
        if (actionList != null)
            actionList.UpdateUI(msg.actionQueue);

        // 6. 激活当前行动敌人的 Tip
        if (msg.actionQueue != null && msg.actionQueue.Count > 0
            && msg.actionQueue[0] is EnemyInfo actingEnemy
            && msg.enemyList != null && enemies != null)
        {
            int enemyIdx = msg.enemyList.IndexOf(actingEnemy);
            if (enemyIdx >= 0)
                enemies.SetEnemyTipActive(enemyIdx);
        }
    }

    /// <summary>
    /// 战斗结束时：隐藏操作按钮，延迟显示 btnBattleEnd
    /// </summary>
    private void HandleBattleEnd(BattleEventDefine.BattleEnd msg)
    {
        battleIsWin = msg.isWin;

        // 隐藏所有操作按钮和面板，只保留 btnBattleEnd
        HideActionButtons();
        btnSetting.gameObject.SetActive(false);
        btnState.gameObject.SetActive(false);
        btnReturn.gameObject.SetActive(false);
        btnQuit.gameObject.SetActive(false);
        if (actionList != null) actionList.gameObject.SetActive(false);
        if (hpadnmp != null) hpadnmp.gameObject.SetActive(false);
        if (roleState != null) roleState.gameObject.SetActive(false);

        // 失活所有 Tip
        if (enemies != null) enemies.DeactivateAllTips();

        // 延迟显示战斗结束按钮（给玩家时间看清最后的战斗结果）
        StartCoroutine(ShowBattleEndDelayed());
    }

    /// <summary>
    /// 延迟显示战斗结束按钮，并根据胜负设置文字和图标
    /// </summary>
    private IEnumerator ShowBattleEndDelayed()
    {
        yield return new WaitForSeconds(battleEndDelay);

        if (battleEndText != null)
            battleEndText.text = battleIsWin ? "胜利" : "失败";

        // 根据胜负显示对应图标
        if (EndIcon != null)
        {
            EndIcon.sprite = battleIsWin ? WinIcon : LoseIcon;
            EndIcon.gameObject.SetActive(true);
        }

        btnBattleEnd.gameObject.SetActive(true);
    }

    /// <summary>
    /// 选择了技能：如果是AOE直接生效并隐藏操作按钮；
    /// 如果是单目标，根据 targetType 显示敌人选择或队友选择界面
    /// </summary>
    private void HandleSelectSkill(BattleEventDefine.SelectSkill msg)
    {
        pendingSkill = msg.skill;
        if (skills != null) skills.gameObject.SetActive(false);

        if (pendingSkill != null && !pendingSkill.isAOE && enemyChoose != null)
        {
            // 非AOE：隐藏操作按钮（防止误触防御/逃跑/再次攻击），根据目标类型显示对应选择面板
            if (ActionControll != null) ActionControll.gameObject.SetActive(false);

            if (pendingSkill.targetType == SkillTargetType.Ally)
            {
                // 对队友技能：显示队友选择面板（含自身），并刷新血量条为队友血量
                currentAllyList = currentOriginalRoles;
                enemyChoose.UpdateUIForAllies(currentOriginalRoles);
                RefreshAllyHpSliders();
                enemyChoose.gameObject.SetActive(true);
            }
            else
            {
                // 对敌技能：先刷新敌人数据再显示（防止之前被 UpdateUIForAllies 覆盖）
                if (currentEnemyList != null)
                    enemyChoose.UpdateUI(currentEnemyList);
                RefreshEnemyHpSliders();
                enemyChoose.gameObject.SetActive(true);
            }
        }
        else if (pendingSkill != null && pendingSkill.isAOE)
        {
            // AOE：无需选择目标，直接隐藏操作按钮防止重复点击
            HideActionButtons();
        }
        // 如果非AOE但 enemyChoose 为 null：pendingSkill 已设置但无法选目标，
        // 此处不隐藏按钮，让玩家仍可点防御/逃跑来推进回合（由 PlayerBattleState 层兜底）
    }

    /// <summary>
    /// 选择了敌人目标：隐藏敌人选择界面和操作按钮（伤害和 NextTurn 由 PlayerBattleState 处理）
    /// </summary>
    private void HandleSelectEnemy(BattleEventDefine.SelectEnemy msg)
    {
        HideActionButtons();
        pendingSkill = null;
    }

    /// <summary>
    /// 选择了队友目标：隐藏选择界面和操作按钮（治疗和 NextTurn 由 PlayerBattleState 处理）
    /// </summary>
    private void HandleSelectAlly(BattleEventDefine.SelectAlly msg)
    {
        HideActionButtons();
        pendingSkill = null;
    }

    /// <summary>
    /// 治疗特效：在施法者和目标身上播放特效，显示绿色治疗数字
    /// </summary>
    private void HandleHealEffect(BattleEventDefine.HealEffect msg)
    {
        // 施法特效：放在施法者的 VFXPoint 上，立即播放
        if (msg.attackEffect != null)
        {
            Transform casterVfx = GetVFXPoint(msg.casterIsPlayer, msg.casterIdx);
            if (casterVfx != null)
                SpawnEffect(msg.attackEffect, casterVfx);
        }

        // 受击特效 + 治疗数字：放在目标的 VFXPoint 上，延迟播放
        Transform targetVfx = GetVFXPoint(msg.targetIsPlayer, msg.targetIdx);
        if (targetVfx != null)
        {
            StartCoroutine(DelayedSpawnHealEffectAndHurtNum(targetVfx, msg.healValue, msg.hitEffect));
        }
    }

    /// <summary>
    /// 根据阵营和索引获取对应 VFXPoint
    /// </summary>
    private Transform GetVFXPoint(bool isPlayer, int idx)
    {
        if (isPlayer)
        {
            if (roles == null) return null;
            if (currentDisplayRoles == null || currentOriginalRoles == null) return null;
            if (idx < 0 || idx >= currentOriginalRoles.Count) return null;
            RoleInfo targetRole = currentOriginalRoles[idx];
            int slotIdx = currentDisplayRoles.IndexOf(targetRole);
            if (slotIdx < 0 || slotIdx >= roles.roleSlots.Count) return null;
            var slot = roles.roleSlots[slotIdx];
            if (slot == null || !slot.gameObject.activeSelf) return null;
            return slot.VFXPoint;
        }
        else
        {
            if (enemies == null) return null;
            if (idx < 0 || idx >= enemies.enemySlots.Count) return null;
            var slot = enemies.enemySlots[idx];
            if (slot == null || !slot.gameObject.activeSelf) return null;
            return slot.VFXPoint;
        }
    }

    /// <summary>
    /// 延迟生成治疗受击特效（与治疗数字同时播放）
    /// </summary>
    private IEnumerator DelayedSpawnHealEffectAndHurtNum(Transform vfxPoint, int healValue,
        GameObject hitEffectPrefab)
    {
        yield return new WaitForSeconds(0.5f);

        // 受击特效：有则播，无则跳过
        if (hitEffectPrefab != null)
        {
            SpawnEffect(hitEffectPrefab, vfxPoint);
        }

        SpawnHealNum(vfxPoint, healValue);
    }

    /// <summary>
    /// 生成治疗数字（绿色）
    /// </summary>
    private void SpawnHealNum(Transform vfxPoint, int healValue)
    {
        if (hurtNumPrefab == null) return;
        GameObject go = Instantiate(hurtNumPrefab, transform);
        go.transform.position = vfxPoint.position;
        var hurtNum = go.GetComponent<HurtNum>();
        if (hurtNum != null)
            hurtNum.SetupHeal(healValue);
    }

    /// <summary>
    /// 刷新当前行动角色的 HP/MP 条
    /// </summary>
    private void RefreshHPandMP()
    {
        if (currentActingRole == null) return;
        float hpPct = currentActingRole.maxHp.value > 0 ? currentActingRole.hp.value / currentActingRole.maxHp.value : 0f;
        float mpPct = currentActingRole.maxMp.value > 0 ? currentActingRole.mp.value / currentActingRole.maxMp.value : 0f;
        hpadnmp.UpdateUI(hpPct, mpPct);

        // MP 变化后同步刷新攻击/技能按钮的可点击状态
        RefreshSkillButtonStates();
    }

    /// <summary>
    /// 根据当前角色的 MP 值，刷新攻击按钮和技能槽的可交互状态。
    /// 蓝量不足以支付 mpCost 的按钮会被设为不可交互（变灰 + 不响应射线）。
    /// 蓝量回复后，足够蓝量的技能/攻击按钮恢复可点击。
    /// </summary>
    private void RefreshSkillButtonStates()
    {
        if (currentActingRole == null) return;
        int currentMp = (int)currentActingRole.mp.value;

        // 刷新普通攻击按钮：无普通攻击或蓝量不足时置灰不可点击
        if (btnAttack != null)
        {
            bool canAttack = currentActingRole.normalAttack != null
                             && currentMp >= currentActingRole.normalAttack.mpCost;
            btnAttack.interactable = canAttack;
        }

        // 刷新技能选择面板中的技能槽
        if (skills != null)
        {
            skills.RefreshInteractable(currentMp);
        }
    }

    /// <summary>
    /// 角色受伤效果（敌方攻击 → 我方受击）：立绘抖动；死亡时变黑
    /// 攻击特效放在攻击方（对应敌人）VFXPoint → 立即播放
    /// 受击特效放在受击方（对应玩家）VFXPoint → 延迟与伤害数字同时播放
    /// </summary>
    private void HandleRoleDamageEffect(BattleEventDefine.RoleDamageEffect msg)
    {
        if (roles == null) return;
        if (currentOriginalRoles == null || currentDisplayRoles == null) return;
        if (msg.idx < 0 || msg.idx >= currentOriginalRoles.Count) return;

        RoleInfo damagedRole = currentOriginalRoles[msg.idx];
        int slotIdx = currentDisplayRoles.IndexOf(damagedRole);
        if (slotIdx < 0 || slotIdx >= roles.roleSlots.Count) return;

        var slot = roles.roleSlots[slotIdx];
        if (slot == null || !slot.gameObject.activeSelf) return;

        // 攻击特效：放在攻击方（对应敌人）的 VFXPoint 上，立即播放
        if (msg.attackEffect != null && enemies != null
            && msg.attackerIdx >= 0 && msg.attackerIdx < enemies.enemySlots.Count)
        {
            var attackerSlot = enemies.enemySlots[msg.attackerIdx];
            if (attackerSlot != null && attackerSlot.gameObject.activeSelf)
            {
                SpawnEffect(msg.attackEffect, attackerSlot.VFXPoint);
            }
        }

        // 受击特效：放在受击方（对应玩家）的 VFXPoint 上，延迟与伤害数字同时播放
        // 立绘 role1 和 role2 受击时 scale * 0.6，role0 保持原 scale
        float hitScale = (msg.idx == 1 || msg.idx == 2) ? 0.6f : 1f;
        StartCoroutine(DelayedSpawnHitEffectAndHurtNum(slot.VFXPoint, msg.hurtValue,
            msg.hitEffect, hitScale));

        // 立绘抖动延迟与受击特效同步
        StartCoroutine(DelayedShakeCoroutine(slot.icon.rectTransform));

        // 死亡变黑延迟与受击特效同步
        if (msg.isDead)
            StartCoroutine(DelayedDeathDarken(slot.icon));
    }

    /// <summary>
    /// 敌人受伤效果（我方攻击 → 敌方受击）：立绘抖动；死亡时变黑
    /// 攻击特效放在攻击方（对应玩家角色）VFXPoint → 立即播放
    /// 受击特效放在受击方（对应被击中敌人）VFXPoint → 延迟与伤害数字同时播放
    /// </summary>
    private void HandleEnemyDamageEffect(BattleEventDefine.EnemyDamageEffect msg)
    {
        if (enemies == null) return;
        if (msg.idx >= 0 && msg.idx < enemies.enemySlots.Count)
        {
            var slot = enemies.enemySlots[msg.idx];
            if (slot == null || !slot.gameObject.activeSelf) return;

            // 攻击特效：放在攻击方（对应玩家角色）的 VFXPoint 上，立即播放
            if (msg.attackEffect != null && roles != null
                && msg.attackerIdx >= 0 && msg.attackerIdx < roles.roleSlots.Count)
            {
                var attackerSlot = roles.roleSlots[msg.attackerIdx];
                if (attackerSlot != null && attackerSlot.gameObject.activeSelf)
                {
                    SpawnEffect(msg.attackEffect, attackerSlot.VFXPoint);
                }
            }

            // 受击特效：放在对应被击中敌人的 VFXPoint 上，延迟与伤害数字同时播放
            // 敌人受击特效保持原 scale
            Transform hitVfxPoint = slot.VFXPoint;
            StartCoroutine(DelayedSpawnHitEffectAndHurtNum(hitVfxPoint, msg.hurtValue,
                msg.hitEffect, 1f));

            // 立绘抖动延迟与受击特效同步
            StartCoroutine(DelayedShakeCoroutine(slot.icon.rectTransform));

            // 死亡变黑延迟与受击特效同步
            if (msg.isDead)
                StartCoroutine(DelayedDeathDarken(slot.icon));
        }
    }

    /// <summary>
    /// 敌人行动提示：激活常驻 ActionToast 并显示 "{敌人名字} 使用了 {行动}"
    /// </summary>
    private void HandleEnemyActionToast(BattleEventDefine.EnemyActionToast msg)
    {
        if (enemyActionToast == null) return;

        enemyActionToast.gameObject.SetActive(true);
        enemyActionToast.Setup(msg.enemyName, msg.actionType);
    }

    /// <summary>
    /// 防御特效：在防御者（玩家角色或敌人）的 VFXPoint 上生成防御特效
    /// </summary>
    private void HandleDefendEffect(BattleEventDefine.DefendEffect msg)
    {
        if (defendEffect == null) return;

        if (msg.isPlayer)
        {
            // 玩家角色防御特效：idx 是 roleList 中的原始索引，需映射到显示 slot
            if (currentOriginalRoles == null || currentDisplayRoles == null) return;
            if (msg.idx < 0 || msg.idx >= currentOriginalRoles.Count) return;

            RoleInfo defendRole = currentOriginalRoles[msg.idx];
            int slotIdx = currentDisplayRoles.IndexOf(defendRole);
            if (slotIdx < 0 || slotIdx >= roles.roleSlots.Count) return;

            var slot = roles.roleSlots[slotIdx];
            if (slot != null && slot.gameObject.activeSelf)
                SpawnEffect(defendEffect, slot.VFXPoint);
        }
        else
        {
            // 敌人防御特效：idx 直接对应 enemySlots 索引
            if (enemies == null) return;
            if (msg.idx < 0 || msg.idx >= enemies.enemySlots.Count) return;

            var slot = enemies.enemySlots[msg.idx];
            if (slot != null && slot.gameObject.activeSelf)
                SpawnEffect(defendEffect, slot.VFXPoint);
        }
    }

    /// <summary>
    /// 延迟生成受击特效（与伤害数字同时播放）
    /// </summary>
    /// <param name="scaleMultiplier">受击特效缩放倍率（role1/role2 为 0.6，其余为 1）</param>
    private IEnumerator DelayedSpawnHitEffectAndHurtNum(Transform vfxPoint, int damage,
        GameObject hitEffectPrefab, float scaleMultiplier = 1f)
    {
        yield return new WaitForSeconds(0.5f);

        // 受击特效：有则播，无则跳过
        if (hitEffectPrefab != null)
        {
            SpawnEffect(hitEffectPrefab, vfxPoint, scaleMultiplier);
        }

        SpawnHurtNum(vfxPoint, damage);
    }

    private void SpawnHurtNum(Transform vfxPoint, int damage)
    {
        if (hurtNumPrefab == null) return;
        GameObject go = Instantiate(hurtNumPrefab, transform);
        go.transform.position = vfxPoint.position;
        go.GetComponent<HurtNum>().Setup(damage);
        // OnEnable 自动开始动画
    }

    /// <summary>
    /// 生成特效到指定位置（直接实例化，EffectController 播放完毕后自动回收/销毁）
    /// </summary>
    /// <param name="effectPrefab">特效预制体（null 则跳过）</param>
    /// <param name="vfxPoint">生成位置锚点</param>
    /// <param name="scaleMultiplier">缩放倍率（默认 1）</param>
    private void SpawnEffect(GameObject effectPrefab, Transform vfxPoint, float scaleMultiplier = 1f)
    {
        if (effectPrefab == null || vfxPoint == null) return;

        // 直接实例化预制体引用（不使用 PoolMgr.GetObj，因为其工厂依赖 Resources.Load，
        // 对于 ScriptableObject 中的直接引用会 fallback 到错误的默认特效）
        GameObject eff = Instantiate(effectPrefab);
        eff.transform.SetParent(transform, false);
        eff.transform.position = vfxPoint.position;
        eff.transform.localScale *= scaleMultiplier;

        // EffectController 在 OnEnable 中自动播放粒子系统，播放完毕后自动归还对象池
        // 若对象池中无对应抽屉，则直接 Destroy
    }

    /// <summary>
    /// 立绘抖动协程
    /// </summary>
    private IEnumerator ShakeCoroutine(RectTransform target)
    {
        Vector3 originPos = target.localPosition;
        float duration = 0.6f;
        float intensity = 15f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = originPos.x + Random.Range(-intensity, intensity);
            float y = originPos.y + Random.Range(-intensity, intensity);
            target.localPosition = new Vector3(x, y, originPos.z);
            intensity *= 0.85f;
            yield return null;
        }
        target.localPosition = originPos;
    }

    /// <summary>
    /// 延迟立绘抖动协程（与受击特效同步播放）
    /// </summary>
    private IEnumerator DelayedShakeCoroutine(RectTransform target)
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ShakeCoroutine(target));
    }

    /// <summary>
    /// 延迟死亡变黑协程（与受击特效同步播放）
    /// </summary>
    private IEnumerator DelayedDeathDarken(Image icon)
    {
        yield return new WaitForSeconds(0.5f);
        icon.color = new Color(0.2f, 0.2f, 0.2f);
    }

    /// <summary>
    /// 点击战斗结束按钮：
    /// 胜利 → 直接确认，执行 WinEvent
    /// 失败 → 弹窗"是否重新开始？" → 重来=重启战斗 / 取消=执行 LoseEvent
    /// </summary>
    private void OnClickBattleEnd()
    {
        if (battleIsWin)
        {
            BattleEventDefine.BattleEndConfirm.SendEventMessage();
        }
        else
        {
            TipPanelEventDefine.ShowTip.SendEventMessage("是否重新开始？", "重来",
                () => SceneEventDefine.NodeGame.SendEventMessage(), "取消",
                () => BattleEventDefine.BattleEndConfirm.SendEventMessage());
        }
    }

    /// <summary>
    /// 构建角色显示列表：行动角色始终在第一位，其余角色保持原顺序跟在后面（跳过行动角色避免重复）
    /// </summary>
    private List<RoleInfo> BuildDisplayRoles(List<RoleInfo> roleList, RoleInfo actingRole)
    {
        var result = new List<RoleInfo>();
        if (actingRole != null)
            result.Add(actingRole);
        if (roleList != null)
        {
            foreach (var r in roleList)
            {
                if (r != actingRole)
                    result.Add(r);
            }
        }
        return result;
    }

#endregion
}
