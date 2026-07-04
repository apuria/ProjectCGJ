using System.Collections.Generic;
using UnityEngine;

public class Roles : MonoBehaviour
{
    public List<RoleSlot> roleSlots;

    /// <summary>
    /// 普通更新，所有角色使用 avatar 立绘，空位隐藏
    /// </summary>
    public void UpdateUI(List<RoleInfo> roles)
    {
        for (int i = 0; i < roleSlots.Count; i++)
        {
            if (i < roles.Count && roles[i] != null)
            {
                roleSlots[i].gameObject.SetActive(true);
                roleSlots[i].UpdateUI(roles[i].icon, roles[i].scale);
                ApplyDeathColor(roleSlots[i], roles[i]);
            }
            else
            {
                roleSlots[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 我方行动时更新：第一位使用 CloseUp 特写图，其他使用 avatar，空位隐藏
    /// </summary>
    public void UpdateUIWithCloseUp(List<RoleInfo> roles)
    {
        for (int i = 0; i < roleSlots.Count; i++)
        {
            if (i < roles.Count && roles[i] != null)
            {
                roleSlots[i].gameObject.SetActive(true);
                Sprite sprite = (i == 0 && roles[i].CloseUp != null) ? roles[i].CloseUp : roles[i].icon;
                roleSlots[i].UpdateUI(sprite, roles[i].scale);
                ApplyDeathColor(roleSlots[i], roles[i]);
            }
            else
            {
                roleSlots[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 根据角色存活状态设置 slot 颜色：死亡 → 黑色，存活 → 白色
    /// </summary>
    private void ApplyDeathColor(RoleSlot slot, RoleInfo role)
    {
        bool isDead = role.hp.value <= 0;
        slot.icon.color = isDead ? new Color(0.2f, 0.2f, 0.2f) : Color.white;
    }
}
