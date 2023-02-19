using GeekShopping.Email.Messages;
using GeekShopping.Email.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.Email.MessageConsumer;

public class RabbitMQPaymentConsumer : BackgroundService {
    private readonly EmailRepository _repository;
    private IConnection _connection;
    private IModel _channel;
    private const string ExchangeName = "DirectPaymentUpdateExchange";
    private const string PaymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";

    public RabbitMQPaymentConsumer(EmailRepository repository) {
        this._repository = repository;
        ConnectionFactory factory = new() {
            HostName = "localhost",
            Password = "guest",
            UserName = "guest"
        };
        this._connection = factory.CreateConnection();
        this._channel = this._connection.CreateModel();
        this._channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
        this._channel.QueueDeclare(PaymentEmailUpdateQueueName, false, false, false, null);
        this._channel.QueueBind(PaymentEmailUpdateQueueName, ExchangeName, "PaymentEmail");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        stoppingToken.ThrowIfCancellationRequested();
        EventingBasicConsumer consumer = new (this._channel);
        consumer.Received += (channel, evt) => {
            var content = Encoding.UTF8.GetString(evt.Body.ToArray());
            UpdatePaymentResultMessage message = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(content);
            ProccessLogs(message).GetAwaiter().GetResult();
            this._channel.BasicAck(evt.DeliveryTag, false);
        };
        this._channel.BasicConsume(PaymentEmailUpdateQueueName, false, consumer);
        return Task.CompletedTask;
    }

    private async Task ProccessLogs(UpdatePaymentResultMessage message) {
        try {
            await this._repository.LogEmail(message);
        } catch (Exception) {
            throw;
        }
    }
}