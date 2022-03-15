using UnityEngine;

public class BulletObjectPool : ObjectPool<Bullet>
{
    private Bullet prefab;
    private GameObject containerGameObject;

    public BulletObjectPool(int size, Bullet prefab, Transform parent = null) : base(size)
    {
        this.prefab = prefab;

        containerGameObject = new GameObject("[ObjectPool] Bullets");
        Object.DontDestroyOnLoad(containerGameObject);
        if (parent != null)
            containerGameObject.transform.SetParent(parent);
    }

    protected override Bullet InstantiateNewPoolableObject()
    {
        Bullet instance = Object.Instantiate(prefab, containerGameObject.transform);
        instance.BecomeInactive();
        return instance;
    }

    public override void Dispose()
    {
        base.Dispose();
        if(containerGameObject != null)
            Object.Destroy(containerGameObject.gameObject);
    }
}
