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
        public string UserName { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}
