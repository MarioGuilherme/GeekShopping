using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer;

public class RabbitMQPaymentConsumer : BackgroundService {
    private readonly OrderRepository _repository;
    private IConnection _connection;
    private IModel _channel;
    private const string ExchangeName = "DirectPaymentUpdateExchange";
    private const string PaymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";

    public RabbitMQPaymentConsumer(OrderRepository repository) {
        this._repository = repository;
        ConnectionFactory factory = new() {
            HostName = "localhost",
            Password = "guest",
            UserName = "guest"
        };
        this._connection = factory.CreateConnection();
        this._channel = this._connection.CreateModel();
        this._channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
        this._channel.QueueDeclare(PaymentOrderUpdateQueueName, false, false, false, null);
        this._channel.QueueBind(PaymentOrderUpdateQueueName, ExchangeName, "PaymentOrder");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        stoppingToken.ThrowIfCancellationRequested();
        EventingBasicConsumer consumer = new (this._channel);
        consumer.Received += (channel, evt) => {
            var content = Encoding.UTF8.GetString(evt.Body.ToArray());
            UpdatePaymentResultVO checkoutHeader = JsonSerializer.Deserialize<UpdatePaymentResultVO>(content);
            UpdatePaymentStatus(checkoutHeader).GetAwaiter().GetResult();
            this._channel.BasicAck(evt.DeliveryTag, false);
        };
        this._channel.BasicConsume(PaymentOrderUpdateQueueName, false, consumer);
        return Task.CompletedTask;
    }

    private async Task UpdatePaymentStatus(UpdatePaymentResultVO checkoutHeader) {
        try {
            await this._repository.UpdateOrderPaymentStatus(checkoutHeader.OrderId, checkoutHeader.Status);
        } catch (Exception) {
            throw;
        }
    }
}