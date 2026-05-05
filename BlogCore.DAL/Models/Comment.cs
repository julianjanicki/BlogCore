namespace BlogCore.DAL.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [ForeignKey(nameof(Post))]
    public int PostId { get; set; }

    public Post? Post { get; set; }
}