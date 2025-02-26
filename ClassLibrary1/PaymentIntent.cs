using Microsoft.Extensions.Logging;

namespace ClassLibrary1;

// Aggregate Root es el objeto que vamos a persistir y que va a tener los eventos aplicados
public class PaymentIntent : Aggregate<PaymentIntentEvent>,
    IEventVisitor
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

    public void Visit(PaymentIntentCreated e)
    {
        PaymentIntentId = e.PaymentIntentId;
        Amount = e.Amount;
        Currency = e.Currency;
        Status = "Created";
    }

    public void Visit(PaymentIntentFailed e)
    {
        Status = "Failed";
    }

    public void Visit(PaymentIntentFinished e)
    {
        if (Status == "Failed")
            throw new InvalidOperationException("Cannot finish a failed payment.");
        Status = "Finished";
    }

    public void Visit(PaymentIntentPending e)
    {
        Status = "Pending";
        // more logic
    }

    public void Visit(PaymentIntentRequired e)
    {
        Status = "Required";
        // more logic
    }
}