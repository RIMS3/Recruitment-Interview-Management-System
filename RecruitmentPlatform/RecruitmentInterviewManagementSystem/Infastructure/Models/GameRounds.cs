using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentInterviewManagementSystem.Infastructure.Models
{
    [Table("GameRounds")]
    public class GameRounds
    {
        [Key]
        public Guid IdGame { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Result { get; set; } = string.Empty;

        public int Dice1 { get; set; }

        public int Dice2 { get; set; }

        public int Dice3 { get; set; }

        public bool IsClosed { get; set; }

        public ICollection<Bets> Bets { get; set; } = new List<Bets>();
    }
}
