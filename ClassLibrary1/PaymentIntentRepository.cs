namespace ClassLibrary1;

public interface IPaymentIntentRepository
{
    Task<PaymentIntent> GetByIdAsync(Guid id);
    Task SaveAsync(PaymentIntent intent);
}

public class PaymentIntentRepository : IPaymentIntentRepository
{
    private readonly EventStore _eventStore;

    public PaymentIntentRepository(EventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<PaymentIntent> GetByIdAsync(Guid id)
    {
        var events = await _eventStore.GetByAggregateIdAsync(id);
        return events.Any() ? PaymentIntent.Rehydrate(events) : null;
    }

    public async Task SaveAsync(PaymentIntent intent)
    {
        var newEvents = intent.GetUncommittedChanges();
        if (!newEvents.Any()) return; // No hay cambios nuevos, no guardamos nada

        await _eventStore.SaveAsync(intent.PaymentIntentId, newEvents);
        intent.MarkChangesAsCommitted();
    }
}