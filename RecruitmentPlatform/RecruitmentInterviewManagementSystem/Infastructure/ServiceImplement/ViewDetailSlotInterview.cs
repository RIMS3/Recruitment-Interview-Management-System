using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.API.DTOs;
using RecruitmentInterviewManagementSystem.Applications.Features.Schedule;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class ViewDetailSlotInterview : IViewDetailSlotInterview
    {
        private readonly FakeTopcvContext _context;

        public ViewDetailSlotInterview(FakeTopcvContext fakeTopcvContext)
        {
            _context = fakeTopcvContext;
        }

        public async Task<InterviewSlotDetailDto?> GetSlotDetailAsync(Guid slotId)
        {
            var slotDetail = await _context.InterviewsSlots
                .Where(s => s.IdInterviewSlot == slotId)
                .Select(s => new InterviewSlotDetailDto
                {
                    SlotId = s.IdInterviewSlot,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsBooked = s.IsBooked,

               
                    InterviewId = s.Interview != null ? s.Interview.IdInterview : null,
                    Status = s.Interview != null ? s.Interview.Status : null,
                    TechnicalScore = s.Interview != null ? s.Interview.TechnicalScore : null,
                    SoftSkillScore = s.Interview != null ? s.Interview.SoftSkillScore : null,
                    Decision = s.Interview != null ? s.Interview.Decision : null,

                    
                    CandidateId = (s.Interview != null && s.Interview.application != null)
                                    ? s.Interview.application.CandidateId : null,

                    CandidateAvatarUrl = (s.Interview != null && s.Interview.application != null && s.Interview.application.Candidate != null)
                                    ? s.Interview.application.Candidate.AvatarUrl : null,

                    ExperienceYears = (s.Interview != null && s.Interview.application != null && s.Interview.application.Candidate != null)
                                    ? s.Interview.application.Candidate.ExperienceYears : null,

                  
                    CandidateName = (s.Interview != null && s.Interview.application != null && s.Interview.application.Candidate != null && s.Interview.application.Candidate.User != null)
                                    ? s.Interview.application.Candidate.User.FullName : null,

                    CandidateEmail = (s.Interview != null && s.Interview.application != null && s.Interview.application.Candidate != null && s.Interview.application.Candidate.User != null)
                                    ? s.Interview.application.Candidate.User.Email : null,

                    CandidatePhone = (s.Interview != null && s.Interview.application != null && s.Interview.application.Candidate != null && s.Interview.application.Candidate.User != null)
                                    ? s.Interview.application.Candidate.User.PhoneNumber : null,
 
                    JobId = (s.Interview != null && s.Interview.application != null)
                                    ? s.Interview.application.JobId : null,
                    JobTitle = (s.Interview != null && s.Interview.application != null && s.Interview.application.Job != null)
                                    ? s.Interview.application.Job.Title : null,
                    JobLocation = (s.Interview != null && s.Interview.application != null && s.Interview.application.Job != null)
                                    ? s.Interview.application.Job.Location : null
                })
                .FirstOrDefaultAsync();

            return slotDetail;
        }
    }
}
