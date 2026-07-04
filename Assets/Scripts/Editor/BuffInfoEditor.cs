using UnityEditor;
using UnityEngine;

/// <summary>
/// BuffInfo 的自定义 Inspector：
/// 根据 StatType 和 isPercentage 的选择，显示对应的数值含义说明。
/// </summary>
[CustomEditor(typeof(BuffInfo))]
public class BuffInfoEditor : Editor
{
    private SerializedProperty buffNameProp;
    private SerializedProperty buffTypeProp;
    private SerializedProperty buffIconProp;
    private SerializedProperty valueProp;
    private SerializedProperty isPercentageProp;

    private void OnEnable()
    {
        buffNameProp     = serializedObject.FindProperty("buffName");
        buffTypeProp     = serializedObject.FindProperty("buffType");
        buffIconProp     = serializedObject.FindProperty("buffIcon");
        valueProp        = serializedObject.FindProperty("value");
        isPercentageProp = serializedObject.FindProperty("isPercentage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Buff 名称 ──
        EditorGUILayout.PropertyField(buffNameProp, new GUIContent("Buff 名称"));

        EditorGUILayout.Space();

        // ── Buff 类型 ──
        EditorGUILayout.PropertyField(buffTypeProp, new GUIContent("属性类型"));
        var statType = (StatType)buffTypeProp.enumValueIndex;

        // ── Buff 图标 ──
        EditorGUILayout.PropertyField(buffIconProp, new GUIContent("Buff 图标"));

        EditorGUILayout.Space();

        // ── 百分比模式 ──
        EditorGUILayout.PropertyField(isPercentageProp, new GUIContent("百分比模式"));
        bool isPct = isPercentageProp.boolValue;

        // ── 数值 ──
        string valueLabel = GetValueLabel(statType, isPct);
        EditorGUILayout.PropertyField(valueProp, new GUIContent(valueLabel));

        // ── 效果预览 ──
        string helpText = GetHelpText(statType, valueProp.floatValue, isPct);
        if (!string.IsNullOrEmpty(helpText))
        {
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static string GetValueLabel(StatType type, bool isPercentage)
    {
        string suffix = isPercentage ? "比例 (0.3=30%)" : "绝对值";
        return type switch
        {
            StatType.HP     => $"治疗量 ({suffix})",
            StatType.MAXHP  => $"最大 HP 加成 ({suffix})",
            StatType.MP     => $"回蓝量 ({suffix})",
            StatType.MAXMP  => $"最大 MP 加成 ({suffix})",
            StatType.ATK    => $"攻击力加成 ({suffix})",
            StatType.DEF    => $"防御力加成 ({suffix})",
            StatType.SPD    => $"速度加成 ({suffix})",
            _               => $"数值 ({suffix})",
        };
    }

    private static string GetHelpText(StatType type, float value, bool isPercentage)
    {
        if (isPercentage)
        {
            int pct = Mathf.RoundToInt(value * 100f);
            return type switch
            {
                StatType.HP     => $"每回合回复最大 HP 的 {pct}%",
                StatType.MAXHP  => $"最大 HP +{pct}%",
                StatType.MP     => $"每回合回复最大 MP 的 {pct}%",
                StatType.MAXMP  => $"最大 MP +{pct}%",
                StatType.ATK    => $"攻击力 +{pct}%",
                StatType.DEF    => $"护盾值 +{pct}%",
                StatType.SPD    => $"速度 +{pct}%",
                _               => $"+{pct}%",
            };
        }
        else
        {
            string sign = value >= 0 ? "+" : "";
            return type switch
            {
                StatType.HP     => $"{(value >= 0 ? "治疗" : "扣除")} {Mathf.Abs(value)} 点 HP",
                StatType.MAXHP  => $"最大 HP {sign}{value}",
                StatType.MP     => $"回复 {value} 点 MP",
                StatType.MAXMP  => $"最大 MP {sign}{value}",
                StatType.ATK    => $"攻击力 {sign}{value}",
                StatType.DEF    => $"护盾值 {sign}{value}",
                StatType.SPD    => $"速度 {sign}{value}",
                _               => $"数值: {sign}{value}",
            };
        }
    }
}
