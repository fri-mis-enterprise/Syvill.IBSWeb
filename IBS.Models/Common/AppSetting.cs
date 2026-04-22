using System.ComponentModel.DataAnnotations;

namespace IBS.Models.Common
{
    public class AppSetting
    {
        [Key] public string SettingKey { get; set; } = null!;

        public string Value { get; set; } = null!;
    }
}
