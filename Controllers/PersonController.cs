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

    // List of dishes
    private static readonly List<Dish> AvailableDishes = new ()
    {
        new Dish() { DishID = 1, DishName = "Spaghetti Bolognese" },
        new Dish() { DishID = 2, DishName = "Chicken Curry" },
        new Dish() { DishID = 3, DishName = "Vegetable Stir Fry" },
        new Dish() { DishID = 4, DishName = "Beef Stroganoff" },
        new Dish() { DishID = 5, DishName = "Fish and Chips" }
    };

    // GET: Person
    public IActionResult Persons(string? searchName, Gender? genderFilter, string? sortOrder)
    {
        var persons = LoadPersonsFromSession();

        if(!string.IsNullOrWhiteSpace(searchName))
        {
            var lower = searchName.ToLower();
            persons = persons.
                        Where(p => (!string.IsNullOrEmpty(p.FirstName) && p.FirstName.ToLower().Contains(lower)) ||
                                   (!string.IsNullOrEmpty(p.LastName) && p.LastName.ToLower().Contains(lower)))
                             .ToList();
        }

        // Filtrering på kön
        if (genderFilter.HasValue)
        {
            persons = persons
                        .Where(p => p.Gender == genderFilter.Value)
                        .ToList();
        }

        // Soretring
        persons = sortOrder switch
        {
            "name_desc" => persons.OrderByDescending(p => p.LastName).ThenByDescending(p => p.FirstName).ToList(),
            "name_asc"  => persons.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList(),
            "age_asc"   => persons.OrderBy(p => p.Age).ToList(),
            "age_desc"  => persons.OrderByDescending(p => p.Age).ToList(),
            _           => persons.OrderBy(p => p.PersonID).ToList(),
        };



        // Provide the person count through both ViewData and ViewBag so views can use either.
        ViewData["CurrentSearch"] = searchName;
        ViewData["CurrentGender"] = genderFilter;
        ViewData["CurrentSort"] = sortOrder;
        ViewBag.PersonCount = persons.Count;
        return View(persons);
    }

    // GET: Create
    public IActionResult Create()
    {
        var vm = new PersonCreateViewModel
        {
            Person = new Person(),
            AvailableDishes = AvailableDishes
        };
        return View(vm);
    }

    // POST: Create
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
            .Select(id => 
            {
            var dish = AvailableDishes.FirstOrDefault(d => d.DishID == id);
            if (dish == null) return null;

            return new PersonDish
            {
                PersonID = vm.Person.PersonID,
                DishID = dish.DishID,
                Dish = dish
            };
        })
        .Where(pd => pd != null)
        .ToList()!;

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
            SavePersonsToSession(persons);
        }

        return RedirectToAction("Persons");
    }
}
