
namespace UserManagementAPI.Models
{
    public class UserEntry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public int Age { get; set; }
    };

    public static class StaffDatabase
    {
        public static List<UserEntry> StaffEntries = new List<UserEntry>
        {
            new UserEntry { Id = 1, Name = "Alice", Role = "Manager", Department = "HR", Age = 35 },
            new UserEntry { Id = 2, Name = "Bob", Role = "Developer", Department = "IT", Age = 28 },
            new UserEntry { Id = 3, Name = "Charlie", Role = "Analyst", Department = "Finance", Age = 30 }
        };

        public static int GetNextId()
        {
            if (StaffEntries.Count == 0)
                return 1;
            else
                return StaffEntries.Max(e => e.Id) + 1;
        }
    }

    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public static class Credentials
    {
        public static string SecretKey { get; } = "MySuperSecretKeyThatIsAtLeast32Chars";
        public static string Issuer { get; } = "UserManagementAPI";
        public static string Audience { get; } = "UserManagementAPIClients";
        public static int ValidityHours { get; } = 24;
        public static string AdminPassword { get; } = "password123";
        public static string AdminUserName { get; } = "admin";
    }
}