using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SystemTools.SharedKernel;

namespace BackendCarcass.Database;

public sealed class DomainEventsDispatcher : IDomainEventsDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventsDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            MethodInfo handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle)) ??
                                      throw new Exception($"Handle method not found for {handlerType.Name}");

            foreach (object? handler in _serviceProvider.GetServices(handlerType))
            {
                if (handler is null)
                {
                    continue;
                }

                await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
            }
        }
    }
}
