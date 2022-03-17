public interface IFactory<T> where T : class
{
    T Construct();
}
