using Code.SubSystem.Minigame.RhythmQTE;

namespace Code.MainSystem.RhythmQTE
{
    /// <summary>
    /// QTE에 등장할 오브젝트가 상속할 인터페이스.
    /// 지금은 일반 하나를 추상화 하기 위해 사용중이다.
    /// 추후 추가하기 용의하게 만듬.
    /// </summary>
    public interface IQTEObject
    {
        public void Initailize(QTEController controller, float lifeTime);
    }
}