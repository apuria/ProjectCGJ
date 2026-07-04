namespace UniFramework.Machine
{
    public interface IStateNode
    {
        /// <summary>
        /// 创建时绑定的数据，由 StateMachine 在状态创建时注入
        /// </summary>
        void OnCreate(StateMachine machine, IStateData stateData);
        void OnDispose();
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }
}
