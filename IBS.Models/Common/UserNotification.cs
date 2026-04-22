using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.MasterFile;

namespace IBS.Models.Common
{
    public class UserNotification
    {
        [Key] public Guid UserNotificationId { get; set; }

        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))] public ApplicationUser User { get; set; } = null!;

        public Guid NotificationId { get; set; }

        [ForeignKey(nameof(NotificationId))] public Notification Notification { get; set; } = null!;

        public bool IsRead { get; set; }

        public bool IsArchived { get; set; }

        public bool RequiresResponse { get; set; }
    }
}
