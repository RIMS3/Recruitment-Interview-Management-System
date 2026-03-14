using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using RecruitmentInterviewManagementSystem.Applications.Payments.DTO;
using RecruitmentInterviewManagementSystem.Applications.Payments.Interface;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class Refill : IRefill
    {
        private readonly FakeTopcvContext _db;
        private readonly PayOSClient _payos;
        private readonly ILogger<Refill> _logger;

        public Refill(FakeTopcvContext fakeTopcvContext, PayOSClient payOSClient, ILogger<Refill> logger)
        {
            _db = fakeTopcvContext;
            _payos = payOSClient;
            _logger = logger;
        }

        public async Task<ResponseRefillDTO> Execute(RefillDTO request)
        {
            var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (request.Amount <= 0)
                {
                    return new ResponseRefillDTO
                    {
                        IsSuccess = false,
                        Message = "Amount must be greater than 0"
                    };
                }

                var orderCode = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = request.IdUser,
                    CreatedAt = DateTime.Now,
                    Coin = 0,
                    OrderCode = orderCode.ToString(),
                    PaidAt = null,
                    Status = (int)PaymentStatus.Pending,
                    TotalAmount = request.Amount,
                };

                _db.Orders.Add(order);

                await _db.SaveChangesAsync();
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = (long)order.TotalAmount,
                    Description = "NapTienITLOCAK",
                    ReturnUrl = "https://your-url.com",
                    CancelUrl = "https://your-url.com"
                };

                var paymentLink = await _payos.PaymentRequests.CreateAsync(paymentRequest);

                if (paymentLink.QrCode == null)
                {
                    await transaction.RollbackAsync();

                    return new ResponseRefillDTO
                    {
                        IsSuccess = false,
                        Message = "Failed to create payment link"
                    };

                }
                else
                {
                    await transaction.CommitAsync();
                    return new ResponseRefillDTO
                    {
                        IsSuccess = true,
                        Message = "Payment link created successfully",
                        QRCode = paymentLink.QrCode,
                    };
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating payment link for refill");
                await transaction.RollbackAsync();

                return new ResponseRefillDTO
                {
                    IsSuccess = false,
                    Message = "Failed to create payment link"
                };
            }

        }

        public async Task<decimal> Execute(Guid idUser)
        {
            return await _db.Users.Where(u => u.Id == idUser).Select(u => u.Coin).FirstOrDefaultAsync();
        }
    }
}
