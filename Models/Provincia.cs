using System.ComponentModel.DataAnnotations;

namespace CrudClientesCSharp.Models;

public class Provincia
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nombre { get; set; } = "";

    public ICollection<Canton> Cantones { get; set; } = new List<Canton>();
}
