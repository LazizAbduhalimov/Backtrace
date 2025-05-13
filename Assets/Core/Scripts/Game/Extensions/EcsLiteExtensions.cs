using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public static class EcsLiteExtensions
{
    public static bool First<T>(this EcsPool<T> pool, out T? component) where T: struct
    {
        var filter = pool.GetWorld().Filter<T>().End();
        foreach (var i in filter)
        {
            component = pool.Get(i);
            return true;
        }
        component = null;
        return false;
    }
    
    public static bool GetFirstIfExists<T>(this EcsFilter filter, out T component) where T: struct
    {
        var pool = filter.GetWorld().GetPool<T>();
        foreach (var i in filter)
        {
            component = pool.Get(i);
            return true;
        }
        component = default;
        return false;
    }
    
    public static bool GetFirstIfExists<T>(this EcsFilterInject<Inc<T>> filter, out T component) where T: struct
    {
        foreach (var i in filter.Value)
        {
            component = filter.Pools.Inc1.Get(i);
            return true;
        }
        component = default;
        return false;
    }

    public static ref T NewEntity<T>(this EcsPool<T> pool, out int entity) where T : struct
    {
        entity = pool.GetWorld().NewEntity();
        return ref pool.Add(entity);
    }
    
    public static bool HasEntity(this EcsFilter filter)
    {
        return filter.GetEntitiesCount() != 0;
    }

    public static void Clear<T>(this EcsPool<T> pool) where T : struct
    {
        var filter = pool.GetWorld().Filter<T>().End();
        foreach (var entity in filter)
        {
            pool.Del(entity);
        }
    }

    public static void DelFromPool<T>(this EcsPackedEntity packedEntity, EcsWorld world) where T: struct
    {
        if (packedEntity.Unpack(world, out var entity))
        {
            world.GetPool<T>().Del(entity);
        }
    }

    public static bool TryAdd<T>(this EcsPool<T> pool, int entity, out T component) where T : struct
    {
        var exists = pool.Has(entity);
        if (!exists)
        {
            component = pool.Add(entity);
        }

        component = default;
        return exists;
    }
}
