using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniFramework.Event;
using UniFramework.Tween;
using UniFramework.Log;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Boot : MonoBehaviour
{

//
/*
1. 将游戏管理器的状态机改为开始游戏
*/

    public GameObject effect;
    void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(this.gameObject);

        UniEvent.Initalize(); // 初始化事件系统（确保在 GameManager.Start 之前就绪）

        // 初始化对象池：传入自定义工厂，直接从 effect 引用实例化，不走 Resources
        PoolMgr.Initialize(enableLayout: true, prefabFactory: (name) =>
        {
            var prefab = Resources.Load<GameObject>(name);
            return Instantiate(prefab != null ? prefab : effect);
        });
    }

    void Start()
    {
        UniTween.Initalize(); // 初始化 Tween 动画系统
        UniLog.Initalize();   // 初始化文件日志系统
        GameManager.Instance.Init(); // 初始化游戏管理器
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 从对象池获取特效
            GameObject eff = PoolMgr.GetObj(effect.name);

            // 挂载到 TopLayer 下
            eff.transform.SetParent(UIMgr.Instance.TopLayer);

            // 将鼠标屏幕坐标转为世界坐标，直接赋值 position（避免 RectTransform 层级间的坐标系转换问题）
            Vector2 mousePos = Mouse.current.position.ReadValue();
            RectTransform canvasRect = UIMgr.Instance.UICanvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasRect,
                mousePos,
                UIMgr.Instance.UICanvas.worldCamera,
                out Vector3 worldPos);
            eff.transform.position = worldPos;
        }
    }

}
