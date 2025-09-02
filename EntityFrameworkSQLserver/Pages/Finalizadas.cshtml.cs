using EntityFrameworkSQLserver.Data;
using EntityFrameworkSQLserver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;


namespace EntityFrameworkSQLserver.Pages
{
    public class FinalizadasModel : PageModel
    {
        private readonly TareaDbContext _context;
        public FinalizadasModel(TareaDbContext context) => _context = context;

        [BindProperty(SupportsGet = true, Name = "pagina")]
        public int PaginaActual { get; set; } = 1;

        [BindProperty(SupportsGet = true, Name = "cantidadRegistrosPorPagina")]
        public int CantidadRegistrosPorPagina { get; set; } = 5;

        [BindProperty(SupportsGet = true, Name = "q")]
        public string TextoBusqueda { get; set; } = "";

        public int CantidadTotalPaginas { get; set; }
        public IList<Tarea> Tarea { get; set; } = new List<Tarea>();

        public int PageWindowStart { get; private set; }
        public int PageWindowEnd { get; private set; }
        public bool HasPrevPage => PaginaActual > 1;
        public bool HasNextPage => PaginaActual < CantidadTotalPaginas;

        private const int WindowSize = 10;

        public async Task OnGetAsync()
        {
            try
            {
                AjustarParametros();

                var termino = (TextoBusqueda ?? "").Trim();
                if (termino.Length > 100) termino = termino.Substring(0, 100);
                var terminoNorm = Normalizar(termino);

                var todas = await _context.Tareas
                    .AsNoTracking()
                    .Where(t => t.estadoTarea.ToLower() == "finalizado")
                    .ToListAsync();

                IEnumerable<Tarea> fuente;
                if (string.IsNullOrEmpty(terminoNorm))
                {
                    fuente = todas.OrderBy(t => t.fechaVencimientoTarea)
                                  .ThenBy(t => t.Id);
                }
                else
                {
                    fuente = todas
                        .Select(t => new { T = t, NombreNorm = Normalizar(t.nombreTarea ?? "") })
                        .Where(x => x.NombreNorm.Contains(terminoNorm))
                        .Select(x => new { x.T, Relev = Relevancia(x.NombreNorm, terminoNorm) })
                        .OrderBy(x => x.Relev.empieza)
                        .ThenBy(x => x.Relev.indice)
                        .ThenBy(x => x.Relev.diferenciaLongitud)
                        .ThenBy(x => x.T.Id)
                        .Select(x => x.T);
                }

                var total = fuente.Count();
                CantidadTotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)CantidadRegistrosPorPagina));
                if (PaginaActual > CantidadTotalPaginas) PaginaActual = CantidadTotalPaginas;

                CalcularVentana();

                var skip = (PaginaActual - 1) * CantidadRegistrosPorPagina;
                Tarea = fuente.Skip(skip).Take(CantidadRegistrosPorPagina).ToList();
            }
            catch
            {
                Tarea = new List<Tarea>();
                CantidadTotalPaginas = 1;
                CalcularVentana();
            }
        }

        private void AjustarParametros()
        {
            if (CantidadRegistrosPorPagina < 1) CantidadRegistrosPorPagina = 5;
            if (CantidadRegistrosPorPagina > 99) CantidadRegistrosPorPagina = 99;
            if (PaginaActual < 1) PaginaActual = 1;
        }

        private void CalcularVentana()
        {
            if (CantidadTotalPaginas < 1) { PageWindowStart = 1; PageWindowEnd = 1; return; }
            PageWindowStart = ((PaginaActual - 1) / WindowSize) * WindowSize + 1;
            PageWindowEnd = Math.Min(PageWindowStart + WindowSize - 1, CantidadTotalPaginas);
        }

        private static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            var descompuesto = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompuesto.Length);
            foreach (var c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        private static (int empieza, int indice, int diferenciaLongitud) Relevancia(string nombreNorm, string terminoNorm)
        {
            var empieza = nombreNorm.StartsWith(terminoNorm, StringComparison.Ordinal) ? 0 : 1;
            var indice = nombreNorm.IndexOf(terminoNorm, StringComparison.Ordinal);
            if (indice < 0) indice = int.MaxValue;
            var diferencia = Math.Abs(nombreNorm.Length - terminoNorm.Length);
            return (empieza, indice, diferencia);
        }
    }
}
