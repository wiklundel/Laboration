using Microsoft.AspNetCore.Mvc;
using Laboration.Models;
using Laboration.Helpers;
using Laboration.ViewModels;
using Laboration.DAL;

namespace Laboration.Controllers;

public class PersonController : Controller
{   
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
        var persons = PersonDal.GetPersons(HttpContext);

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
            ModelState.AddModelError(nameof(vm.SelectedDishIds), "Du måste välja minst en signaturrätt.");
        }

        if (!ModelState.IsValid)
        {
            vm.AvailableDishes = AvailableDishes;

            return View(vm);
        }

        vm.Person.PersonDishes = vm.SelectedDishIds.Select(id =>
        {
            var dish = AvailableDishes.FirstOrDefault(d => d.DishID == id);
            if (dish == null) return null;

            return new PersonDish
            {
                DishID = dish.DishID,
                Dish = dish
            };

        })
        .Where(pd => pd != null)
        .ToList()!;

        PersonDal.AddPerson(HttpContext,  vm.Person);
        return RedirectToAction("Persons");
    }

    public IActionResult Details(int id)
    {
        var person = PersonDal.GetById(HttpContext, id);
        if (person == null)
        {
            return NotFound();
        }

        return View(person);
    }

    public IActionResult Delete(int id)
    {
        var person = PersonDal.GetById(HttpContext, id);
        if (person == null)
            return NotFound();

        return View(person);
    }

    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
        PersonDal.Delete(HttpContext, id);
        return RedirectToAction("Persons");
    }

    // GET: Edit
    public IActionResult Edit(int id)
    {
        var person = PersonDal.GetById(HttpContext, id);
        if (person == null)
        {
            return NotFound();
        }

        var vm = new PersonCreateViewModel
        {
            Person = person,
            AvailableDishes = AvailableDishes,
            SelectedDishIds = person.PersonDishes?.Select(pd => pd.DishID).ToList() ?? new List<int>()
        };

        return View(vm);
    }

    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, PersonCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableDishes = AvailableDishes;
            return View(vm);
        }
        PersonDal.UpdatePerson(HttpContext, id, vm);
        return RedirectToAction("Persons");
    }
}
