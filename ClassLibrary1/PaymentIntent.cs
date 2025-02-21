namespace ClassLibrary1;

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
    public void Apply(PaymentIntentCreated @event)
    {
        PaymentIntentId = @event.PaymentIntentId;
        Amount = @event.Amount;
        Currency = @event.Currency;
        Status = "Created";
    }

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

    public void Fail(string reason) => ApplyChange(new PaymentIntentFailed(PaymentIntentId, reason));
    public void Finish() => ApplyChange(new PaymentIntentFinished(PaymentIntentId));
}
