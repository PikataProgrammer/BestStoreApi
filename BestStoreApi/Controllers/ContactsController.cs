using BestStoreApi.Dtos;
using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ContactsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContactsController(ApplicationDbContext context)
    {
        this._context = context;
    }
    [HttpGet("subjects")]
    public IActionResult GetContactSubjects()
    {
        var listSubjects = _context.Subjects.ToList();
        return Ok(listSubjects);
    }

    [HttpGet]
    public IActionResult GetContacts(int? page)
    {
        if (page == null || page < 1)
        {
            page = 1;
        }
        
        int pageSize = 5;
        int totalPages = 0;
        
        decimal count = _context.Contacts.Count();
        totalPages = (int)Math.Ceiling(count / pageSize);
        
        var contacts = _context.Contacts.Include(s => s.Subject)
            .OrderByDescending(p => p.Id)
            .Skip((int)(page! - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = new
        {
            Contacts = contacts,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page,
        };
        
        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetContactById(int id)
    {
        var contact = _context.Contacts.Include(c => c.Subject).FirstOrDefault(c => c.Id == id);
        if (contact == null)
        {
            return NotFound();
        }

        return Ok(contact);
    }
    [HttpPost]
    public IActionResult CreateContact(ContactDto contact)
    {
        var subject = _context.Subjects.Find(contact.SubjectId);
        if (subject == null)
        {
            ModelState.AddModelError("Subject", "Subject does not exist");
            return BadRequest(ModelState);
        }
        var newContact = new Contact()
        {
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone =  contact.Phone ?? string.Empty,
            Subject = subject,
            Message = contact.Message,
            CreatedAt = DateTime.Now
        };
        _context.Contacts.Add(newContact);
        _context.SaveChanges();
        
        return Ok(newContact);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateContact(int id, ContactDto contact)
    {
        var subject = _context.Subjects.Find(contact.SubjectId);
        if (subject == null)
        {
            ModelState.AddModelError("Subject", "Subject does not exist");
            return BadRequest(ModelState);
        }
        var existingContact = _context.Contacts.Find(id);

        if (existingContact == null)
        {
            return NotFound();
        }

        existingContact.FirstName = contact.FirstName;
        existingContact.LastName = contact.LastName;
        existingContact.Email = contact.Email;
        existingContact.Phone = contact.Phone ?? string.Empty;
        existingContact.Subject = subject;
        existingContact.Message = contact.Message;
        
        _context.SaveChanges();
        
        return Ok(existingContact);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteContact(int id)
    {
        //Method 1
        // var contact = _context.Contacts.Find(id);
        //
        // if (contact == null)
        // {
        //     return NotFound();
        // }
        //
        // _context.Contacts.Remove(contact);
        // _context.SaveChanges();
        //
        // return NoContent();
        
        //Method2
        try
        {
            var contact = new Contact() {Id = id, Subject = new Subject()};
            _context.Contacts.Remove(contact);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            return NotFound();
        }
        return Ok(); 
    }
}