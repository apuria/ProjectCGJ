using UnityEditor;
using UnityEngine;

/// <summary>
/// BranchSetting 的自定义 Editor：
/// 根据 onEnd 的选择，只显示对应的 Setting 字段。
/// </summary>
[CustomEditor(typeof(BranchSetting))]
public class BranchSettingEditor : Editor
{
    private SerializedProperty idProp;
    private SerializedProperty branchesProp;
    private SerializedProperty onEndProp;
    private SerializedProperty nextDialogueProp;
    private SerializedProperty battleSettingProp;
    private SerializedProperty nextBranchProp;
    private SerializedProperty endSettingProp;

    private void OnEnable()
    {
        idProp = serializedObject.FindProperty("id");
        branchesProp = serializedObject.FindProperty("branches");
        onEndProp = serializedObject.FindProperty("onEnd");
        nextDialogueProp = serializedObject.FindProperty("nextDialogue");
        battleSettingProp = serializedObject.FindProperty("battleSetting");
        nextBranchProp = serializedObject.FindProperty("nextBranch");
        endSettingProp = serializedObject.FindProperty("endSetting");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── ID ──
        EditorGUILayout.PropertyField(idProp, new GUIContent("ID", "当前分支选项对应的选择配置"));

        EditorGUILayout.Space();

        // ── 分支选项列表 ──
        EditorGUILayout.PropertyField(branchesProp, new GUIContent("Branches"), true);

        EditorGUILayout.Space();

        // ── onEnd 枚举 ──
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(onEndProp, new GUIContent("On End"));
        if (EditorGUI.EndChangeCheck())
        {
            // 切换时清空不相关的 Setting
            var newOnEnd = (EOnEnd)onEndProp.enumValueIndex;
            if (newOnEnd != EOnEnd.NextDialogue) nextDialogueProp.objectReferenceValue = null;
            if (newOnEnd != EOnEnd.StartBattle) battleSettingProp.objectReferenceValue = null;
            if (newOnEnd != EOnEnd.StartBranch) nextBranchProp.objectReferenceValue = null;
            if (newOnEnd != EOnEnd.End) endSettingProp.objectReferenceValue = null;
        }

        // ── 根据 onEnd 显示对应 Setting ──
        var onEnd = (EOnEnd)onEndProp.enumValueIndex;
        switch (onEnd)
        {
            case EOnEnd.NextDialogue:
                EditorGUILayout.PropertyField(nextDialogueProp, new GUIContent("Next Dialogue"));
                break;

            case EOnEnd.StartBattle:
                EditorGUILayout.PropertyField(battleSettingProp, new GUIContent("Battle Setting"));
                break;

            case EOnEnd.StartBranch:
                EditorGUILayout.PropertyField(nextBranchProp, new GUIContent("Next Branch"));
                break;

            case EOnEnd.End:
                EditorGUILayout.PropertyField(endSettingProp, new GUIContent("End Setting"));
                break;

            case EOnEnd.GoBackToLastState:
                // 不需要额外配置
                EditorGUILayout.HelpBox("不需要额外配置，将回到上一个状态。", MessageType.Info);
                break;

            case EOnEnd.GoBackToMap:
                // 不需要额外配置
                EditorGUILayout.HelpBox("不需要额外配置，将回到地图。", MessageType.Info);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
