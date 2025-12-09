using Microsoft.AspNetCore.Mvc;
using Laboration.Models;
using Laboration.ViewModels;
using Laboration.DAL;

namespace Laboration.Controllers;

public class PersonController : Controller
{   
    private readonly SqlPersonDal _personDal;
    public PersonController(IConfiguration config)
    {
        var connStr = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            throw new Exception("ConnectionString 'DefaultConnection' saknas eller är tom.");
        }
        _personDal = new SqlPersonDal(connStr);
    }

    // GET: Person
    public IActionResult Persons(string? searchName, Gender? genderFilter, string? sortOrder)
    {
        var persons = _personDal.GetPersons();

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
            AvailableDishes = _personDal.GetAllDishes(),
            SelectedDishIds = new List<int>()
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
            vm.AvailableDishes = _personDal.GetAllDishes();
            return View(vm);
        }

        vm.Person.ImagePath = SaveImage(vm.ImageFile);

        _personDal.AddPerson(vm.Person, vm.SelectedDishIds);
        return RedirectToAction("Persons");
    }

    public IActionResult Details(int id)
    {
        var person = _personDal.GetById(id);
        if (person == null)
        {
            return NotFound();
        }

        return View(person);
    }

    public IActionResult Delete(int id)
    {
        var person = _personDal.GetById(id);
        if (person == null)
            return NotFound();

        return View(person);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _personDal.DeletePerson(id);
        return RedirectToAction("Persons");
    }

    // GET: Edit
    public IActionResult Edit(int id)
    {
        var person = _personDal.GetById(id);
        if (person == null)
        {
            return NotFound();
        }

        var vm = new PersonCreateViewModel
        {
            Person = person,
            AvailableDishes = _personDal.GetAllDishes(),
            SelectedDishIds = person.PersonDishes?.Select(pd => pd.DishID).ToList() ?? new List<int>()
        };

        return View(vm);
    }

    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, PersonCreateViewModel vm)
    {
        if (vm.SelectedDishIds == null || !vm.SelectedDishIds.Any())
        {
            ModelState.AddModelError(nameof(vm.SelectedDishIds), "Du måste välja minst en signaturrätt.");
        }

        if (!ModelState.IsValid)
        {
            vm.AvailableDishes = _personDal.GetAllDishes();
            return View(vm);
        }

        if (vm.ImageFile != null)
        {
            vm.Person.ImagePath = SaveImage(vm.ImageFile);
        }

        _personDal.UpdatePerson(id, vm);
        return RedirectToAction("Persons");
    }

    private string? SaveImage(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return null;

        var uploadsFolder = Path.Combine("wwwroot", "images");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            imageFile.CopyTo(stream);
        }

        return $"/images/{fileName}";
    }
    public IActionResult Stats()
    {
        var persons = _personDal.GetPersons();

        var dishGroups = persons
            .SelectMany(p => p.PersonDishes)
            .GroupBy(pd => pd.Dish.DishName)
            .Select(g => new
            {
                Dish = g.Key,
                Count = g.Count()
            })
            .ToList();

        ViewBag.Labels = dishGroups.Select(d => d.Dish).ToList();
        ViewBag.Values = dishGroups.Select(d => d.Count).ToList();

        return View();
    }


}
