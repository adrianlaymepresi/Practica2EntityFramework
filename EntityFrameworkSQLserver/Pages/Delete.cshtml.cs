using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkSQLserver.Data;
using EntityFrameworkSQLserver.Models;

namespace EntityFrameworkSQLserver.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly EntityFrameworkSQLserver.Data.TareaDbContext _context;

        public DeleteModel(EntityFrameworkSQLserver.Data.TareaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Tarea Tarea { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var tarea = await _context.Tareas.FirstOrDefaultAsync(m => m.Id == id);

                if (tarea == null)
                {
                    return NotFound();
                }
                else
                {
                    Tarea = tarea;
                }
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al cargar los datos.");
                return Page();
            }            
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var tarea = await _context.Tareas.FindAsync(id);
                if (tarea != null)
                {
                    Tarea = tarea;
                    _context.Tareas.Remove(Tarea);
                    await _context.SaveChangesAsync();

                }

                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "Otro usuario modificó o eliminó esta tarea. Recarga la página.");
                return Page();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo eliminar la tarea. Intenta nuevamente.");
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado. Intenta nuevamente.");
                return Page();
            }
        }
    }
}
