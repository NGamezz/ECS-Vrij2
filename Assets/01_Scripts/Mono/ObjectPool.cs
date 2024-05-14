using System.Collections.Concurrent;

public class ObjectPool<T> where T : class
{
    private readonly ConcurrentStack<T> inactiveObjectsConcurrent = new();

    public T GetPooledObject ( out bool succes )
    {
        if ( !inactiveObjectsConcurrent.TryPop(out T item) )
        {
            succes = false;
            return null;
        }
        succes = true;
        return item;
    }

    public T[] GetAllPooledObjects ()
    {
        return inactiveObjectsConcurrent.ToArray();
    }

    public void ClearPool ()
    {
        inactiveObjectsConcurrent.Clear();
    }

    public void PoolObject ( T item )
    {
        if ( item == null )
            return;

        inactiveObjectsConcurrent.Push(item);
    }
}