using System.Collections.Generic;

public class ObjectPool<T> where T : class
{
    private readonly Stack<T> inactiveObjectsConcurrent = new();

    public bool GetPooledObject ( out T succes )
    {
        lock ( inactiveObjectsConcurrent )
        {
            if ( !inactiveObjectsConcurrent.TryPop(out succes) )
            {
                return false;
            }
        }
        return true;
    }

    public T[] GetAllPooledObjects ()
    {
        lock ( inactiveObjectsConcurrent )
        {
            return inactiveObjectsConcurrent.ToArray();
        }
    }

    public void ClearPool ()
    {
        lock ( inactiveObjectsConcurrent )
        {
            inactiveObjectsConcurrent.Clear();
        }
    }

    public void PoolObject ( T item )
    {
        if ( item == null )
            return;

        lock ( inactiveObjectsConcurrent )
        {
            inactiveObjectsConcurrent.Push(item);
        }
    }
}