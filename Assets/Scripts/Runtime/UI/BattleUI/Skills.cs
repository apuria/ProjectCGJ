using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skills : MonoBehaviour
{
    public Button controller;
    public List<SkillSlot> skillSlots;

    /// <summary>
    /// 当前显示的技能列表（用于蓝量刷新时重新判定可交互状态）
    /// </summary>
    private List<SkillInfo> currentSkills;

    public void UpdateUI(List<SkillInfo> skills)
    {
        currentSkills = skills;

        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i < skills.Count)
            {
                int idx = i;
                skillSlots[idx].gameObject.SetActive(true);
                skillSlots[idx].UpdateUI(skills[idx].skillIcon, skills[idx].skillName);

                // 绑定点击事件：通过索引安全捕获，避免闭包变量共享问题
                skillSlots[idx].button.onClick.RemoveAllListeners();
                skillSlots[idx].button.onClick.AddListener(() =>
                {
                    BattleEventDefine.SelectSkill.SendEventMessage(skills[idx]);
                });
            }
            else
            {
                skillSlots[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 根据当前 MP 值刷新每个技能槽的可交互状态
    /// 蓝量不足的技能槽变灰（Button.interactable = false），不可点击且不响应射线
    /// </summary>
    /// <param name="currentMp">当前角色的 MP 值</param>
    public void RefreshInteractable(int currentMp)
    {
        if (currentSkills == null) return;
        for (int i = 0; i < skillSlots.Count && i < currentSkills.Count; i++)
        {
            bool canAfford = currentMp >= currentSkills[i].mpCost;
            skillSlots[i].button.interactable = canAfford;
        }
    }
}
