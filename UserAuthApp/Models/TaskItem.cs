using System.ComponentModel.DataAnnotations.Schema;

namespace UserAuthApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public bool IsCompleted { get; set; }
        public required string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        [NotMapped]
        public bool IsDueSoon =>
    DueDate.HasValue &&
    DueDate.Value.Date <= DateTime.Today.AddDays(1) &&
    !IsCompleted;


    }
}
