namespace User.API.ElasticSearch
{
    public class UserSearchResult
    {
        public long Id { get; set; }
        public bool IsEnabled { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarPath { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public long RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
