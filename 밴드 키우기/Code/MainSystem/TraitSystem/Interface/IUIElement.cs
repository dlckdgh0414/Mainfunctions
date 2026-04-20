namespace Code.MainSystem.TraitSystem.Interface
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
    
    public interface IUIElement<T1, T2, T3>
    {
        void EnableFor(T1 itemA, T2 itemB, T3 itemC);
        void Disable();
    }
}