using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleState : MonoBehaviour
{
    public Button controller;
    public List<StateSlot> stateSlots;

    /// <summary>
    /// 更新角色状态UI：跳过第一位（行动角色），按从上到下显示其余角色，空位隐藏
    /// </summary>
    public void UpdateUI(List<RoleInfo> roles)
    {
        // 除第一位行动角色外，其他角色的数量
        int otherCount = roles != null ? roles.Count - 1 : 0;

        for (int i = 0; i < stateSlots.Count; i++)
        {
            if (i < otherCount)
            {
                RoleInfo role = roles[i + 1]; // 跳过 index 0（行动角色）
                stateSlots[i].gameObject.SetActive(true);
                float hpPct = role.maxHp.value > 0 ? role.hp.value / role.maxHp.value : 0f;
                float mpPct = role.maxMp.value > 0 ? role.mp.value / role.maxMp.value : 0f;
                stateSlots[i].UpdateUI(role.avatar, role.Name, hpPct, mpPct);
            }
            else
            {
                stateSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
