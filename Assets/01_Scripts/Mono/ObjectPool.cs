using System.Collections.Concurrent;

public class ObjectPool<T> where T : class
{
    private ConcurrentStack<T> inactiveObjectsConcurrent = new();

    public T GetPooledObject ()
    {
        if(!inactiveObjectsConcurrent.TryPop(out T item))
        {
            return null;
        }
        return item;
    }

    public void PoolObject ( T item )
    {
        if ( item == null )
            return;

        inactiveObjectsConcurrent.Push(item);
    }
}