using MongoDB.Driver;
namespace ClassLibrary1;

public class EventStore
{
    private readonly IMongoCollection<PaymentIntentEvent> _events;

    public EventStore(IMongoDatabase database)
    {
        _events = database.GetCollection<PaymentIntentEvent>("PaymentIntent");
    }

    public async Task SaveAsync(Guid aggregateId, IEnumerable<PaymentIntentEvent> events)
    {
        if (events.Any())
        {
            await _events.InsertManyAsync(events);
        }
    }

    public async Task<List<PaymentIntentEvent>> GetByAggregateIdAsync(Guid aggregateId)
    {
        return await _events.Find(e => e.PaymentIntentId == aggregateId)
                             .SortBy(e => e.OccurredOn) // Asegurar orden cronológico
                             .ToListAsync();
    }
}