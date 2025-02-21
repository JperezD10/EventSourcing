// Por lo que vi esta es la db que hay que usar para mongo (validar)
using MongoDB.Driver;
public abstract class Aggregate<TEvent> where TEvent : class
{
    private readonly List<AggregateChange<TEvent>> _changes = new();
    public Guid Id { get; protected set; }
    public int Version { get; private set; } = 0;
    protected string AggregateType => GetType().Name;

    public IReadOnlyCollection<TEvent> GetUncommittedChanges() => _changes.Where(c => c.IsNew).Select(c => c.Event).ToList().AsReadOnly();
    public void MarkChangesAsCommitted() => _changes.ForEach(c => c.MarkCommitted());

    protected void ApplyChange(TEvent @event)
    {
        ((IApply<TEvent>)this).Apply(@event);
        _changes.Add(new AggregateChange<TEvent>(@event, true, AggregateType));
        Version++;
    }

    public void LoadFromHistory(IEnumerable<TEvent> history)
    {
        foreach (var @event in history)
        {
            ((IApply<TEvent>)this).Apply(@event);
            _changes.Add(new AggregateChange<TEvent>(@event, false, AggregateType));
            Version++;
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


// Esta interfaz es para poder aplicar los eventos a los objetos
public interface IApply<TEvent>
{
    void Apply(TEvent @event);
}

// Eventos
public abstract record PaymentIntentEvent(Guid Id, DateTime OccurredOn);

public record PaymentIntentCreated(Guid Id, decimal Amount, string Currency) : PaymentIntentEvent(Id, DateTime.UtcNow);
public record PaymentIntentFailed(Guid Id, string Reason) : PaymentIntentEvent(Id, DateTime.UtcNow);
public record PaymentIntentFinished(Guid Id) : PaymentIntentEvent(Id, DateTime.UtcNow);

// Aggregate Root es el objeto que vamos a persistir y que va a tener los eventos aplicados
public class PaymentIntent : Aggregate<PaymentIntentEvent>, 
    IApply<PaymentIntentCreated>,
    IApply<PaymentIntentFailed>,
    IApply<PaymentIntentFinished>
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public string Status { get; private set; }

    //Bloqueamos la creacion para que use nuestras reglas de negocio
    private PaymentIntent() { }

    public static PaymentIntent Create(Guid id, decimal amount, string currency)
    {
        var intent = new PaymentIntent();
        intent.ApplyChange(new PaymentIntentCreated(id, amount, currency));
        return intent;
    }

    //Para poder listar los eventos y rehidratar el objeto (y poder cargar desde la db)
    public static PaymentIntent Rehydrate(IEnumerable<PaymentIntentEvent> history)
    {
        var intent = new PaymentIntent();
        intent.LoadFromHistory(history);
        return intent;
    }

    //estos apply son llamados desde la clase de arriba porque implementa IApply
    //recuerden la linea ((IApply<TEvent>)this).Apply(@event); en ApplyChange
    public void Apply(PaymentIntentCreated @event)
    {
        Id = @event.Id;
        Amount = @event.Amount;
        Currency = @event.Currency;
        Status = "Created";
    }

    //aca van validaciones y seteos boludas que puse para el ejemplo
    public void Apply(PaymentIntentFailed @event)
    {
        if (Status == "Finished")
            throw new InvalidOperationException("Cannot fail a finished payment.");
        Status = "Failed";
    }

    public void Apply(PaymentIntentFinished @event)
    {
        if (Status == "Failed")
            throw new InvalidOperationException("Cannot finish a failed payment.");
        Status = "Finished";
    }

    public void Fail(string reason) => ApplyChange(new PaymentIntentFailed(Id, reason));
    public void Finish() => ApplyChange(new PaymentIntentFinished(Id));
}

// Event Store (Simulado con MongoDB)
public class EventStore
{
    //mongoDb.Driver
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
        return await _events.Find(e => e.Id == aggregateId).ToListAsync();
    }
}

// Repository
public interface IPaymentIntentRepository
{
    Task<PaymentIntent> GetByIdAsync(Guid id);
    Task SaveAsync(PaymentIntent intent);
}
public class PaymentIntentRepository: IPaymentIntentRepository
{
    private readonly EventStore _eventStore;

    public PaymentIntentRepository(EventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<PaymentIntent> GetByIdAsync(Guid id)
    {
        var events = await _eventStore.GetByAggregateIdAsync(id);
        return PaymentIntent.Rehydrate(events);
    }

    public async Task SaveAsync(PaymentIntent intent)
    {
        await _eventStore.SaveAsync(intent.Id, intent.GetUncommittedChanges());
        intent.MarkChangesAsCommitted();
    }
}

// Servicio
//Aca podemos usar los serviceResponse, pero para el ejemplo sirve
public interface IPaymentIntentService
{
    Task CreateAsync(Guid id, decimal amount, string currency);
    Task FailAsync(Guid id, string reason);
    Task FinishAsync(Guid id);
}
public class PaymentIntentService: IPaymentIntentService
{
    private readonly PaymentIntentRepository _repository;

    public PaymentIntentService(PaymentIntentRepository repository)
    {
        _repository = repository;
    }

    public async Task CreateAsync(Guid id, decimal amount, string currency)
    {
        var intent = PaymentIntent.Create(id, amount, currency);
        await _repository.SaveAsync(intent);
    }

    public async Task FailAsync(Guid id, string reason)
    {
        var intent = await _repository.GetByIdAsync(id);
        intent.Fail(reason);
        await _repository.SaveAsync(intent);
    }

    public async Task FinishAsync(Guid id)
    {
        var intent = await _repository.GetByIdAsync(id);
        intent.Finish();
        await _repository.SaveAsync(intent);
    }
}
