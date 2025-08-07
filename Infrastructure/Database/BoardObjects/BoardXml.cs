using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Database.BoardObjects;

public class BoardXml
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "xml")]
    public string Body { get; set; } = string.Empty;
}