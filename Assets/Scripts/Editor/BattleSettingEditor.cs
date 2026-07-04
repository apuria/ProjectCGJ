using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BattleSetting 的自定义 Editor：
/// 根据 startEvent / endEvent / 每个 midEvent 的 triggerType 和 action，只显示对应的 Setting 字段。
/// </summary>
[CustomEditor(typeof(BattleSetting))]
public class BattleSettingEditor : Editor
{
    private SerializedProperty bgmNameProp;
    private SerializedProperty backgroundProp;
    private SerializedProperty enemiesProp;

    // 开始事件
    private SerializedProperty startEventProp;
    private SerializedProperty startDialogueProp;
    private SerializedProperty startBranchProp;

    // 战斗胜利事件
    private SerializedProperty winEventProp;
    private SerializedProperty winDialogueProp;
    private SerializedProperty winBranchProp;

    // 战斗失败事件
    private SerializedProperty loseEventProp;
    private SerializedProperty loseDialogueProp;
    private SerializedProperty loseBranchProp;

    // End 事件
    private SerializedProperty winEndSettingProp;
    private SerializedProperty loseEndSettingProp;

    // 中途事件列表
    private SerializedProperty midEventsProp;

    // 多角色分支
    private SerializedProperty moreRoleBranchesProp;

    // 道具分支
    private SerializedProperty moreItemBranchesProp;

    // 道具分支折叠状态
    private List<bool> itemBranchFoldouts = new();

    private void OnEnable()
    {
        bgmNameProp = serializedObject.FindProperty("bgmName");
        backgroundProp = serializedObject.FindProperty("Background");
        enemiesProp = serializedObject.FindProperty("enemies");

        startEventProp = serializedObject.FindProperty("startEvent");
        startDialogueProp = serializedObject.FindProperty("startDialogue");
        startBranchProp = serializedObject.FindProperty("startBranch");

        winEventProp = serializedObject.FindProperty("winEvent");
        winDialogueProp = serializedObject.FindProperty("winDialogue");
        winBranchProp = serializedObject.FindProperty("winBranch");

        loseEventProp = serializedObject.FindProperty("loseEvent");
        loseDialogueProp = serializedObject.FindProperty("loseDialogue");
        loseBranchProp = serializedObject.FindProperty("loseBranch");

        winEndSettingProp = serializedObject.FindProperty("winEndSetting");
        loseEndSettingProp = serializedObject.FindProperty("loseEndSetting");

        midEventsProp = serializedObject.FindProperty("midEvents");
        moreRoleBranchesProp = serializedObject.FindProperty("moreRoleBranches");
        moreItemBranchesProp = serializedObject.FindProperty("moreItemBranches");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── 音乐 ──
        EditorGUILayout.PropertyField(bgmNameProp, new GUIContent("背景音乐"));
        EditorGUILayout.Space();

        // ── 背景图 ──
        EditorGUILayout.PropertyField(backgroundProp, new GUIContent("自定义背景图"));
        if (backgroundProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("背景图为空，将使用默认背景。", MessageType.Info);
        }
        EditorGUILayout.Space();

        // ── 敌人列表 ──
        EditorGUILayout.PropertyField(enemiesProp, new GUIContent("Enemies"), true);

        EditorGUILayout.Space();

        // ── 多角色分支 ──
        EditorGUILayout.PropertyField(moreRoleBranchesProp, new GUIContent("More Role Branches"), true);

        EditorGUILayout.Space();

        // ── 道具分支 ──
        DrawMoreItemBranches();

        EditorGUILayout.Space();

        // ── 开始事件 ──
        DrawStartEvent();

        EditorGUILayout.Space();

        // ── 战斗胜利事件 ──
        DrawEndEvent("战斗胜利事件", winEventProp, winDialogueProp, winBranchProp, winEndSettingProp);

        EditorGUILayout.Space();

        // ── 战斗失败事件 ──
        DrawEndEvent("战斗失败事件", loseEventProp, loseDialogueProp, loseBranchProp, loseEndSettingProp);

        EditorGUILayout.Space();

        // ── 中途事件列表 ──
        DrawMidEvents();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStartEvent()
    {
        EditorGUILayout.LabelField("Start Event", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(startEventProp, new GUIContent("Start Event"));
        if (EditorGUI.EndChangeCheck())
        {
            var newStartEvent = (EBattleStartEvent)startEventProp.enumValueIndex;
            if (newStartEvent != EBattleStartEvent.PlayDialogue) startDialogueProp.objectReferenceValue = null;
            if (newStartEvent != EBattleStartEvent.StartBranch) startBranchProp.objectReferenceValue = null;
        }

        var startEvent = (EBattleStartEvent)startEventProp.enumValueIndex;
        switch (startEvent)
        {
            case EBattleStartEvent.PlayDialogue:
                EditorGUILayout.PropertyField(startDialogueProp, new GUIContent("Start Dialogue"));
                break;

            case EBattleStartEvent.StartBranch:
                EditorGUILayout.PropertyField(startBranchProp, new GUIContent("Start Branch"));
                break;

            case EBattleStartEvent.None:
                EditorGUILayout.HelpBox("不需要额外配置，战斗将直接开始。", MessageType.Info);
                break;
        }
    }

    private void DrawEndEvent(string label, SerializedProperty eventProp, SerializedProperty dialogueProp, SerializedProperty branchProp, SerializedProperty endSettingProp)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(eventProp, new GUIContent(label));
        if (EditorGUI.EndChangeCheck())
        {
            var newEndEvent = (EBattleEndEvent)eventProp.enumValueIndex;
            if (newEndEvent != EBattleEndEvent.NextDialogue) dialogueProp.objectReferenceValue = null;
            if (newEndEvent != EBattleEndEvent.StartBranch) branchProp.objectReferenceValue = null;
            if (newEndEvent != EBattleEndEvent.End) endSettingProp.objectReferenceValue = null;
        }

        var endEvent = (EBattleEndEvent)eventProp.enumValueIndex;
        switch (endEvent)
        {
            case EBattleEndEvent.NextDialogue:
                EditorGUILayout.PropertyField(dialogueProp, new GUIContent("Dialogue"));
                break;

            case EBattleEndEvent.StartBranch:
                EditorGUILayout.PropertyField(branchProp, new GUIContent("Branch"));
                break;

            case EBattleEndEvent.End:
                EditorGUILayout.PropertyField(endSettingProp, new GUIContent("End Setting"));
                break;

            case EBattleEndEvent.None:
                EditorGUILayout.HelpBox("不需要额外配置。", MessageType.Info);
                break;

            case EBattleEndEvent.GoBackToLastState:
                EditorGUILayout.HelpBox("不需要额外配置，将回到上一个状态。", MessageType.Info);
                break;

            case EBattleEndEvent.GoBackToMap:
                EditorGUILayout.HelpBox("不需要额外配置，将回到地图。", MessageType.Info);
                break;
        }
    }

    private void DrawMidEvents()
    {
        EditorGUILayout.LabelField("Mid Events", EditorStyles.boldLabel);

        if (midEventsProp == null) return;

        for (int i = 0; i < midEventsProp.arraySize; i++)
        {
            var elementProp = midEventsProp.GetArrayElementAtIndex(i);
            DrawMidEventElement(elementProp, i);
            EditorGUILayout.Space();
        }

        // ── 添加 / 清空按钮 ──
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Mid Event"))
        {
            midEventsProp.arraySize++;
            // 新元素的默认值由 MidBattleEvent 类中的字段默认值处理
        }

        if (midEventsProp.arraySize > 0)
        {
            if (GUILayout.Button("Remove Last"))
            {
                midEventsProp.arraySize--;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawMidEventElement(SerializedProperty elementProp, int index)
    {
        var triggerTypeProp = elementProp.FindPropertyRelative("triggerType");
        var targetIndexProp = elementProp.FindPropertyRelative("targetIndex");
        var hpPercentageProp = elementProp.FindPropertyRelative("hpPercentage");
        var turnCountProp = elementProp.FindPropertyRelative("turnCount");
        var actionProp = elementProp.FindPropertyRelative("action");
        var dialogueProp = elementProp.FindPropertyRelative("dialogue");
        var branchProp = elementProp.FindPropertyRelative("branch");

        // ── 标题与删除 ──
        var triggerType = (EMidEventTriggerType)triggerTypeProp.enumValueIndex;
        var action = (EMidEventAction)actionProp.enumValueIndex;
        string summary = GetMidEventSummary(triggerType, action,
            triggerType == EMidEventTriggerType.RoundCount ? turnCountProp.intValue : hpPercentageProp.intValue,
            targetIndexProp.intValue, index);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(summary, EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        // ── 触发条件类型 ──
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(triggerTypeProp, new GUIContent("Trigger Type"));
        if (EditorGUI.EndChangeCheck())
        {
            // 切换触发类型时，重置对应字段为默认值
            targetIndexProp.intValue = 0;
            hpPercentageProp.intValue = 50;
            turnCountProp.intValue = 1;
        }

        var currentTriggerType = (EMidEventTriggerType)triggerTypeProp.enumValueIndex;
        switch (currentTriggerType)
        {
            case EMidEventTriggerType.CharacterHpThreshold:
                EditorGUILayout.PropertyField(targetIndexProp, new GUIContent("角色索引"));
                EditorGUILayout.PropertyField(hpPercentageProp, new GUIContent("血量百分比"));
                EditorGUILayout.HelpBox($"当角色[{targetIndexProp.intValue}]的血量 ≤ {hpPercentageProp.intValue}% 时触发", MessageType.None);
                break;

            case EMidEventTriggerType.EnemyHpThreshold:
                EditorGUILayout.PropertyField(targetIndexProp, new GUIContent("敌人索引"));
                EditorGUILayout.PropertyField(hpPercentageProp, new GUIContent("血量百分比"));
                EditorGUILayout.HelpBox($"当敌人[{targetIndexProp.intValue}]的血量 ≤ {hpPercentageProp.intValue}% 时触发", MessageType.None);
                break;

            case EMidEventTriggerType.RoundCount:
                EditorGUILayout.PropertyField(turnCountProp, new GUIContent("回合数"));
                EditorGUILayout.HelpBox($"到达第 {turnCountProp.intValue} 回合时触发", MessageType.None);
                break;
        }

        EditorGUILayout.Space();

        // ── 触发后做什么 ──
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(actionProp, new GUIContent("Action"));
        if (EditorGUI.EndChangeCheck())
        {
            var newAction = (EMidEventAction)actionProp.enumValueIndex;
            if (newAction != EMidEventAction.PlayDialogue) dialogueProp.objectReferenceValue = null;
            if (newAction != EMidEventAction.StartBranch) branchProp.objectReferenceValue = null;
        }

        var currentAction = (EMidEventAction)actionProp.enumValueIndex;
        switch (currentAction)
        {
            case EMidEventAction.PlayDialogue:
                EditorGUILayout.PropertyField(dialogueProp, new GUIContent("Dialogue"));
                break;

            case EMidEventAction.StartBranch:
                EditorGUILayout.PropertyField(branchProp, new GUIContent("Branch"));
                break;
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private static string GetMidEventSummary(EMidEventTriggerType triggerType, EMidEventAction action,
        int value, int targetIndex, int index)
    {
        string triggerDesc = triggerType switch
        {
            EMidEventTriggerType.CharacterHpThreshold => $"角色[{targetIndex}] HP ≤ {value}%",
            EMidEventTriggerType.EnemyHpThreshold => $"敌人[{targetIndex}] HP ≤ {value}%",
            EMidEventTriggerType.RoundCount => $"第 {value} 回合",
            _ => "未知",
        };

        string actionDesc = action switch
        {
            EMidEventAction.PlayDialogue => "播放对话",
            EMidEventAction.StartBranch => "开始分支",
            _ => "未知",
        };

        return $"Mid Event [{index}] | {triggerDesc} → {actionDesc}";
    }

    // ─────────────────── 道具分支绘制 ───────────────────

    private void DrawMoreItemBranches()
    {
        EditorGUILayout.LabelField("More Item Branches (道具分支)", EditorStyles.boldLabel);

        if (moreItemBranchesProp == null) return;

        // 同步折叠状态列表与数组大小
        while (itemBranchFoldouts.Count < moreItemBranchesProp.arraySize)
            itemBranchFoldouts.Add(true);
        while (itemBranchFoldouts.Count > moreItemBranchesProp.arraySize)
            itemBranchFoldouts.RemoveAt(itemBranchFoldouts.Count - 1);

        for (int i = 0; i < moreItemBranchesProp.arraySize; i++)
        {
            var elementProp = moreItemBranchesProp.GetArrayElementAtIndex(i);
            DrawMoreItemBranchElement(elementProp, i);
            EditorGUILayout.Space();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Item Branch"))
        {
            moreItemBranchesProp.arraySize++;
            itemBranchFoldouts.Add(true);
        }
        if (moreItemBranchesProp.arraySize > 0)
        {
            if (GUILayout.Button("Remove Last"))
            {
                moreItemBranchesProp.arraySize--;
                if (itemBranchFoldouts.Count > 0)
                    itemBranchFoldouts.RemoveAt(itemBranchFoldouts.Count - 1);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawMoreItemBranchElement(SerializedProperty elementProp, int index)
    {
        var branchIdProp  = elementProp.FindPropertyRelative("branchId");
        var chooseProp    = elementProp.FindPropertyRelative("choose");
        var buffInfoProp  = elementProp.FindPropertyRelative("buffInfo");
        var isForAllProp  = elementProp.FindPropertyRelative("isForAllRole");
        var roleIndexProp = elementProp.FindPropertyRelative("roleIndex");
        var isEveryProp   = elementProp.FindPropertyRelative("isEveryRound");

        // ── 折叠标题 ──
        var buffInfo = buffInfoProp.objectReferenceValue as BuffInfo;
        string buffName = buffInfo != null ? buffInfo.buffName : "未设置";
        string branchDesc = string.IsNullOrEmpty(branchIdProp.stringValue)
            ? "无条件"
            : $"{branchIdProp.stringValue} = {chooseProp.stringValue}";
        string targetDesc = isForAllProp.boolValue
            ? "全体友军"
            : $"角色[{roleIndexProp.intValue}]";
        string roundDesc = isEveryProp.boolValue ? " (每回合)" : "";

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        itemBranchFoldouts[index] = EditorGUILayout.Foldout(
            itemBranchFoldouts[index],
            $"Item Branch [{index}] | {branchDesc} → {buffName} → {targetDesc}{roundDesc}",
            true,
            EditorStyles.foldoutHeader);

        if (!itemBranchFoldouts[index])
        {
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUI.indentLevel++;

        // ── 分支条件 ──
        EditorGUILayout.LabelField("分支条件", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(branchIdProp, new GUIContent("Branch Id"));
        if (string.IsNullOrEmpty(branchIdProp.stringValue))
        {
            EditorGUILayout.HelpBox("Branch Id 为空 → 无条件添加此 Buff", MessageType.Info);
        }
        else
        {
            EditorGUILayout.PropertyField(chooseProp, new GUIContent("Choose (选择值)"));
            if (string.IsNullOrEmpty(chooseProp.stringValue))
            {
                EditorGUILayout.HelpBox("Choose 为空 → 只要 BranchList 中存在该 Branch Id 即匹配", MessageType.Info);
            }
        }

        EditorGUILayout.Space();

        // ── Buff 配置 ──
        EditorGUILayout.LabelField("Buff 配置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(buffInfoProp, new GUIContent("Buff Info"));

        if (buffInfo != null)
        {
            string statLabel = buffInfo.buffType switch
            {
                StatType.HP     => "HP",
                StatType.MAXHP  => "Max HP",
                StatType.MP     => "MP",
                StatType.MAXMP  => "Max MP",
                StatType.ATK    => "ATK",
                StatType.DEF    => "DEF",
                StatType.SPD    => "SPD",
                _               => "?",
            };
            if (buffInfo.isPercentage)
            {
                int pct = Mathf.RoundToInt(buffInfo.value * 100f);
                EditorGUILayout.HelpBox($"{statLabel} +{pct}% (按最大值比例)", MessageType.None);
            }
            else
            {
                string sign = buffInfo.value >= 0 ? "+" : "";
                EditorGUILayout.HelpBox($"{statLabel} {sign}{buffInfo.value}", MessageType.None);
            }
        }

        EditorGUILayout.Space();

        // ── 目标配置 ──
        EditorGUILayout.LabelField("目标配置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(isForAllProp, new GUIContent("全体友军"));

        if (!isForAllProp.boolValue)
        {
            EditorGUILayout.PropertyField(roleIndexProp, new GUIContent("角色索引"));
        }

        EditorGUILayout.Space();

        // ── 回合配置 ──
        EditorGUILayout.PropertyField(isEveryProp, new GUIContent("每回合执行"));
        if (isEveryProp.boolValue)
        {
            EditorGUILayout.HelpBox("此 Buff 将在每回合开始时重新应用（如每回合回血效果）", MessageType.Info);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }
}
