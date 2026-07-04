using UnityEditor;
using UnityEngine;

/// <summary>
/// GameManager 自定义 Inspector：
/// - hasStartEvent 勾选后才显示 StartNode 编辑
/// - hasEndEvent 勾选后才显示 EndNode 编辑
/// </summary>
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private SerializedProperty hasStartEventProp;
    private SerializedProperty hasEndEventProp;
    private SerializedProperty startNodeProp;
    private SerializedProperty endNodeProp;

    private void OnEnable()
    {
        hasStartEventProp = serializedObject.FindProperty("hasStartEvent");
        hasEndEventProp = serializedObject.FindProperty("hasEndEvent");
        startNodeProp = serializedObject.FindProperty("StartNode");
        endNodeProp = serializedObject.FindProperty("EndNode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── 默认绘制除特殊字段外的所有属性 ──
        DrawDefaultInspectorExcept("hasStartEvent", "hasEndEvent", "StartNode", "EndNode");

        // ── 游戏流程事件区域 ──
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("游戏流程事件", EditorStyles.boldLabel);

        // hasStartEvent 开关
        EditorGUILayout.PropertyField(hasStartEventProp);
        if (hasStartEventProp.boolValue && startNodeProp != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(startNodeProp);
            EditorGUI.indentLevel--;
        }

        // hasEndEvent 开关
        EditorGUILayout.PropertyField(hasEndEventProp);
        if (hasEndEventProp.boolValue && endNodeProp != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(endNodeProp);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 使用默认绘制器逐字段绘制，但跳过指定字段名。
    /// </summary>
    private void DrawDefaultInspectorExcept(params string[] excludeNames)
    {
        var property = serializedObject.GetIterator();
        if (!property.NextVisible(true))
            return;

        do
        {
            if (System.Array.IndexOf(excludeNames, property.name) >= 0)
                continue;

            EditorGUILayout.PropertyField(property, true);
        }
        while (property.NextVisible(false));
    }
}
