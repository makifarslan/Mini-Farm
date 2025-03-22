using Zenject;

public class ZenjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<ResourceManager>()
            .AsSingle() // Like singleton
            .NonLazy(); // NonLazy() is for immediate creation
    }
}