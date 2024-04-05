using Nest;

namespace ElasticSearch.API.Models
{
    public class UserSearchResult
    {
        public long Id { get; set; }
        public bool IsEnabled { get; set; }
        [PropertyName("fullName")]
        public string? FullName { get; set; }
        [PropertyName("email")]
        public string? Email { get; set; }
        [PropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }
        public string? AvatarPath { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public long RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
