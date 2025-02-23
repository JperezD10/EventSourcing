namespace ClassLibrary1;

public interface IAcceptEventVisitor
{
    void Accept(IEventVisitor visitor);
}
