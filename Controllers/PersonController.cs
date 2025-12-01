using Microsoft.AspNetCore.Mvc;
using Laboration.Models;
using Laboration.Helpers;
using Laboration.ViewModels;

namespace Laboration.Controllers;

public class PersonController : Controller
{
    private static int _nextPersonId = 1;
    private const string SessionKeyPersons = "Persons";
    private List<Person> LoadPersonsFromSession()
    {
        var persons = HttpContext.Session.GetObjectFromJson<List<Person>>(SessionKeyPersons);
        return persons ?? new List<Person>();
    }
    
    private void SavePersonsToSession(List<Person> persons)
    {
        HttpContext.Session.SetObjectAsJson(SessionKeyPersons, persons);
    }

    private static readonly List<Dish> AvailableDishes = new ()
    {
        new Dish() { DishID = 1, DishName = "Spaghetti Bolognese" },
        new Dish() { DishID = 2, DishName = "Chicken Curry" },
        new Dish() { DishID = 3, DishName = "Vegetable Stir Fry" },
        new Dish() { DishID = 4, DishName = "Beef Stroganoff" },
        new Dish() { DishID = 5, DishName = "Fish and Chips" }
    };

    // GET: Person
    public IActionResult Persons()
    {
        var persons = LoadPersonsFromSession();
        ViewData["PersonCount"] = persons.Count;
        return View(persons);
    }

    public IActionResult Create()
    {
        var vm = new PersonCreateViewModel
        {
            Person = new Person(),
            AvailableDishes = AvailableDishes
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PersonCreateViewModel vm)
    {   
        // 1. Kräv minst en dish
        if (vm.SelectedDishIds == null || !vm.SelectedDishIds.Any())
        {
            // Attach the error to the actual property name so it shows up next to the checkbox list
            ModelState.AddModelError(nameof(vm.SelectedDishIds), "Du måste välja minst en signaturrätt.");
        }

        if (!ModelState.IsValid)
        {
            vm.AvailableDishes = AvailableDishes;

            return View(vm);
        }
        
        // hämta nuvarande lista från session
        var persons = LoadPersonsFromSession();

        if (!persons.Any())
        {
            _nextPersonId = 1;
        }
        vm.Person.PersonID = _nextPersonId++;
        vm.Person.PersonDishes = vm.SelectedDishIds
            .Select(id => new PersonDish(vm.Person.PersonID, id))
            .ToList();

        persons.Add(vm.Person);
        SavePersonsToSession(persons);

        return RedirectToAction("Persons");
    }

    public IActionResult Details(int id)
    {
        var persons = LoadPersonsFromSession();
        var person = persons.FirstOrDefault(p => p.PersonID == id);

        if (person == null)
        {
            return NotFound();
        }

        return View(person);
    }

    public IActionResult Delete(int id)
    {
        var persons = LoadPersonsFromSession();
        var person = persons.FirstOrDefault(p => p.PersonID == id);

        if (person == null)
            return NotFound();

        return View(person);
    }

    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
        var persons = LoadPersonsFromSession();
        var person = persons.FirstOrDefault(p => p.PersonID == id);

        if (person != null)
        {
            persons.Remove(person);
            SavePersonsToSession(persons);   // viktigt!
        }

        return RedirectToAction("Persons");
    }
}
