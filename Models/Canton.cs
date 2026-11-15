using System.ComponentModel.DataAnnotations;

namespace CrudClientesCSharp.Models;

public class Canton
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nombre { get; set; } = "";

    public int ProvinciaId { get; set; }
    public Provincia? Provincia { get; set; }
}
