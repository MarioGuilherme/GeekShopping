using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.RabbitMQSender;
using GeekShopping.PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.MessageConsumer;

public class RabbitMQPaymentConsumer : BackgroundService {
    private IConnection _connection;
    private IModel _channel;
    private IRabbitMQMessageSender _rabbitMQMessageSender;
    private readonly IProcessPayment _processPayment;

    public RabbitMQPaymentConsumer(
        IProcessPayment processPayment,
        IRabbitMQMessageSender rabbitMQMessageSender
    ) {
        this._processPayment = processPayment;
        this._rabbitMQMessageSender = rabbitMQMessageSender;
        ConnectionFactory factory = new() {
            HostName = "localhost",
            Password = "guest",
            UserName = "guest"
        };
        this._connection = factory.CreateConnection();
        this._channel = this._connection.CreateModel();
        this._channel.QueueDeclare("orderpaymentprocessqueue", false, false, false, null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        stoppingToken.ThrowIfCancellationRequested();
        EventingBasicConsumer consumer = new (this._channel);
        consumer.Received += (channel, evt) => {
            string content = Encoding.UTF8.GetString(evt.Body.ToArray());
            PaymentMessage paymentMessage = JsonSerializer.Deserialize<PaymentMessage>(content);
            ProcessPayment(paymentMessage).GetAwaiter().GetResult();
            this._channel.BasicAck(evt.DeliveryTag, false);
        };
        this._channel.BasicConsume("orderpaymentprocessqueue", false, consumer);
        return Task.CompletedTask;
    }

    private async Task ProcessPayment(PaymentMessage checkoutHeader) {
        bool result = this._processPayment.PaymentPorcessor();
        UpdatePaymentResultMessage paymentResultMessage = new() {
            Status = result,
            OrderId = checkoutHeader.OrderId,
            Email = checkoutHeader.Email
        };
        try {
            this._rabbitMQMessageSender.SendMessage(paymentResultMessage);
        } catch (Exception) {
            throw;
        }
    }
}