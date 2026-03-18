using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using RecruitmentInterviewManagementSystem.Applications.Notifications.DTO;
using RecruitmentInterviewManagementSystem.Applications.Notifications.Interfaces;
using System.Text;
using System.Text.Json;

namespace RecruitmentInterviewManagementSystem.Infastructure.Workers
{
    public class EmailWorker : BackgroundService
    {
        private readonly IConfiguration _iConfig;
        private readonly IEnumerable<INotification> _notifications;
        private readonly ILogger<EmailWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private IChannel _channel;
        private IConnection _connection;

        public EmailWorker(
            IConfiguration configuration,
            IEnumerable<INotification> notifications,
            ILogger<EmailWorker> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _iConfig = configuration;
            _notifications = notifications;
            _logger = logger;
            _scopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _iConfig["RabbitMQ:Host"]!,
                    UserName = _iConfig["RabbitMQ:UserName"]!,
                    Password = _iConfig["RabbitMQ:Password"]!
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                if (_connection.IsOpen && _channel.IsOpen)
                {
                    _logger.LogInformation(" Connected to RabbitMQ successfully.");
                }
                else
                {
                    _logger.LogWarning(" RabbitMQ connection not open.");
                    return; 
                }

                var queueName = _iConfig["RabbitMQ:Queuen:Email"]!;

                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                await _channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        var content = JsonSerializer.Deserialize<NotificationDTOS>(message);

                        if (content == null)
                        {
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            return;
                        }

                        var request = new RequestSendMessage(
                            to: content.Email,
                            subject: "Welcome to IT Locak",
                            body: $"Chào mừng {content.Name} đã gia nhập cộng đồng của chúng mình!\n\n" +
                                  $"Tài khoản của bạn đã được thiết lập thành công.\n\n" +
                                  $"Trân trọng,\nĐội ngũ vận hành",
                            messageType: content.TypeService
                        );

                        var service = _notifications
                            .FirstOrDefault(x => x.TypeService == content.TypeService);

                        if (service == null)
                        {
                            _logger.LogWarning("Notification service not found: {type}", content.TypeService);
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            return;
                        }

                        var result = await service.SendRegisterAccount(request);

                        if (result)
                        {
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        }
                        else
                        {
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Email consumer error");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumerTag: "Notification Interview",
                    consumer: consumer);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Emai interview Worker crash");
            }
        }
    }
}