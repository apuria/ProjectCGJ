using UnityEditor;
using UnityEngine;

/// <summary>
/// DialogueSetting 的自定义 Editor：
/// hasBackground 勾选后才显示 BackGround 字段。
/// </summary>
[CustomEditor(typeof(DialogueSetting))]
public class DialogueSettingsEditor : Editor
{
    private SerializedProperty bgmNameProp;
    private SerializedProperty hasReturnButtonProp;
    private SerializedProperty hasBackgroundProp;
    private SerializedProperty backGroundProp;
    private SerializedProperty speakersProp;
    private SerializedProperty dialoguesProp;

    private void OnEnable()
    {
        bgmNameProp = serializedObject.FindProperty("bgmName");
        hasReturnButtonProp = serializedObject.FindProperty("hasReturnButton");
        hasBackgroundProp = serializedObject.FindProperty("hasBackground");
        backGroundProp = serializedObject.FindProperty("BackGround");
        speakersProp = serializedObject.FindProperty("speakers");
        dialoguesProp = serializedObject.FindProperty("dialogues");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(bgmNameProp, new GUIContent("背景音乐"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(hasReturnButtonProp, new GUIContent("显示返回按钮"));
        EditorGUILayout.PropertyField(hasBackgroundProp, new GUIContent("显示背景"));

        if (hasBackgroundProp.boolValue)
        {
            EditorGUILayout.PropertyField(backGroundProp, new GUIContent("背景图片"));
        }

        EditorGUILayout.PropertyField(speakersProp, new GUIContent("角色列表"), true);
        EditorGUILayout.PropertyField(dialoguesProp, new GUIContent("对话列表"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
