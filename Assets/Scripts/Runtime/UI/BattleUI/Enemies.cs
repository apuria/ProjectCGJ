using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    public List<EnemySlot> enemySlots;

    /// <summary>
    /// 更新敌人立绘：对应站位的敌人不存在或已死亡则隐藏该slot
    /// </summary>
    public void UpdateUI(List<EnemyInfo> enemies)
    {
        for (int i = 0; i < enemySlots.Count; i++)
        {
            if (i < enemies.Count && enemies[i] != null && enemies[i].hp.value > 0)
            {
                enemySlots[i].gameObject.SetActive(true);
                enemySlots[i].UpdateUI(enemies[i].icon, enemies[i].scale);
            }
            else
            {
                enemySlots[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 激活指定索引敌人的 Tip，其余失活
    /// </summary>
    public void SetEnemyTipActive(int enemyIndex)
    {
        for (int i = 0; i < enemySlots.Count; i++)
        {
            if (enemySlots[i] != null)
                enemySlots[i].SetTipActive(i == enemyIndex);
        }
    }

    /// <summary>
    /// 失活所有敌人的 Tip
    /// </summary>
    public void DeactivateAllTips()
    {
        foreach (var slot in enemySlots)
        {
            if (slot != null)
                slot.SetTipActive(false);
        }
    }
}
