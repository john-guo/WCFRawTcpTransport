namespace WCFRawTcpTransport
{
    public interface IObjectPool<T> where T : class, new()
    {
        void Return(T item);
        T Take();
    }
}