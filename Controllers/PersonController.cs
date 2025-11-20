using Microsoft.AspNetCore.Mvc;
using Laboration.Models;

namespace Laboration.Controllers;

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

    public IActionResult Create()
    {
        ViewBag.Dishes = new List<Dish>
        {
            new Dish() { DishID = 1, DishName = "Spaghetti Bolognese" },
            new Dish() { DishID = 2, DishName = "Chicken Curry" },
            new Dish() { DishID = 3, DishName = "Vegetable Stir Fry" }
        };
        return View(new Person());
    }

    [HttpPost]
    public IActionResult Create(Person person, List<int>? SelectedDishes)
    {   
        // 1. Kräv minst en dish
        if (SelectedDishes == null || !SelectedDishes.Any())
        {
            ModelState.AddModelError("SelectedDishes", "Du måste välja minst en signaturrätt.");
        }

        if (!ModelState.IsValid)
        {
            return Create();
        }
        
        foreach (var dishID in SelectedDishes)
        {
            person.PersonDishes.Add(new PersonDish 
            {
                PersonID = person.PersonID,
                DishID = dishID
            });
        }
        personList.Add(person);
        return RedirectToAction("Persons");
    }

    public IActionResult Details()
    {
        return View();
    }
}
