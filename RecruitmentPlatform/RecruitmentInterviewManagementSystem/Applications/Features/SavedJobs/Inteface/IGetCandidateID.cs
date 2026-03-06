using System.Runtime.InteropServices;
 
public interface IGetCandidateID
{
  Task<RequestGetCandidateID>  GetCandidateId(Guid idUser);

}