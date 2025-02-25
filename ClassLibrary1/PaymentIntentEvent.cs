using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ClassLibrary1;

[BsonDiscriminator(Required = true)]
[BsonKnownTypes(
    typeof(PaymentIntentCreated), 
    typeof(PaymentIntentFailed),
    typeof(PaymentIntentPending),
    typeof(PaymentIntentFinished))]
public abstract class PaymentIntentEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid EventId { get; private set; } = Guid.NewGuid(); // Identificador único del evento

    [BsonRepresentation(BsonType.String)]
    public Guid PaymentIntentId { get; private set; } // Identificador del PaymentIntent

    public DateTime OccurredOn { get; private set; }

    protected PaymentIntentEvent() { } // Constructor vacío necesario para MongoDB

    protected PaymentIntentEvent(Guid id, DateTime occurredOn)
    {
        PaymentIntentId = id;
        OccurredOn = occurredOn;
    }
}


public class PaymentIntentCreated : PaymentIntentEvent, IAcceptEventVisitor
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private PaymentIntentCreated() { } // Necesario para la deserialización de MongoDB

    public PaymentIntentCreated(Guid id, decimal amount, string currency)
        : base(id, DateTime.UtcNow)
    {
        Amount = amount;
        Currency = currency;
    }

    public void Accept(IEventVisitor visitor) => visitor.Visit(this);
}

public class PaymentIntentFailed : PaymentIntentEvent, IAcceptEventVisitor
{
    public string Reason { get; private set; }

    private PaymentIntentFailed() { } // Necesario para la deserialización

    public PaymentIntentFailed(Guid id, string reason)
        : base(id, DateTime.UtcNow)
    {
        Reason = reason;
    }

    public void Accept(IEventVisitor visitor) => visitor.Visit(this);
}

public class PaymentIntentFinished : PaymentIntentEvent, IAcceptEventVisitor
{
    private PaymentIntentFinished() { } // Necesario para MongoDB

    public PaymentIntentFinished(Guid id)
        : base(id, DateTime.UtcNow) { }

    public void Accept(IEventVisitor visitor) => visitor.Visit(this);
}
public class PaymentIntentPending: PaymentIntentEvent, IAcceptEventVisitor
{
    private PaymentIntentPending() { } // Necesario para MongoDB

    public PaymentIntentPending(Guid id)
        : base(id, DateTime.UtcNow) { }

    public void Accept(IEventVisitor visitor) => visitor.Visit(this);
}