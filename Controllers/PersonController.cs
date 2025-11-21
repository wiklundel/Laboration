using Microsoft.AspNetCore.Mvc;
using Laboration.Models;
using Laboration.Helpers;

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

    // GET: Person
    public IActionResult Persons()
    {
        var persons = LoadPersonsFromSession();
        return View(persons);
    }

    private static readonly List<Dish> AvailableDishes = new ()
    {
        new Dish() { DishID = 1, DishName = "Spaghetti Bolognese" },
        new Dish() { DishID = 2, DishName = "Chicken Curry" },
        new Dish() { DishID = 3, DishName = "Vegetable Stir Fry" },
        new Dish() { DishID = 4, DishName = "Beef Stroganoff" },
        new Dish() { DishID = 5, DishName = "Fish and Chips" }
    };

    public IActionResult Create()
    {
        ViewBag.Dishes = AvailableDishes;
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
            ViewBag.Dishes = AvailableDishes;

            return View(person);
        }
        
        // hämta nuvarande lista från session
        var persons = LoadPersonsFromSession();

        // sätt unikt ID
        if (persons == null || !persons.Any())
        {
            _nextPersonId = 1;
        }
        person.PersonID = _nextPersonId++;

        person.PersonDishes = new List<PersonDish>();
        foreach (var dishId in SelectedDishes)
        {
            var dish = AvailableDishes.FirstOrDefault(d => d.DishID == dishId);
            if (dish != null)
            {
                person.PersonDishes.Add(new PersonDish
                {
                    PersonID = person.PersonID,
                    DishID = dish.DishID,
                    Dish = dish
                });
            }
        }

        // lägg till valda rätter
        persons.Add(person);


        // spara tillbaka till session
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
