using System;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Shmap.UI
{
    public static class ContainerExtensions
    {
        public static IUnityContainer RegisterTypeAsSingleton<TFrom, TTo>(this IUnityContainer container, params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            return (container ?? throw new ArgumentNullException(nameof(container))).RegisterType(typeof(TFrom), typeof(TTo), null, new ContainerControlledLifetimeManager(), injectionMembers);
        }
    }
}