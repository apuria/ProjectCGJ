using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInfo : ScriptableObject
{
//基础信息
/*
1. 攻击特效 + 敌人受击特效
2. 立绘
3. 头像
4. 缩放比例
*/
    public string Name;

    public Sprite avatar;
    public Sprite icon;
    public float scale = 1.0f;
    public Stat attack;
    public Stat maxHp;
    public Stat hp;
    public Stat maxMp;
    public Stat mp;
    public Stat speed;
    public List<SkillInfo> skills;
    public SkillInfo ultimateSkill;
    public SkillInfo normalAttack;
    
    /// <summary>
    /// 防御时获得的护盾值
    /// </summary>
    public Stat shieldValue;

    /// <summary>
    /// 临时防御值（当前回合有效，回合结束时清零）
    /// </summary>
    [System.NonSerialized]
    public float tempDefense;

    public GameObject attackEffect;
    public GameObject hitEffect;

    private Dictionary<BuffInfo, int> buffs = new();

    public void AddBuff(BuffInfo buff)
    {
        if (buffs.ContainsKey(buff))
        {
            buffs[buff] = buff.duration;
            return;
        }
        buffs.Add(buff, buff.duration);
        BuffChangeValue(buff.buffType, buff.value);
    }

    public void RemoveBuff(BuffInfo buff)
    {
        if (buffs == null)
            return;
        if(!buffs.ContainsKey(buff))
            return;
        buffs.Remove(buff);
        BuffChangeValue(buff.buffType, -buff.value);
    }

    private void BuffChangeValue(StatType type, float value)
    {
        switch (type)
        {
            case StatType.MAXHP:
                maxHp.value += value;
                break;
            case StatType.HP:
                hp.value += value;
                if(hp.value > maxHp.value)
                    hp.value = maxHp.value;
                break;
            case StatType.MAXMP:  
                maxMp.value += value;
                break;
            case StatType.MP:   
                mp.value += value;
                if (mp.value > maxMp.value)
                    mp.value = maxMp.value;
                break;
            case StatType.ATK:
                attack.value += value;
                break;
            case StatType.DEF:
                shieldValue.value += value;
                break;
            case StatType.SPD:  
                speed.value += value;
                break;
        }
    }

}
