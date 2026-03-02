
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.Workers
{
    public class AutoUnPost : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AutoUnPost> _logger;

        public AutoUnPost(IServiceScopeFactory serviceScopeFactory, ILogger<AutoUnPost> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var listJobPostActive = scope.ServiceProvider.GetRequiredService<IJobPostRepository>();

                    var listTrue = await listJobPostActive.GetAllAsync();

                    var listChange = scope.ServiceProvider.GetRequiredService<IChangeStatusJobPost>();

                    if (listTrue != null)
                    {
                        var listId = listTrue.Where(j => j.ExpireAt < DateTime.UtcNow && j.IsActive == true).Select(j => j.Id).ToList();
                        if (listId.Any())
                        {
                            await listChange.ChangeStatusAsync(listId);
                        }
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Bug in backgroundService OutBoxMessage Type :{ex.Message}");
                }
                _logger.LogInformation("JobPostbackgroundservice is running");
                // thư viQuartz.NET
            }
        }
    }
}

