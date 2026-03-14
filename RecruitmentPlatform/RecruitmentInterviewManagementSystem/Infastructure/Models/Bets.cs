using RecruitmentInterviewManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentInterviewManagementSystem.Infastructure.Models
{
    [Table("Bets")]
    public class Bets
    {
        [Key]
        public Guid Id { get; set; }

        public Guid IdUser { get; set; }

        public Guid IdGame { get; set; }

        public string BetType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal WinAmount { get; set; } = 0;

        public DateTime CreatedAt { get; set; }

        [ForeignKey("IdGame")]
        public GameRounds GameRound { get; set; }


        [ForeignKey("IdUser")]
        public User User { get; set; }
    }
}
