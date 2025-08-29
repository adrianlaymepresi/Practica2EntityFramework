namespace EntityFrameworkSQLserver.Models
{
    public class Tarea
    {
        public int Id { get; set; }
        public string nombreTarea { get; set; }
        public DateTime fechaVencimientoTarea { get; set; }
        public string estadoTarea { get; set; } 
        public int idUsuario { get; set; }

    }
}
