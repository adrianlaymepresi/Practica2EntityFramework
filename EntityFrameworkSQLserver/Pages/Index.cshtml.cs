using EntityFrameworkSQLserver.Data;
using EntityFrameworkSQLserver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkSQLserver.Pages
{
    public class IndexModel : PageModel
    {
        private readonly EntityFrameworkSQLserver.Data.TareaDbContext _context;

        public IndexModel(EntityFrameworkSQLserver.Data.TareaDbContext context)
        {
            _context = context;
        }
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
                AjustarParametrosDePaginacion();

                var termino = (TextoBusqueda ?? "").Trim();
                var terminoNorm = NormalizarTexto(termino);

                var estados = new[] { "pendiente", "en curso" };

                var todas = await _context.Tareas
                    .AsNoTracking()
                    .Where(t => estados.Contains(t.estadoTarea.ToLower()))
                    .ToListAsync();

                IEnumerable<Tarea> fuente;
                if (terminoNorm.Length == 0)
                {
                    fuente = todas
                        .OrderBy(t => t.fechaVencimientoTarea)
                        .ThenBy(t => t.Id);
                }
                else
                {
                    fuente = todas
                        .Select(t => new
                        {
                            T = t,
                            NombreNorm = NormalizarTexto(t.nombreTarea ?? "")
                        })
                        .Where(x => x.NombreNorm.Contains(terminoNorm))
                        .Select(x => new
                        {
                            x.T,
                            Relev = CalcularRelevancia(x.NombreNorm, terminoNorm)
                        })
                        .OrderBy(x => x.Relev.empieza)
                        .ThenBy(x => x.Relev.indice)
                        .ThenBy(x => x.Relev.diferenciaLongitud)
                        .ThenBy(x => x.T.Id)
                        .Select(x => x.T);
                }

                var totalRegistros = fuente.Count();
                CantidadTotalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)CantidadRegistrosPorPagina));
                if (PaginaActual > CantidadTotalPaginas) PaginaActual = CantidadTotalPaginas;

                CalcularVentanaDePaginas();

                var omitir = (PaginaActual - 1) * CantidadRegistrosPorPagina;

                Tarea = fuente
                    .Skip(omitir)
                    .Take(CantidadRegistrosPorPagina)
                    .ToList();
            }
            catch
            {
                Tarea = new List<Tarea>();
                CantidadTotalPaginas = 1;
                CalcularVentanaDePaginas();
            }
        }

        private void AjustarParametrosDePaginacion()
        {
            if (CantidadRegistrosPorPagina < 1) CantidadRegistrosPorPagina = 5;
            if (CantidadRegistrosPorPagina > 99) CantidadRegistrosPorPagina = 99;
            if (PaginaActual < 1) PaginaActual = 1;
        }

        private void CalcularVentanaDePaginas()
        {
            if (CantidadTotalPaginas < 1) { PageWindowStart = 1; PageWindowEnd = 1; return; }

            PageWindowStart = ((PaginaActual - 1) / WindowSize) * WindowSize + 1;
            if (PageWindowStart < 1) PageWindowStart = 1;

            PageWindowEnd = Math.Min(PageWindowStart + WindowSize - 1, CantidadTotalPaginas);
        }

        private static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            var descompuesto = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompuesto.Length);
            foreach (var c in descompuesto)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        private static (int empieza, int indice, int diferenciaLongitud) CalcularRelevancia(string nombreNormalizado, string terminoNormalizado)
        {
            var empieza = nombreNormalizado.StartsWith(terminoNormalizado, StringComparison.Ordinal) ? 0 : 1;
            var indice = nombreNormalizado.IndexOf(terminoNormalizado, StringComparison.Ordinal);
            if (indice < 0) indice = int.MaxValue;
            var diferenciaLongitud = Math.Abs(nombreNormalizado.Length - terminoNormalizado.Length);
            return (empieza, indice, diferenciaLongitud);
        }
    }
}

/*
    public class IndexModel : PageModel
    {
        private readonly EntityFrameworkSQLserver.Data.TareaDbContext _context;

        public IndexModel(EntityFrameworkSQLserver.Data.TareaDbContext context)
        {
            _context = context;
        }

        public IList<Tarea> Tarea { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Tarea = await _context.Tareas.ToListAsync();
        }
    } 
*/
