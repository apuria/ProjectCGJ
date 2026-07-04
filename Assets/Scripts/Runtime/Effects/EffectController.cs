using System.Collections;
using UnityEngine;

/// <summary>
/// 特效控制器：挂载到特效预设体上，负责在特效播放完毕后自动归还对象池
/// </summary>
public class EffectController : MonoBehaviour
{
    private ParticleSystem[] _allPS;
    private float _cachedDuration = -1f;

    private void Awake()
    {
        // 缓存所有粒子系统引用和最大 duration，避免每次 OnEnable 重复计算
        _allPS = GetComponentsInChildren<ParticleSystem>();
        _cachedDuration = CalcMaxDuration();
    }

    private void OnEnable()
    {
        // 显式重新播放所有粒子系统（确保从池中取出时能正确播放）
        if (_allPS != null)
        {
            foreach (var ps in _allPS)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }
        }

        float delay = _cachedDuration > 0f ? _cachedDuration : 5f;
        StartCoroutine(ReturnToPool(delay));
    }

    private float CalcMaxDuration()
    {
        float max = 0f;
        if (_allPS == null) return 5f;

        foreach (var ps in _allPS)
        {
            var main = ps.main;
            if (main.loop) continue;
            float dur = main.duration;
            if (dur > max) max = dur;
        }

        return max > 0f ? max : 5f;
    }

    private IEnumerator ReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 去除 "(Clone)" 后缀，兼容直接 Instantiate 的方式
        string poolKey = gameObject.name.Replace("(Clone)", "").Trim();
        gameObject.name = poolKey;
        PoolMgr.PushObj(gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
