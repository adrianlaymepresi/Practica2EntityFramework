using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkSQLserver.Data;
using EntityFrameworkSQLserver.Models;

namespace EntityFrameworkSQLserver.Pages
{
    public class EditModel : PageModel
    {
        private readonly EntityFrameworkSQLserver.Data.TareaDbContext _context;

        public EditModel(EntityFrameworkSQLserver.Data.TareaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Tarea Tarea { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();
            var tarea = await _context.Tareas.FirstOrDefaultAsync(m => m.Id == id);
            if (tarea == null) return NotFound();
            Tarea = tarea;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var original = await _context.Tareas.AsNoTracking().FirstOrDefaultAsync(t => t.Id == Tarea.Id);
            if (original == null) return NotFound();

            if (!ValidarEdicion(original)) return Page();

            try
            {
                original.nombreTarea = Tarea.nombreTarea;
                original.fechaVencimientoTarea = Tarea.fechaVencimientoTarea.Date;
                original.idUsuario = Tarea.idUsuario;

                _context.Update(original);
                await _context.SaveChangesAsync();

                TempData["exito_edicion"] = true;
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "Otro usuario modificó este registro. Recarga la página.");
                return Page();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar los cambios. Intenta nuevamente.");
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado. Intenta nuevamente.");
                return Page();
            }
        }

        private bool ValidarEdicion(Tarea original)
        {
            var ok = true;
            if (!ValidarNombre()) ok = false;
            if (!ValidarFecha()) ok = false;
            if (!ValidarIdUsuario()) ok = false;
            Tarea.estadoTarea = original.estadoTarea;
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
            if (nombre.Length < 9)
            {
                ModelState.AddModelError("Tarea.nombreTarea", "Debe tener al menos 9 caracteres.");
                return false;
            }
            if (nombre.Length > 255)
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
            if (fecha < DateTime.Today)
            {
                ModelState.AddModelError("Tarea.fechaVencimientoTarea", "La fecha debe ser hoy o futura.");
                return false;
            }
            Tarea.fechaVencimientoTarea = fecha;
            return true;
        }

        private bool ValidarIdUsuario()
        {
            if (Tarea.idUsuario <= 0)
            {
                ModelState.AddModelError("Tarea.idUsuario", "Debe ser un número positivo distinto a 0.");
                return false;
            }
            if (Tarea.idUsuario.ToString().Length > 10)
            {
                ModelState.AddModelError("Tarea.idUsuario", "No debe exceder 10 dígitos.");
                return false;
            }
            return true;
        }
    }
}
