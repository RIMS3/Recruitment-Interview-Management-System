
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using RecruitmentInterviewManagementSystem.Applications.Notifications.Interfaces;
using System.Text;
using System.Text.Json;
using RecruitmentInterviewManagementSystem.Applications.Notifications.DTO;

namespace RecruitmentInterviewManagementSystem.Infastructure.Workers
{
    public class NotificationInterviewWorker : BackgroundService
    {
        private readonly IConfiguration _iConfig;
        private readonly IEnumerable<INotification> _notifications;
        private readonly ILogger<NotificationInterviewWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private IChannel _channel;
        private IConnection _connection;

        public NotificationInterviewWorker(IConfiguration iConfig, IEnumerable<INotification> notifications, ILogger<NotificationInterviewWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _iConfig = iConfig;
            _notifications = notifications;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
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

                var queueName = _iConfig["RabbitMQQUEUE"]!;

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
                          body: $"Chào {content.Name},\n\n" +
      $"Chúng tôi rất ấn tượng với hồ sơ của bạn và mong muốn mời bạn tham gia buổi phỏng vấn.\n\n" +
      $"Vui lòng chọn thời gian phù hợp với bạn thông qua đường link bên dưới:\n" +
      $"{content.Link}\n\n" +
      $"Sau khi bạn đặt lịch thành công, chúng tôi sẽ xác nhận lại thông tin chi tiết về buổi phỏng vấn.\n\n" +
      $"Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi.\n\n" +
      $"Trân trọng,\n" ,
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
                        _logger.LogError(ex, "Notification Interview consumer error");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumerTag: "Notification Interview Consumer ITLocak 999 ",
                    consumer: consumer);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotificationInterview Worker crash");
            }
        }
    }
}
