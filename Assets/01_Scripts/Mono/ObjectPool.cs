using System.Collections.Concurrent;

public class ObjectPool<T> where T : class
{
    private readonly ConcurrentStack<T> inactiveObjectsConcurrent = new();

    public T GetPooledObject ()
    {
        if ( !inactiveObjectsConcurrent.TryPop(out T item) )
        {
            return null;
        }
        return item;
    }

    public T[] GetAllPooledObjects ()
    {
        return inactiveObjectsConcurrent.ToArray();
    }

    public void PoolObject ( T item )
    {
        if ( item == null )
            return;

        inactiveObjectsConcurrent.Push(item);
    }
}