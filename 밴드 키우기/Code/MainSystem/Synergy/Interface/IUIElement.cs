namespace Code.MainSystem.Synergy.Interface
{
    public interface IUIElement<T>
    {
        void EnableFor(T item);
        void Disable();
    }

    public interface IUIElement<T1, T2>
    {
        void EnableFor(T1 itemA, T2 itemB);
        void Disable();
    }
}