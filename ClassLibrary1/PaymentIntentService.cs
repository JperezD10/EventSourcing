namespace ClassLibrary1;

// Servicio
//Aca podemos usar los serviceResponse, pero para el ejemplo sirve
public interface IPaymentIntentService
{
    Task CreateAsync(Guid id, decimal amount, string currency);
    Task FailAsync(Guid id, string reason);
    Task FinishAsync(Guid id);
    Task PendingAsync(Guid id);
    Task<PaymentIntent> GetByIdAsync(Guid id);
}
public class PaymentIntentService : IPaymentIntentService
{
    private readonly IPaymentIntentRepository _repository;

    public PaymentIntentService(IPaymentIntentRepository repository)
    {
        _repository = repository;
    }

    private async Task ChangeStatusAsync(Guid id, PaymentIntentEvent @event)
    {
        var intent = await _repository.GetByIdAsync(id);
        intent.ApplyChange(@event);
        await _repository.SaveAsync(intent);
    }
    public async Task CreateAsync(Guid id, decimal amount, string currency)
    {
        var intent = PaymentIntent.Create(id, amount, currency);
        await _repository.SaveAsync(intent);
    }

    public async Task FailAsync(Guid id, string reason)
    {
        await ChangeStatusAsync(id, new PaymentIntentFailed(id, reason));
    }

    public async Task FinishAsync(Guid id)
    {
        await ChangeStatusAsync(@id, new PaymentIntentFinished(id));
    }

    public async Task PendingAsync(Guid id)
    {
        await ChangeStatusAsync(id, new PaymentIntentPending(id));
    }

    public async Task<PaymentIntent> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
