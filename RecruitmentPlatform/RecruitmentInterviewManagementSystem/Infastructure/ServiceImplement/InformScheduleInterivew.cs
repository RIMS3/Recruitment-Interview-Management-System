using RabbitMQ.Client;
using RecruitmentInterviewManagementSystem.Applications.Notifications.DTO;
using RecruitmentInterviewManagementSystem.Applications.Notifications.Producers;
using System.Text;
using System.Text.Json;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class InformScheduleInterivew : IInformScheduleInterivewProducer
    {

        private readonly IConfiguration _iConfig;
        private readonly ILogger<EmailProducer> _logger;

        public InformScheduleInterivew(IConfiguration iConfig, ILogger<EmailProducer> logger)
        {
            _iConfig = iConfig;
            _logger = logger;
        }

        public async Task Execute(NotificationDTOS request)
        {
            var factory = new ConnectionFactory
            {
                HostName = _iConfig["RabbitMQ:Host"]!,
                UserName = _iConfig["RabbitMQ:UserName"]!,
                Password = _iConfig["RabbitMQ:Password"]!
            };
            using var _connection = await factory.CreateConnectionAsync();
            using var _channel = await _connection.CreateChannelAsync();
            _logger.LogInformation($"Producer Product is Running");
            await _channel.ExchangeDeclareAsync(_iConfig["RabbitMQExchangeInformInterview"]!, type: ExchangeType.Direct, durable: true);


            await _channel.QueueDeclareAsync(
                _iConfig["RabbitMQQUEUEInformInterview"]!,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await _channel.QueueBindAsync(
                queue: _iConfig["RabbitMQQUEUEInformInterview"]!,
                exchange: _iConfig["RabbitMQExchangeInformInterview"]!,
                routingKey: _iConfig["RabbitMQRoutingKeyInformInterview"]!
            );

            var json = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(json);
            var properties = new BasicProperties
            {
                Persistent = true
            };
            await _channel.BasicPublishAsync(
                    exchange: _iConfig["RabbitMQExchangeInformInterview"]!,
                    routingKey: _iConfig["RabbitMQRoutingKeyInformInterview"]!,
                    mandatory: false,
                    basicProperties: properties,
                    body: body
            );
        }
    }
}
