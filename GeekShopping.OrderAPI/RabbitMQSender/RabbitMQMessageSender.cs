using GeekShopping.MessageBus;
using GeekShopping.OrderAPI.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.RabbitMQSender;

public class RabbitMQMessageSender : IRabbitMQMessageSender {
    private readonly string _hostName;
    private readonly string _password;
    private readonly string _useName;
    private IConnection _connection;

    public RabbitMQMessageSender() {
        this._hostName = "localhost";
        this._password = "guest";
        this._useName = "guest";
    }

    public void SendMessage(BaseMessage message, string queueName) {
        if (ConnectionExists()) {
            using var channel = this._connection.CreateModel();
            channel.QueueDeclare(queueName, false, false, false, null);
            byte[] body = GetMessageAsByteArray(message);
            channel.BasicPublish(string.Empty, queueName, null, body);
        }
    }

    private static byte[] GetMessageAsByteArray(BaseMessage message) {
        JsonSerializerOptions options = new () {
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(message as PaymentVO, options);
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