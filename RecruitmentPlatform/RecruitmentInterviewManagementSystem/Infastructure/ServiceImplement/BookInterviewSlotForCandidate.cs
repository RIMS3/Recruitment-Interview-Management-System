using CommunityToolkit.HighPerformance.Helpers;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.DistributeLock;
using RecruitmentInterviewManagementSystem.Applications.Features.Schedule;
using RecruitmentInterviewManagementSystem.Applications.Notifications.Producers;
using RecruitmentInterviewManagementSystem.Infastructure.Models;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class BookInterviewSlotForCandidate : IBookInterviewSlotForCandidate
    {
        private readonly FakeTopcvContext _db;
        private readonly IRedisLock _redisLock;
        private readonly IInformScheduleInterivewProducer _informBooking;

        public BookInterviewSlotForCandidate(FakeTopcvContext db, IRedisLock redisLock, IInformScheduleInterivewProducer informScheduleInterivewProducer)
        {
            _db = db;
            _redisLock = redisLock;
            _informBooking = informScheduleInterivewProducer;
        }

        public async Task<(bool IsSuccess, string Message)> ExecuteAsync(string token, Guid slotId)
        {
            // 1. Kiểm tra Token (Hợp lệ, chưa dùng, chưa hết hạn)
            var interviewToken = await _db.InterviewBookingTokens
                .Include(t => t.Application)
                    .ThenInclude(a => a.Job)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (interviewToken == null || interviewToken.IsUsed || interviewToken.ExpiredAt < DateTime.UtcNow)
            {
                return (false, "Link đặt lịch không hợp lệ, đã được sử dụng hoặc đã hết hạn.");
            }

            // 2. Thiết lập thông số cho Redis Lock
            // Khóa theo ID của Slot để đảm bảo 2 ứng viên cùng đặt 1 slot sẽ bị block nhau
            string lockKey = $"lock:interview_slot:{slotId}";
            string lockValue = Guid.NewGuid().ToString(); // Tạo giá trị unique cho phiên lock này
            TimeSpan lockExpiry = TimeSpan.FromSeconds(30); // Thời gian lock tối đa (đề phòng sập server không nhả được lock)

            // 3. Cố gắng lấy Lock
            bool isLocked = await _redisLock.AcquireAsync(lockKey, lockValue, lockExpiry);

            if (!isLocked)
            {
                // Nếu không lấy được lock, nghĩa là có người khác đang xử lý booking slot này
                return (false, "Slot phỏng vấn này đang được ứng viên khác thao tác. Vui lòng thử lại sau giây lát hoặc chọn slot khác.");
            }

            try
            {
                // 4. BẮT ĐẦU XỬ LÝ KHI ĐÃ CẦM LOCK (Double-check)
                // Lấy slot từ DB ra để kiểm tra lại trạng thái hiện tại (bắt buộc)
                var slot = await _db.InterviewsSlots
                                    .FirstOrDefaultAsync(s => s.IdInterviewSlot == slotId);

                if (slot == null)
                    return (false, "Không tìm thấy slot phỏng vấn hệ thống.");

                // Kiểm tra xem slot có thuộc về công ty của Job ứng viên apply không
                if (slot.IdCompany != interviewToken.Application.Job.CompanyId)
                    return (false, "Slot phỏng vấn không hợp lệ với công ty bạn ứng tuyển.");

                if (slot.IsBooked)
                    return (false, "Rất tiếc, slot này vừa mới được một ứng viên khác đặt thành công.");

                if (slot.StartTime < DateTime.UtcNow)
                    return (false, "Slot phỏng vấn này đã qua thời gian bắt đầu.");

                // 5. Cập nhật dữ liệu
                // Tạo record Interview mới
                var newInterview = new Interviews
                {
                    IdInterview = Guid.NewGuid(),
                    IdInterviewSlot = slotId,
                    IdApplication = interviewToken.ApplicationId,
                    Status = "Scheduled", // Trạng thái mặc định ban đầu
                };

                // Đánh dấu Slot đã bị đặt
                slot.IsBooked = true;

                // Đánh dấu Token đã được sử dụng
                interviewToken.IsUsed = true;

                // Thêm vào DbContext
                await _db.Interview.AddAsync(newInterview);
                _db.InterviewsSlots.Update(slot);
                _db.InterviewBookingTokens.Update(interviewToken);

                // Commit xuống database
                await _db.SaveChangesAsync();

                var ApplicationCandidate = _db.Applications.Where(s => s.Id == interviewToken.ApplicationId).FirstOrDefault();

                if (ApplicationCandidate != null)
                {

                    var user = await _db.CandidateProfiles
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.Id == ApplicationCandidate.CandidateId);

                    if (user != null)
                    {

                        string jobTitle = interviewToken.Application.Job.Title;
                        string interviewTime = slot.StartTime.ToString("dd/MM/yyyy HH:mm");
                        string interviewDate = slot.StartTime.ToString("dd/MM/yyyy");
                        string startTime = slot.StartTime.ToString("HH:mm");

                        string emailMessage = $@"
            <h3>Xác nhận lịch phỏng vấn thành công</h3>
            <p>Xin chào <b>{user.User.FullName ?? "Ứng viên tiềm năng"}</b>,</p>
            <p>Chúc mừng bạn đã đặt lịch phỏng vấn thành công cho vị trí: <b>{jobTitle}</b>.</p>
            <hr/>
            <p><b>Thông tin chi tiết:</b></p>
            <ul>
                <li><b>Thời gian:</b> {startTime} ngày {interviewDate}</li>
                <li><b>Trạng thái:</b> Đã xác nhận</li>
            </ul>
            <p>Vui lòng chuẩn bị kỹ lưỡng và có mặt đúng giờ. Nếu có bất kỳ thay đổi nào, bạn vui lòng liên hệ với bộ phận nhân sự của công ty.</p>
            <p>Note,<br/>Nếu đây là nhầm lẫn thì hãy liện hệ cho bộ phận chăm sóc khác hàng của ITLOCAK Corporation</p>"";
            <p>Trân trọng,<br/>Đội ngũ Tuyển dụng</p>";

                        await _informBooking.Execute(new Applications.Notifications.DTO.NotificationDTOS
                        {
                            Email = user.User.Email,
                            Message = emailMessage,
                            TypeService = "Email",
                            Name = user.User.FullName ?? "Ứng Viên Tiềm Năng"
                        });
                    }
                }

                return (true, "Đặt lịch phỏng vấn thành công!");
            }
            catch (Exception ex)
            {
                // TODO: Nên ghi log lỗi ra file/console tại đây
                return (false, "Đã xảy ra lỗi trong quá trình đặt lịch. Vui lòng thử lại.");
            }
            finally
            {
                // 6. GIẢI PHÓNG LOCK (Bắt buộc phải đặt trong finally)
                await _redisLock.ReleaseAsync(lockKey, lockValue);
            }
        }
    }
}
