using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skills : MonoBehaviour
{
    public Button controller;
    public List<SkillSlot> skillSlots;

    public void UpdateUI(List<SkillInfo> skills)
    {
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
}
