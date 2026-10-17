using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// 游戏门基类
    /// 自定义门系统从此类继承。
    /// </summary>
    public abstract class DoorBase : MonoBehaviour
    {
        [SerializeField]private State _doorState = State.Close;
        public State DoorState
        {
            get => _doorState;
            set => _doorState = value;
        }

        private int _doorId = -1;
        public int DoorID
        {
            get => _doorId;
            set => _doorId = value;
        }

        /// <summary>
        /// 在此函数中处理传入的门状态
        /// 应用该状态的旋转/位置，并带动画/补间
        /// </summary>
        public abstract void SetDoorState(State newState);

        /// <summary>
        /// 在此函数中处理传入的门状态
        /// 新玩家进入房间时由主客户端调用，用于同步门状态。
        /// 立即应用该状态的旋转/位置
        /// </summary>
        public abstract void SetDoorStateInstantly(State newState);

        /// <summary>
        /// 用法实现见 BasicDoor.cs
        /// </summary>
        public abstract void OnUpdateDoor();

        public enum State
        {
            Close,
            OpenToOutside,
            OpenToInside        
        }
    }
}