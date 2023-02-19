using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.RabbitMQSender;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer;

public class RabbitMQCheckoutConsumer : BackgroundService {
    private readonly OrderRepository _repository;
    private IConnection _connection;
    private IModel _channel;
    private IRabbitMQMessageSender _rabbitMQMessageSender;

    public RabbitMQCheckoutConsumer(
        OrderRepository repository,
        IRabbitMQMessageSender rabbitMQMessageSender
    ) {
        this._repository = repository;
        this._rabbitMQMessageSender = rabbitMQMessageSender;
        ConnectionFactory factory = new() {
            HostName = "localhost",
            Password = "guest",
            UserName = "guest"
        };
        this._connection = factory.CreateConnection();
        this._channel = this._connection.CreateModel();
        this._channel.QueueDeclare("checkoutqueue", false, false, false, null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        stoppingToken.ThrowIfCancellationRequested();
        EventingBasicConsumer consumer = new (this._channel);
        consumer.Received += (channel, evt) => {
            var content = Encoding.UTF8.GetString(evt.Body.ToArray());
            CheckoutHeaderVO checkoutHeader = JsonSerializer.Deserialize<CheckoutHeaderVO>(content);
            ProccessOrder(checkoutHeader).GetAwaiter().GetResult();
            this._channel.BasicAck(evt.DeliveryTag, false);
        };
        this._channel.BasicConsume("checkoutqueue", false, consumer);
        return Task.CompletedTask;
    }

    private async Task ProccessOrder(CheckoutHeaderVO checkoutHeader) {
        OrderHeader order = new() {
            UserId = checkoutHeader.UserId,
            FirstName = checkoutHeader.FirstName,
            LastName = checkoutHeader.LastName,
            OrderDetails = new List<OrderDetail>(),
            CardNumber = checkoutHeader.CardNumber,
            CouponCode = checkoutHeader.CouponCode,
            CVV = checkoutHeader.CVV,
            DiscountAmount = checkoutHeader.DiscountAmount,
            Email = checkoutHeader.Email,
            ExpiryMonthYear = checkoutHeader.ExpiryMonthYear,
            OrderTime = DateTime.Now,
            PurchaseAmount = checkoutHeader.PurchaseAmount,
            PaymentStatus = false,
            Phone = checkoutHeader.Phone,
            DateTime = checkoutHeader.DateTime
        };

        foreach (CartDetailVO detail in checkoutHeader.CartDetails) {
            order.CartTotalItens += detail.Count;
            order.OrderDetails.Add(new() {
                ProductId = detail.ProductId,
                ProductName = detail.Product.Name,
                Price = detail.Product.Price,
                Count = detail.Count
            });
        }
        await this._repository.AddOrder(order);
        PaymentVO payment = new() {
            Name = $"{order.FirstName} {order.LastName}",
            CardNumber = order.CardNumber,
            CVV = order.CVV,
            ExpiryMonthYear = order.ExpiryMonthYear,
            OrderId = order.Id,
            PurchaseAmount = order.PurchaseAmount,
            Email = order.Email
        };
        try {
            this._rabbitMQMessageSender.SendMessage(payment, "orderpaymentprocessqueue");
        } catch (Exception) {
            // Log
            throw;
        }
    }
}