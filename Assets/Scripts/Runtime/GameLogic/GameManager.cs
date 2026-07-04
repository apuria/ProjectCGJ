using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;
using System;

public enum GameState
{
    Battle,
    DiaLogue,
    Branch
}

public class GameManager : SingletonMono<GameManager>
{
//游戏管理器
/*
1.
2.
*/
    public Canvas canvas;
    private StateMachine stateMachine;
    private EventGroup eventGroup;
    private PlayerData playerData;

    public PlayerData PlayerData => playerData;
    [SerializeField]
    public InGameData inGameData;

    public LoadingTexts loadingTexts;
    public RoleInfo mainRole;
    public List<RoleInfo> roleList = new();

#region 生命周期
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();
        eventGroup = new EventGroup();
        var canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            canvas = canvasObj.GetComponent<Canvas>();
        }
        else
        {
            Debug.LogWarning("GameManager.Awake: 场景中未找到名为 'Canvas' 的 GameObject，稍后将由 UIMgr 创建");
        }
    }

    void Start()
    {
        // 状态切换事件
        eventGroup.AddListener<StateEventDefine.ChangeState>(OnHandleEventMessage);
        eventGroup.AddListener<StateEventDefine.BackToPrevState>(OnHandleEventMessage);

        // 场景流程事件
        eventGroup.AddListener<SceneEventDefine.StartGame>(OnHandleEventMessage);
        eventGroup.AddListener<SceneEventDefine.EndGame>(OnHandleEventMessage);
        eventGroup.AddListener<SceneEventDefine.NodeGame>(OnHandleEventMessage);

        // 游戏数据事件
        eventGroup.AddListener<GameEventDefine.ContinueGame>(OnHandleEventMessage);
        eventGroup.AddListener<GameEventDefine.NewGame>(OnHandleEventMessage);
        eventGroup.AddListener<GameEventDefine.SaveProgress>(OnHandleEventMessage);
        eventGroup.AddListener<GameEventDefine.ReturnToMainMenu>(OnHandleEventMessage);
        eventGroup.AddListener<GameEventDefine.QuitGame>(OnHandleEventMessage);

        // 提示面板事件
        eventGroup.AddListener<TipPanelEventDefine.ShowTip>(OnHandleEventMessage);
        
        //游戏内数据事件
        eventGroup.AddListener<GameEventDefine.SaveInGameData>(OnHandleEventMessage);
    }

    void Update()
    {
        stateMachine.Update();
        
    }

    void OnDestroy()
    {
        eventGroup.RemoveAllListener();
    }

#endregion

#region 事件监听
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is StateEventDefine.ChangeState changeState)
        {
            if (changeState.stateType == typeof(MapState) || changeState.stateType == typeof(BattleState) || changeState.stateType == typeof(GameStart))
            {
                // 到地图、战斗或主菜单的切换先经过 LoadingState
                SwitchToWithLoading(changeState.stateType, changeState.tag, changeState.data);
            }
            else
            {
                stateMachine.SwitchTo(changeState.stateType, changeState.tag, changeState.data, changeState.destroy);
            }
        }
        else if (message is StateEventDefine.BackToPrevState backMsg)
        {
            string prevTag = stateMachine.PreviousNodeTag;
            if (string.IsNullOrEmpty(prevTag))
            {
                Debug.LogError("No previous state to go back to.");
                return;
            }

            if (!stateMachine.IsSuspended(prevTag))
            {
                Debug.LogError($"Previous state '{prevTag}' was destroyed, cannot go back.");
                return;
            }

            stateMachine.SwitchTo(prevTag, backMsg.destroy);
        }
        else if (message is SceneEventDefine.StartGame)
        {
            StartGameEvent();
        }
        else if (message is SceneEventDefine.EndGame)
        {
            EndGameEvent();
        }
        else if (message is SceneEventDefine.NodeGame)
        {
            NodeGameEvent();
        }
        else if (message is GameEventDefine.ContinueGame)
        {
            LoadPlayerData();
            SwitchToWithLoading(typeof(MapState), "MapState", null);
        }
        else if (message is GameEventDefine.NewGame)
        {
            StartANewGame();
            SwitchToWithLoading(typeof(MapState), "MapState", null);
        }
        else if (message is GameEventDefine.SaveProgress)
        {
            SavePlayerData();
        }
        else if (message is GameEventDefine.ReturnToMainMenu)
        {
            ReturnToMainMenuEvent();
        }
        else if (message is GameEventDefine.QuitGame)
        {
            QuitGameEvent();
        }
        else if (message is TipPanelEventDefine.ShowTip showTip)
        {
            ShowTipPanel(showTip);
        }
        else if (message is GameEventDefine.SaveInGameData)
        {
            SaveSettingData();
        }
    }
#endregion

#region 玩家数据
    public void SavePlayerData()
    {
        JsonMgr.Instance.SaveData(playerData, "PlayerData");
    }

    public void LoadPlayerData()
    {
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");
        if (playerData == null)
        {
            Debug.LogWarning("LoadPlayerData: 存档不存在或读取失败，按新游戏处理");
            playerData = new PlayerData();
        }
    }

    public void StartANewGame()
    {
        playerData = new PlayerData();
    }

    public void AddBranchChoose(string id, string choose)
    {
        playerData.branchList[id] =  choose;
    }
#endregion

#region 游戏内设置数据

//
/*
1. 游戏内设置数据
    1.音效音量
    2.音乐音量
    3.音效开关
    4.音乐开关
*/

    public void LoadSettingData()
    {
        inGameData.SfxVolume = PlayerPrefs.GetFloat("SoundVolume");
        inGameData.MusicVolume = PlayerPrefs.GetFloat("MusicVolume");  
        inGameData.SfxOn = PlayerPrefs.GetInt("SoundMute") == 1;
        inGameData.MusicOn = PlayerPrefs.GetInt("MusicMute") == 1;
    }

    public void SaveSettingData()
    {
        PlayerPrefs.SetFloat("SoundVolume", inGameData.SfxVolume);
        PlayerPrefs.SetFloat("MusicVolume", inGameData.MusicVolume);
        PlayerPrefs.SetInt("SoundMute", inGameData.SfxOn ? 1 : 0);
        PlayerPrefs.SetInt("MusicMute", inGameData.MusicOn ? 1 : 0);
    }

    public void SetSoundVolume(float volume)
    {
        inGameData.SfxVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        inGameData.MusicVolume = volume;
    }

    public void SetSoundMute(bool isOn)
    {
        inGameData.SfxOn = isOn;
    }

    public void SetMusicMute(bool isOn)
    {
        inGameData.MusicOn = isOn;
    }

#endregion


#region 游戏流程配置

    [SerializeField]
    [Tooltip("是否启用开始游戏事件")]
    private bool hasStartEvent = true;
    [SerializeField]
    [Tooltip("是否启用结束游戏事件")]
    private bool hasEndEvent = true;

    /// <summary>
    /// 是否启用了开始游戏事件（供 MapState 查询）
    /// </summary>
    public bool HasStartEvent => hasStartEvent;
    /// <summary>
    /// 是否启用了结束游戏事件（供 MapState 查询）
    /// </summary>
    public bool HasEndEvent => hasEndEvent;

    [SerializeField]
    [Tooltip("开始游戏事件节点，游戏启动时触发")]
    public GameNode StartNode;
    [SerializeField]
    [Tooltip("结束游戏事件节点，游戏结束时触发")]
    public GameNode EndNode;

    [SerializeField]
    [Tooltip("游戏流程的 4 个节点配置，每个节点对应一个状态类型及其 Setting")]
    private List<GameNode> gameNodes = new()
    {
        new GameNode { nodeName = "Node 1", stateType = GameState.Battle },
        new GameNode { nodeName = "Node 2", stateType = GameState.Battle },
        new GameNode { nodeName = "Node 3", stateType = GameState.DiaLogue },
        new GameNode { nodeName = "Node 4", stateType = GameState.Battle },
    };

    /// <summary>
    /// 获取节点配置列表（只读）
    /// </summary>
    public IReadOnlyList<GameNode> GameNodes => gameNodes;

#endregion

#region 游戏流程控制

    /// <summary>
    /// 游戏初始化：加载开始界面和开始状态
    /// </summary>
    public void Init()
    {
        // LoadSettingData();
        stateMachine.SwitchTo<GameStart>("GameStart");
    }

    /// <summary>
    /// 跳转到下一个节点
    /// </summary>
    public void NextNode()
    {
        playerData.NextFlow();
    }

    /// <summary>
    /// 根据当前节点类型，触发对应事件
    /// </summary>
    public void StartGameEvent()
    {
        switch (StartNode.stateType)
        {
            case GameState.Battle:
                StartBattle(StartNode.battleSetting);
                break;
            case GameState.DiaLogue:
                StartDiaLogue(StartNode.dialogueSetting);
                break;
            case GameState.Branch:
                StartBranch(StartNode.branchSetting);
                break;
        }
    }

    /// <summary>
    /// 根据当前节点类型，触发对应事件
    /// </summary>
    public void EndGameEvent()
    {
        switch (EndNode.stateType)
        {
            case GameState.Battle:
                StartBattle(EndNode.battleSetting);
                break;
            case GameState.DiaLogue:
                StartDiaLogue(EndNode.dialogueSetting);
                break;
            case GameState.Branch:
                StartBranch(EndNode.branchSetting);
                break;
        }
    }

    /// <summary>
    /// 根据当前节点类型，触发对应事件
    /// </summary>
    public void NodeGameEvent()
    {
        switch (gameNodes[(int)playerData.NowFlow].stateType)
        {
            case GameState.Battle:
                StartBattle(gameNodes[(int)playerData.NowFlow].battleSetting);
                break;
            case GameState.DiaLogue:
                StartDiaLogue(gameNodes[(int)playerData.NowFlow].dialogueSetting);
                break;
            case GameState.Branch:
                StartBranch(gameNodes[(int)playerData.NowFlow].branchSetting);
                break;
        }
    }

    /// <summary>
    /// 通过 LoadingState 过渡到目标状态
    /// </summary>
    private void SwitchToWithLoading(Type targetType, string targetTag, IStateData targetData)
    {
        var loadingInfo = new LoadingInfo
        {
            targetStateType = targetType,
            targetTag = targetTag,
            targetData = targetData,
        };
        stateMachine.SwitchTo<LoadingState>("LoadingState", loadingInfo);
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    /// <param name="battleSetting"></param>
    public void StartBattle(BattleSetting battleSetting)
    {
        SwitchToWithLoading(typeof(BattleState), "BattleState", battleSetting);
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    /// <param name="dialogueSetting"></param>
    public void StartDiaLogue(DialogueSetting dialogueSetting)
    {
        stateMachine.SwitchTo<DialogueState>("DialogueState", dialogueSetting);
    }

    /// <summary>
    /// 开始分支选项
    /// </summary>
    /// <param name="branchSetting"></param>
    public void StartBranch(BranchSetting branchSetting)
    {
        stateMachine.SwitchTo<BranchState>("BranchState", branchSetting);
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenuEvent()
    {
        SavePlayerData();
        playerData = null; // 清空玩家数据
        stateMachine.SwitchTo<GameStart>("GameStart");
    }

    /// <summary>
    /// 显示提示面板
    /// </summary>
    private void ShowTipPanel(TipPanelEventDefine.ShowTip showTip)
    {
        UIMgr.Instance.ShowPanel<TipPanel>(E_UILayer.Top, (panel) =>
        {
            panel.Setup(showTip.tipText, showTip.leftButtonText, showTip.leftButtonAction, showTip.rightButtonText, showTip.rightButtonAction);
        });
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGameEvent()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

#endregion
}
