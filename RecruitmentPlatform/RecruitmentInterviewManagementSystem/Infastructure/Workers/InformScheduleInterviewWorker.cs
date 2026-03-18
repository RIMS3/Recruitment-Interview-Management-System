
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using RecruitmentInterviewManagementSystem.Applications.Notifications.Interfaces;
using System.Text;
using System.Text.Json;
using RecruitmentInterviewManagementSystem.Applications.Notifications.DTO;

namespace RecruitmentInterviewManagementSystem.Infastructure.Workers
{
    public class InformScheduleInterviewWorker : BackgroundService
    {
        private readonly IConfiguration _iConfig;
        private readonly IEnumerable<INotification> _notifications;
        private readonly ILogger<InformScheduleInterviewWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private IChannel _channel;
        private IConnection _connection;

        public InformScheduleInterviewWorker(
            IConfiguration configuration,
            IEnumerable<INotification> notifications,
            ILogger<InformScheduleInterviewWorker> logger,
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

                var queueName = _iConfig["RabbitMQQUEUEInformInterview"]!;

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
                            subject: content.Titel ?? "Thông báo lịch phỏng vấn - IT LOCAK Corporation",
                            body: content.Message ?? "Hihihi có vẻ như feature này đang bị lỗi",
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
                    consumerTag: "Inform Interview",
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
