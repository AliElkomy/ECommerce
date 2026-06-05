using MassTransit;
using Shared.Contracts.Commands;
using Shared.Contracts.Events;

namespace OrderService.Api.Sagas;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Pending { get; private set; }
    public State Invoiced { get; private set; }
    public State Cancelled { get; private set; }

    public Event<OrderPlacedEvent> OrderPlaced { get; private set; }
    public Event<InvoiceCreatedEvent> InvoiceCreated { get; private set; }
    public Event<InvoiceFailedEvent> InvoiceFailed { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => InvoiceCreated, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => InvoiceFailed, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderPlaced)
                .Then(ctx =>
                {
                    ctx.Saga.UserId = ctx.Message.UserId;
                    ctx.Saga.ProductId = ctx.Message.ProductId;
                    ctx.Saga.Quantity = ctx.Message.Quantity;
                    ctx.Saga.UnitPrice = ctx.Message.UnitPrice;
                    ctx.Saga.CreatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Pending)
                .Publish(ctx => new CreateInvoiceCommand
                {
                    OrderId = ctx.Saga.CorrelationId,
                    UserId = ctx.Saga.UserId,
                    ProductId = ctx.Saga.ProductId,
                    Quantity = ctx.Saga.Quantity,
                    UnitPrice = ctx.Saga.UnitPrice
                })
        );

        During(Pending,
            When(InvoiceCreated)
                .TransitionTo(Invoiced),

            When(InvoiceFailed)
                .TransitionTo(Cancelled)
                .Publish(ctx => new ReleaseInventoryCommand { OrderId = ctx.Saga.CorrelationId })
                .Publish(ctx => new NotifyUserCommand
                {
                    UserId = ctx.Saga.UserId,
                    Reason = "Invoice failed — order cancelled"
                })
        );
    }
}
