using EntityFrameworkSQLserver.Data;
using EntityFrameworkSQLserver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityFrameworkSQLserver.Pages
{
    public class CreateModel : PageModel
    {
        private readonly EntityFrameworkSQLserver.Data.TareaDbContext _context;

        public CreateModel(EntityFrameworkSQLserver.Data.TareaDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Tarea Tarea { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            if (!ValidarTarea()) return Page();

            try
            {
                _context.Tareas.Add(Tarea);
                await _context.SaveChangesAsync();
                TempData["exito_creacion"] = true;
                return RedirectToPage("./Index"); // return Page();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar la tarea en la base de datos. Intenta nuevamente.");
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado. Intenta nuevamente.");
                return Page();
            }
        }

        private bool ValidarTarea()
        {
            var ok = true;
            if (!ValidarNombre()) ok = false;
            if (!ValidarFecha()) ok = false;
            if (!ValidarEstado()) ok = false;
            if (!ValidarIdUsuario()) ok = false;

            if (ok)
            {
                // OJO: como ValidarTarea es sync, hacemos un GetAwaiter().
                var duplicada = ExisteTareaDuplicadaAsync(Tarea.nombreTarea, Tarea.fechaVencimientoTarea, Tarea.idUsuario, Tarea.estadoTarea)
                    .GetAwaiter().GetResult();

                if (duplicada)
                {
                    ModelState.AddModelError(string.Empty, "Ya existe una tarea exactamente igual con el mismo nombre, fecha, estado y usuario.");
                    ok = false;
                }
            }
            return ok;
        }

        private bool ValidarNombre()
        {
            var nombre = Tarea.nombreTarea?.Trim() ?? "";
            if (string.IsNullOrEmpty(nombre))
            {
                ModelState.AddModelError("Tarea.nombreTarea", "El nombre es obligatorio.");
                return false;
            }
            if (nombre.Length < 9) // Ir a casa = 9 caracteres, no hay frase logica menor a esa cantidad
            {
                ModelState.AddModelError("Tarea.nombreTarea", "Debe tener al menos 9 caracteres.");
                return false;
            }
            if (nombre.Length > 255) // En tema de Nombrar una Tarea no Deberia ser mas de eso, 
            {
                ModelState.AddModelError("Tarea.nombreTarea", "Máximo 255 caracteres.");
                return false;
            }
            Tarea.nombreTarea = nombre;
            return true;
        }

        private bool ValidarFecha()
        {
            var fecha = Tarea.fechaVencimientoTarea.Date;
            var maxFecha = new DateTime(2100, 12, 31);
            if (fecha < DateTime.Today)
            {
                ModelState.AddModelError("Tarea.fechaVencimientoTarea", "La fecha debe ser hoy o futura.");
                return false;
            }
            if (fecha > maxFecha)
            {
                ModelState.AddModelError("Tarea.fechaVencimientoTarea", "La fecha no puede ser posterior al 31/12/2100.");
                return false;
            }
            Tarea.fechaVencimientoTarea = fecha;
            return true;
        }

        private bool ValidarEstado()
        {
            Tarea.estadoTarea = "Pendiente";
            return true;
        }

        private bool ValidarIdUsuario()
        {
            if (Tarea.idUsuario <= 0)
            {
                ModelState.AddModelError("Tarea.idUsuario", "Debe ser un número positivo distinto a 0.");
                return false;
            }
            if (Tarea.idUsuario.ToString().Length > 9)
            {
                ModelState.AddModelError("Tarea.idUsuario", "No debe exceder 10 dígitos.");
                return false;
            }
            return true;
        }

        // EXTRA POR SI EXISTIERA TAREA DUPLICADA
        private async Task<bool> ExisteTareaDuplicadaAsync(string nombre, DateTime fecha, int idUsuario, string estadoTarea)
        {
            var n = nombre.ToLowerInvariant();
            return await _context.Tareas.AsNoTracking()
                .AnyAsync(t => t.idUsuario == idUsuario
                               && t.fechaVencimientoTarea.Date == fecha.Date
                               && t.estadoTarea == estadoTarea
                               && (t.nombreTarea ?? "").ToLower() == n);
        }
    }
}
