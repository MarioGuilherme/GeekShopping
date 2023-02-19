using GeekShopping.MessageBus;
using GeekShopping.PaymentAPI.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.RabbitMQSender;

public class RabbitMQMessageSender : IRabbitMQMessageSender {
    private readonly string _hostName;
    private readonly string _password;
    private readonly string _useName;
    private IConnection _connection;
    private const string ExchangeName = "DirectPaymentUpdateExchange";
    private const string PaymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";
    private const string PaymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";

    public RabbitMQMessageSender() {
        this._hostName = "localhost";
        this._password = "guest";
        this._useName = "guest";
    }

    public void SendMessage(BaseMessage message) {
        if (ConnectionExists()) {
            using var channel = this._connection.CreateModel();
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, false);
            channel.QueueDeclare(PaymentEmailUpdateQueueName, false, false, false, null);
            channel.QueueDeclare(PaymentOrderUpdateQueueName, false, false, false, null);
            channel.QueueBind(PaymentEmailUpdateQueueName, ExchangeName, "PaymentEmail");
            channel.QueueBind(PaymentOrderUpdateQueueName, ExchangeName, "PaymentOrder");
            byte[] body = GetMessageAsByteArray(message);
            channel.BasicPublish(ExchangeName, "PaymentEmail", null, body);
            channel.BasicPublish(ExchangeName, "PaymentOrder", null, body);
        }
    }

    private static byte[] GetMessageAsByteArray(BaseMessage message) {
        JsonSerializerOptions options = new () {
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(message as UpdatePaymentResultMessage, options);
        return Encoding.UTF8.GetBytes(json);
    }

    private void CreateConnection() {
        try {
            ConnectionFactory factory = new() {
                HostName = this._hostName,
                Password = this._password,
                UserName = this._useName
            };
            this._connection = factory.CreateConnection();
        } catch (Exception) {
            // Log
            throw;
        }
    }

    private bool ConnectionExists() {
        if (this._connection is not null)
            return true;
        CreateConnection();
        return this._connection is not null;
    }
}