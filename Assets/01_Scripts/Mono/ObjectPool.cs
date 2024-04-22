using System.Collections.Generic;

public class ObjectPool<T> where T : class
{
    private class ObjectHolder
    {
        public List<T> objects = new();
    }

    private Dictionary<bool, ObjectHolder> objectsInPool = new();

    public T GetPooledObject ()
    {
        T item = null;

        if ( !objectsInPool.ContainsKey(false) )
            return item;

        item = objectsInPool[false].objects[0];
        objectsInPool[false].objects.Remove(item);

        if ( objectsInPool[false].objects.Count < 1 )
        {
            objectsInPool.Remove(false);
        }

        if ( !objectsInPool.ContainsKey(true) )
        {
            var newHolder = new ObjectHolder();
            newHolder.objects.Add(item);
            objectsInPool.Add(true, newHolder);
        }

        return item;
    }

    public void PoolObject ( T item )
    {
        if ( !objectsInPool.ContainsKey(false) )
        {
            var newHolder = new ObjectHolder();
            newHolder.objects.Add(item);
            objectsInPool.Add(false, newHolder);
        }
        else
        {
            objectsInPool[false].objects.Add(item);
        }
    }
}