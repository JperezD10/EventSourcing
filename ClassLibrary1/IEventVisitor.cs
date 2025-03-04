﻿namespace ClassLibrary1;

public interface IEventVisitor
{
    void Visit(PaymentIntentCreated e);
    void Visit(PaymentIntentFailed e);
    void Visit(PaymentIntentFinished e);
    void Visit(PaymentIntentPending e);
    void Visit(PaymentIntentRequired e);
}
