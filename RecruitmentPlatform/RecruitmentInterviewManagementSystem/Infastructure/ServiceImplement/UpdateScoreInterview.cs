using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Schedule;
using RecruitmentInterviewManagementSystem.Applications.Notifications.Producers;
using RecruitmentInterviewManagementSystem.Models;
using System;
using System.Threading.Tasks;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class UpdateScoreInterview : IUpdateScoreInterview
    {
        private readonly FakeTopcvContext _db;
        private readonly IInformScheduleInterivewProducer _notificationService;

        public UpdateScoreInterview(FakeTopcvContext fakeTopcvContext, IInformScheduleInterivewProducer notificationInterviewProducer)
        {
            _db = fakeTopcvContext;
            _notificationService = notificationInterviewProducer;
        }

        public async Task<bool> Execute(UpdateScoreInterivewDTo request)
        {
            try
            {
              
                var interview = await _db.Interview
                    .Include(i => i.application)
                        .ThenInclude(a => a.Candidate)
                            .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(x => x.IdInterview == request.IdInterview);

               
                if (interview == null || interview.application?.Candidate?.User == null)
                {
                    return false;
                }

                var user = interview.application.Candidate.User;

                interview.TechnicalScore = request.Technical;
                interview.SoftSkillScore = request.SoftSkill;
                interview.Status = "Scored";

              
                _db.Interview.Update(interview);
                await _db.SaveChangesAsync();

                var notification = new Applications.Notifications.DTO.NotificationDTOS
                {
                    Name = user.FullName ?? "Ứng viên",
                    Email = user.Email,
                    Titel = "Kết quả phỏng vấn - Hệ thống tuyển dụng ITLOCAK",
                    Message = $@"Chào {user.FullName}, 
                                Kết quả phỏng vấn của bạn đã được cập nhật:
                                - Điểm kỹ thuật (Technical): {request.Technical}
                                - Điểm kỹ năng mềm (Soft Skill): {request.SoftSkill}
                                Chúng tôi sẽ liên hệ sớm nhất về quyết đinh tuyển dụng.",
                    TypeService = "Email"
                };

                
                await _notificationService.Execute(notification);

                return true;
            }
            catch (Exception ex)
            {
             
                return false;
            }
        }
    }
}