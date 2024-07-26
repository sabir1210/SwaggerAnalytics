using System.ComponentModel.DataAnnotations;

namespace SwaggerAnalytics
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
}