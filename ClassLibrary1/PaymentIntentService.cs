namespace ClassLibrary1;

// Servicio
//Aca podemos usar los serviceResponse, pero para el ejemplo sirve
public interface IPaymentIntentService
{
    Task CreateAsync(Guid id, decimal amount, string currency);
    Task FailAsync(Guid id, string reason);
    Task FinishAsync(Guid id);
    Task<PaymentIntent> GetByIdAsync(Guid id);
}
public class PaymentIntentService : IPaymentIntentService
{
    private readonly IPaymentIntentRepository _repository;

    public PaymentIntentService(IPaymentIntentRepository repository)
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

    public async Task<PaymentIntent> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
