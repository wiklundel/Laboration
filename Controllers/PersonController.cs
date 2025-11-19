using Microsoft.AspNetCore.Mvc;
using Laboration.Models;

namespace Laboration.Controllers
{
    public class PersonController : Controller
    {
        static IList<Person> personList = new List<Person>
        {
            new Person() { FirstName = "Alice", LastName = "Andersson", Age = 30 },
            new Person() { FirstName = "Bob", LastName = "Bengtsson", Age = 25 },
            new Person() { FirstName = "Charlie", LastName = "Carlsson", Age = 35 }
        };

        // GET: Person
        public IActionResult Persons()
        {
            return View(personList);    
        }
    }
}