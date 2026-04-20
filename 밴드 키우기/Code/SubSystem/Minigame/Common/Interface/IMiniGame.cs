using Code.SubSystem.Minigame.Common.Contexts;

namespace Code.SubSystem.Minigame.Common.Interface
{
    public interface IMiniGame
    {
        /// <summary>
        /// 게임 초기화 및 시작
        /// </summary>
        void Initialize(MiniGameContext context);

        /// <summary>
        /// 게임 강제 종료
        /// </summary>
        void Terminate();
    }
}