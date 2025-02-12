using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourNamespace.Models  // 🔹 Match this with ApplicationDbContext.cs
{
    [Table("UserTable")]
    public class UserTable
    {
        //   [Key]
        //   public int Id { get; set; }

        [Key]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
