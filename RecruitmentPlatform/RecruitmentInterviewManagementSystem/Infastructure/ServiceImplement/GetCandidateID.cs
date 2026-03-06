using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Models;
using System.Runtime.InteropServices;

public class GetCandidateID : IGetCandidateID
{
    private readonly FakeTopcvContext _db;

    public GetCandidateID(FakeTopcvContext fakeTopcvContext)
    {
        _db = fakeTopcvContext;
    }

    public async Task<RequestGetCandidateID> GetCandidateId(Guid idUser)
    {

        var candidate = await _db.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == idUser);
        if (candidate == null)
        {
            return new RequestGetCandidateID
            {
                IsSuccess = false,
                CandidateId = Guid.Empty,
            };
        }
        else
        {

            return new RequestGetCandidateID
            {
                IsSuccess = true,
                CandidateId = candidate.Id,
            };
        }
    }
}