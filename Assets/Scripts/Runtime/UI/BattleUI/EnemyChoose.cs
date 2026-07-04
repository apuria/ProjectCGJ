using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyChoose : MonoBehaviour
{
    public Button controller;
    public List<EnemyChooseSlot> enemyChooseSlots;

    /// <summary>
    /// 更新敌人选择UI：对应站位的敌人不存在或已死亡则隐藏该slot
    /// </summary>
    public void UpdateUI(List<EnemyInfo> enemies)
    {
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
}
