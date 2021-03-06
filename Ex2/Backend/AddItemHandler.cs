﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;

class AddItemHandler : IHandleMessages<AddItem>
{
    public async Task Handle(AddItem message, 
        IMessageHandlerContext context)
    {
        var dbContext = new OrdersDataContext();

        var order = await dbContext.Orders
            .FirstAsync(o => o.OrderId == message.OrderId);

        if (order.Lines.Any(x => x.Filling == message.Filling))
        {
            log.Info("Duplicate AddItem message detected. Ignoring.");
            return;
        }

        await context.PublishImmediately(
            new ItemAdded(message.OrderId, message.Filling));

        var line = new OrderLine(message.Filling);
        order.Lines.Add(line);

        await Task.Delay(1000);

        await dbContext.SaveChangesAsync();

        log.Info($"Item {message.Filling} added.");
    }

    static readonly ILog log = LogManager.GetLogger<AddItemHandler>();
}