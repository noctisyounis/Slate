namespace Database.Runtime.Interfaces
{
    public interface ISaveSystem
    {
        T Save<T>(T data);
        T Load<T>(); 
        string Path { get;}
        void SetPath(string path);
    }
}