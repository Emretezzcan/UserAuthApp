using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using UserAuthApp.Data;
using UserAuthApp.Models;

public class TaskController : Controller
{
    private readonly ApplicationDbContext _context;

    public TaskController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Listeleme + filtreleme + arama
    public IActionResult Index(string filter = "all", string search = "")
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Auth");

        var query = _context.TaskItems.Where(t => t.UserEmail == email);

        if (filter == "completed")
            query = query.Where(t => t.IsCompleted);
        else if (filter == "incomplete")
            query = query.Where(t => !t.IsCompleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Title.Contains(search));

        var result = query
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate)
            .ToList();

        // 📊 İstatistikler
        ViewBag.TotalTasks = _context.TaskItems.Count(t => t.UserEmail == email);
        ViewBag.CompletedTasks = _context.TaskItems.Count(t => t.UserEmail == email && t.IsCompleted);
        ViewBag.OverdueTasks = _context.TaskItems.Count(t => t.UserEmail == email && !t.IsCompleted && t.DueDate < DateTime.Now);

        ViewBag.CurrentFilter = filter;
        ViewBag.CurrentSearch = search;

        return View(result);
    }


    // Görev ekleme
    [HttpPost]
    public IActionResult Add(string title, DateTime? dueDate)
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrWhiteSpace(title))
        {
            var newTask = new TaskItem
            {
                Title = title,
                UserEmail = email,
                CreatedAt = DateTime.Now,
                DueDate = dueDate,
                IsCompleted = false
            };

            _context.TaskItems.Add(newTask);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // Görev durumunu tersine çevirme (tamamlandı/devam)
    public IActionResult Toggle(int id)
    {
        var task = _context.TaskItems.Find(id);
        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // Görev silme
    public IActionResult Delete(int id)
    {
        var task = _context.TaskItems.Find(id);
        if (task != null)
        {
            _context.TaskItems.Remove(task);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // GET: Görev düzenleme formu
    public IActionResult Edit(int id)
    {
        var task = _context.TaskItems.Find(id);
        if (task == null) return NotFound();

        var email = HttpContext.Session.GetString("UserEmail");
        if (task.UserEmail != email) return Unauthorized();

        return View(task);
    }

    // POST: Düzenlenen görevi kaydetme
    [HttpPost]
    public IActionResult Edit(TaskItem updatedTask)
    {
        var existingTask = _context.TaskItems.Find(updatedTask.Id);
        if (existingTask == null) return NotFound();

        var email = HttpContext.Session.GetString("UserEmail");
        if (existingTask.UserEmail != email) return Unauthorized();

        existingTask.Title = updatedTask.Title;
        existingTask.DueDate = updatedTask.DueDate;

        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}

