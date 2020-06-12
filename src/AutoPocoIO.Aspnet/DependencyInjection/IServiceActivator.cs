namespace AutoPoco.DependencyInjection
{
    internal interface IServiceActivator
    {
        object Activate(IContainer container);
    }
}
