using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentInterviewManagementSystem.Infastructure.Models
{
    [Table("Banners")]
    public class Banner
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageName { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;


    }
}
