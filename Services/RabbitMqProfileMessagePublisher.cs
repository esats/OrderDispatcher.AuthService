using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using OrderDispatcher.AuthService.Configuration;
using OrderDispatcher.AuthService.Models;

namespace OrderDispatcher.AuthService.Services
{
    public class RabbitMqProfileMessagePublisher : IProfileMessagePublisher
    {
        private readonly RabbitMqOptions _options;
        private readonly IConnection _connection;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public RabbitMqProfileMessagePublisher(IOptions<RabbitMqOptions> options, IConnection connection)
        {
            _options = options.Value;
            _connection = connection;
        }

        public Task PublishProfileCreatedAsync(ProfileModel profile, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_options.QueueName))
                {
                    throw new InvalidOperationException("RabbitMQ queue name is not configured.");
                }

                if (string.IsNullOrWhiteSpace(_options.ExchangeName))
                {
                    throw new InvalidOperationException("RabbitMQ exchange name is not configured.");
                }

                if (string.IsNullOrWhiteSpace(_options.RoutingKey))
                {
                    throw new InvalidOperationException("RabbitMQ routing key is not configured.");
                }

                using var channel = _connection.CreateModel();

                channel.ExchangeDeclare(
                    exchange: _options.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                channel.QueueDeclare(
                    queue: _options.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.QueueBind(
                    queue: _options.QueueName,
                    exchange: _options.ExchangeName,
                    routingKey: _options.RoutingKey);

                var body = JsonSerializer.SerializeToUtf8Bytes(profile, JsonOptions);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(
                    exchange: _options.ExchangeName,
                    routingKey: _options.RoutingKey,
                    basicProperties: properties,
                    body: body);

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }
    }
}
