// Por lo que vi esta es la db que hay que usar para mongo (validar)
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using ClassLibrary1;

public abstract class Aggregate<TEvent> where TEvent : class
{
    private readonly List<AggregateChange<TEvent>> _changes = new();
    [BsonRepresentation(BsonType.String)]
    public Guid EventId { get; private set; } = Guid.NewGuid(); // Identificador único del evento
    [BsonRepresentation(BsonType.String)]
    public Guid PaymentIntentId { get; protected set; }
    public int Version { get; private set; } = 0;
    protected string AggregateType => GetType().Name;

    public IReadOnlyCollection<TEvent> GetUncommittedChanges() => _changes.Where(c => c.IsNew).Select(c => c.Event).ToList().AsReadOnly();
    public void MarkChangesAsCommitted() => _changes.ForEach(c => c.MarkCommitted());

    public void ApplyChange(TEvent @event)
    {
        ApplyEvent(@event);
        _changes.Add(new AggregateChange<TEvent>(@event, true, AggregateType));
        Version++;
    }

    public void LoadFromHistory(IEnumerable<TEvent> history)
    {
        foreach (var @event in history)
        {
            ApplyEvent(@event);
            _changes.Add(new AggregateChange<TEvent>(@event, false, AggregateType));
            Version++;
        }
    }

    private void ApplyEvent(TEvent @event)
    {
        if (@event is IAcceptEventVisitor acceptor)
        {
            acceptor.Accept((IEventVisitor)this);
        }
        else
        {
            throw new InvalidOperationException($"No handler found for event {@event.GetType().Name}");
        }
    }
}

// Clase para rastrear si un evento es nuevo o viene del histórico
public class AggregateChange<TEvent> where TEvent : class
{
    public TEvent Event { get; }
    public bool IsNew { get; private set; }
    public string AggregateType { get; }

    public AggregateChange(TEvent @event, bool isNew, string aggregateType)
    {
        Event = @event;
        IsNew = isNew;
        AggregateType = aggregateType;
    }

    public void MarkCommitted() => IsNew = false;
}
