using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlExpertSimpleAPI.Models;

namespace ControlExpertSimpleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/Todo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodo()
        {
          if (_context.Todo == null)
          {
              return NotFound();
          }
            return await _context.Todo.ToListAsync();
        }

        // POST: api/Todo
        [HttpPost]
        public async Task<ActionResult<Todo>> PostTodoItem(Todo todoItem)
        {
            _context.Todo.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodo), new { id = todoItem.Id }, todoItem);
        }
    }
}
