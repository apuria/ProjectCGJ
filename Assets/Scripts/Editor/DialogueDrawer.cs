using UnityEditor;
using UnityEngine;

/// <summary>
/// Dialogue 的自定义 PropertyDrawer：
/// 根据 dialogueType 的选择，显示人物对话或 CG 对话的对应字段。
/// </summary>
[CustomPropertyDrawer(typeof(Dialogue))]
public class DialogueDrawer : PropertyDrawer
{
    private const float LineHeight = 18f;
    private const float Padding = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var dialogueTypeProp = property.FindPropertyRelative("dialogueType");
        var cgImageProp = property.FindPropertyRelative("cgImage");
        var showDialogueBoxProp = property.FindPropertyRelative("showDialogueBox");
        var leftIdxProp = property.FindPropertyRelative("leftSpeakerIndex");
        var rightIdxProp = property.FindPropertyRelative("rightSpeakerIndex");
        var talkProp = property.FindPropertyRelative("talkingSpeaker");
        var textProp = property.FindPropertyRelative("text");
        var onEndProp = property.FindPropertyRelative("onEnd");
        var battleProp = property.FindPropertyRelative("Battle");
        var branchProp = property.FindPropertyRelative("Branch");
        var nextLogProp = property.FindPropertyRelative("NextLog");
        var endProp = property.FindPropertyRelative("End");

        float y = position.y;
        float x = position.x;
        float w = position.width;

        // ── 对话类型 ──
        Rect rType = new Rect(x, y, w, LineHeight);
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(rType, dialogueTypeProp, new GUIContent("对话类型"));
        if (EditorGUI.EndChangeCheck())
        {
            var newType = (EDialogueType)dialogueTypeProp.enumValueIndex;
            if (newType == EDialogueType.CGDialogue)
            {
                leftIdxProp.intValue = -1;
                rightIdxProp.intValue = -1;
                talkProp.enumValueIndex = 0;
            }
            else
            {
                cgImageProp.objectReferenceValue = null;
                showDialogueBoxProp.boolValue = true;
            }
        }
        y += LineHeight + Padding;

        var dialogueType = (EDialogueType)dialogueTypeProp.enumValueIndex;

        if (dialogueType == EDialogueType.CGDialogue)
        {
            y = DrawCGDialogueFields(x, y, w, cgImageProp, showDialogueBoxProp, leftIdxProp, rightIdxProp, talkProp,
                textProp, onEndProp, battleProp, branchProp, nextLogProp, endProp);
        }
        else
        {
            y = DrawCharacterDialogueFields(x, y, w, leftIdxProp, rightIdxProp, talkProp, textProp, onEndProp,
                battleProp, branchProp, nextLogProp, endProp);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var dialogueTypeProp = property.FindPropertyRelative("dialogueType");
        var showDialogueBoxProp = property.FindPropertyRelative("showDialogueBox");
        var textProp = property.FindPropertyRelative("text");
        var onEndProp = property.FindPropertyRelative("onEnd");

        var dialogueType = (EDialogueType)dialogueTypeProp.enumValueIndex;
        var onEnd = (EOnEnd)onEndProp.enumValueIndex;

        // 对话类型行
        float height = LineHeight + Padding;

        if (dialogueType == EDialogueType.CGDialogue)
        {
            // CG Image + Show Dialogue Box
            height += (LineHeight + Padding) * 2;

            if (showDialogueBoxProp.boolValue)
            {
                // 角色字段 3 行 + Text
                height += (LineHeight + Padding) * 3;
                float textH = EditorGUI.GetPropertyHeight(textProp, new GUIContent("对话文本"));
                height += textH + Padding;
            }

            // On End
            height += LineHeight + Padding;

            if (onEnd == EOnEnd.StartBattle || onEnd == EOnEnd.StartBranch ||
                onEnd == EOnEnd.NextLog || onEnd == EOnEnd.End)
            {
                height += LineHeight + Padding;
            }
        }
        else
        {
            float textH = EditorGUI.GetPropertyHeight(textProp, new GUIContent("对话文本"));
            height += LineHeight * 4 + textH + Padding * 5;

            if (onEnd == EOnEnd.StartBattle || onEnd == EOnEnd.StartBranch ||
                onEnd == EOnEnd.NextLog || onEnd == EOnEnd.End)
            {
                height += LineHeight + Padding;
            }
        }

        return height;
    }

    // ─────────────────── CG 对话字段 ───────────────────

    private static float DrawCGDialogueFields(float x, float y, float w,
        SerializedProperty cgImageProp, SerializedProperty showDialogueBoxProp,
        SerializedProperty leftIdxProp, SerializedProperty rightIdxProp,
        SerializedProperty talkProp, SerializedProperty textProp,
        SerializedProperty onEndProp, SerializedProperty battleProp,
        SerializedProperty branchProp, SerializedProperty nextLogProp,
        SerializedProperty endProp)
    {
        // CG 图片
        Rect rCg = new Rect(x, y, w, LineHeight);
        EditorGUI.PropertyField(rCg, cgImageProp, new GUIContent("CG 图片"));
        y += LineHeight + Padding;

        // 显示对话框
        Rect rShowBox = new Rect(x, y, w, LineHeight);
        EditorGUI.PropertyField(rShowBox, showDialogueBoxProp, new GUIContent("显示对话框"));
        y += LineHeight + Padding;

        if (showDialogueBoxProp.boolValue)
        {
            // 角色索引（用于显示名字和高亮）
            Rect rLeft = new Rect(x, y, w, LineHeight);
            EditorGUI.PropertyField(rLeft, leftIdxProp, new GUIContent("左侧角色索引"));
            y += LineHeight + Padding;

            Rect rRight = new Rect(x, y, w, LineHeight);
            EditorGUI.PropertyField(rRight, rightIdxProp, new GUIContent("右侧角色索引"));
            y += LineHeight + Padding;

            Rect rTalk = new Rect(x, y, w, LineHeight);
            EditorGUI.PropertyField(rTalk, talkProp, new GUIContent("当前说话者"));
            y += LineHeight + Padding;

            float textH = EditorGUI.GetPropertyHeight(textProp, new GUIContent("对话文本"));
            Rect rText = new Rect(x, y, w, textH);
            EditorGUI.PropertyField(rText, textProp, new GUIContent("对话文本"));
            y += textH + Padding;
        }

        y = DrawOnEndSection(x, y, w, onEndProp, battleProp, branchProp, nextLogProp, endProp);
        return y;
    }

    // ─────────────────── 人物对话字段 ───────────────────

    private static float DrawCharacterDialogueFields(float x, float y, float w,
        SerializedProperty leftIdxProp, SerializedProperty rightIdxProp,
        SerializedProperty talkProp, SerializedProperty textProp,
        SerializedProperty onEndProp, SerializedProperty battleProp,
        SerializedProperty branchProp, SerializedProperty nextLogProp,
        SerializedProperty endProp)
    {
        Rect r1 = new Rect(x, y, w, LineHeight);
        EditorGUI.PropertyField(r1, leftIdxProp, new GUIContent("左侧角色索引"));
        y += LineHeight + Padding;

        Rect r2 = new Rect(x, y, w, LineHeight);
        EditorGUI.PropertyField(r2, rightIdxProp, new GUIContent("右侧角色索引"));
        y += LineHeight + Padding;

        Rect r3 = new Rect(x, y, w, LineHeight);
        EditorGUI.PropertyField(r3, talkProp, new GUIContent("当前说话者"));
        y += LineHeight + Padding;

        float textH = EditorGUI.GetPropertyHeight(textProp, new GUIContent("对话文本"));
        Rect r4 = new Rect(x, y, w, textH);
        EditorGUI.PropertyField(r4, textProp, new GUIContent("对话文本"));
        y += textH + Padding;

        y = DrawOnEndSection(x, y, w, onEndProp, battleProp, branchProp, nextLogProp, endProp);
        return y;
    }

    // ─────────────────── 结束行为 ───────────────────

    private static float DrawOnEndSection(float x, float y, float w,
        SerializedProperty onEndProp, SerializedProperty battleProp,
        SerializedProperty branchProp, SerializedProperty nextLogProp,
        SerializedProperty endProp)
    {
        Rect rOnEnd = new Rect(x, y, w, LineHeight);
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(rOnEnd, onEndProp, new GUIContent("结束行为"));
        if (EditorGUI.EndChangeCheck())
        {
            var newOnEnd = (EOnEnd)onEndProp.enumValueIndex;
            if (newOnEnd != EOnEnd.StartBattle) battleProp.objectReferenceValue = null;
            if (newOnEnd != EOnEnd.StartBranch) branchProp.objectReferenceValue = null;
            if (newOnEnd != EOnEnd.NextLog) nextLogProp.objectReferenceValue = null;
            if (newOnEnd != EOnEnd.End) endProp.objectReferenceValue = null;
        }
        y += LineHeight + Padding;

        var onEnd = (EOnEnd)onEndProp.enumValueIndex;
        if (onEnd == EOnEnd.StartBattle)
        {
            Rect r = new Rect(x, y, w, LineHeight);
            EditorGUI.PropertyField(r, battleProp, new GUIContent("战斗配置"));
        }
        else if (onEnd == EOnEnd.StartBranch)
        {
            Rect r = new Rect(x, y, w, LineHeight);
            EditorGUI.PropertyField(r, branchProp, new GUIContent("分支对话"));
        }
        else if (onEnd == EOnEnd.NextLog)
        {
            Rect r = new Rect(x, y, w, LineHeight);
            EditorGUI.PropertyField(r, nextLogProp, new GUIContent("下一个对话"));
        }
        else if (onEnd == EOnEnd.End)
        {
            Rect r = new Rect(x, y, w, LineHeight);
            EditorGUI.PropertyField(r, endProp, new GUIContent("End 配置"));
        }

        return y;
    }
}
