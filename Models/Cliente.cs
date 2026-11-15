using System.ComponentModel.DataAnnotations;

namespace CrudClientesCSharp.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(80)]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80)]
    public string Apellidos { get; set; } = "";

    [Display(Name = "Cédula")]
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Ingrese una cédula de 10 dígitos.")]
    public string Cedula { get; set; } = "";

    [Display(Name = "Correo electrónico")]
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(120)]
    public string Correo { get; set; } = "";

    [Display(Name = "Teléfono")]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[+0-9 ()-]{7,20}$", ErrorMessage = "Ingrese un teléfono de 7 a 20 caracteres válidos.")]
    public string Telefono { get; set; } = "";

    [Display(Name = "Dirección")]
    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200)]
    public string Direccion { get; set; } = "";

    [Display(Name = "Fecha de nacimiento")]
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; }

    [Display(Name = "Ocupación")]
    [Required(ErrorMessage = "La ocupación es obligatoria.")]
    [StringLength(80)]
    public string Ocupacion { get; set; } = "";

    [Display(Name = "Provincia")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una provincia.")]
    public int ProvinciaId { get; set; }

    [Display(Name = "Cantón")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cantón.")]
    public int CantonId { get; set; }

    public Provincia? Provincia { get; set; }
    public Canton? Canton { get; set; }
}
