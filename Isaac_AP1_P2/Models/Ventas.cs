using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Isaac_AP1_P2.Models;

public class Ventas
{
    [Key]
    public int VentaId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La observación es obligatoria")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
    public string Observacion { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [ForeignKey("VentaId")]
    public List<VentasDetalle> VentasDetalle { get; set; } = new();
}