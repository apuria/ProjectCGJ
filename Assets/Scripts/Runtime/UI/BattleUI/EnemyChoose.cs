using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyChoose : MonoBehaviour
{
    public Button controller;
    public List<EnemyChooseSlot> enemyChooseSlots;
    /// <summary>
    /// 可选：面板标题文本，会在切换敌人/队友模式时更新
    /// </summary>
    public TextMeshProUGUI titleText;

    /// <summary>
    /// 当前是否处于队友选择模式
    /// </summary>
    public bool IsAllyMode { get; private set; }

    /// <summary>
    /// 更新敌人选择UI：对应站位的敌人不存在或已死亡则隐藏该slot
    /// </summary>
    public void UpdateUI(List<EnemyInfo> enemies)
    {
        IsAllyMode = false;
        if (titleText != null) titleText.text = "选择敌人";

        for (int i = 0; i < enemyChooseSlots.Count; i++)
        {
            if (i < enemies.Count && enemies[i] != null && enemies[i].hp.value > 0)
            {
                int enemyIndex = i;
                enemyChooseSlots[i].gameObject.SetActive(true);
                enemyChooseSlots[i].UpdateUI(enemies[i].avatar);

                // 绑定点击事件：选中目标敌人
                enemyChooseSlots[i].button.onClick.RemoveAllListeners();
                enemyChooseSlots[i].button.onClick.AddListener(() =>
                {
                    BattleEventDefine.SelectEnemy.SendEventMessage(enemyIndex);
                });
            }
            else
            {
                enemyChooseSlots[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 更新队友选择UI：显示存活队友，排除当前行动角色自身
    /// </summary>
    /// <param name="roles">角色列表</param>
    /// <param name="excludeIndex">排除的角色索引（当前行动角色自身，不可选）</param>
    public void UpdateUIForAllies(List<RoleInfo> roles, int excludeIndex = -1)
    {
        IsAllyMode = true;
        if (titleText != null) titleText.text = "选择队友";

        for (int i = 0; i < enemyChooseSlots.Count; i++)
        {
            if (i < roles.Count && roles[i] != null && roles[i].hp.value > 0 && i != excludeIndex)
            {
                int allyIndex = i;
                enemyChooseSlots[i].gameObject.SetActive(true);
                enemyChooseSlots[i].UpdateUI(roles[i].avatar);

                // 绑定点击事件：选中目标队友
                enemyChooseSlots[i].button.onClick.RemoveAllListeners();
                enemyChooseSlots[i].button.onClick.AddListener(() =>
                {
                    BattleEventDefine.SelectAlly.SendEventMessage(allyIndex);
                });
            }
            else
            {
                enemyChooseSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
