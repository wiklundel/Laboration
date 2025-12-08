using Laboration.Models;
using Laboration.Helpers;
using Microsoft.AspNetCore.Http;
using Laboration.ViewModels;

namespace Laboration.DAL
{
    public static class PersonDal
    {
        private const string SessionKeyPersons = "Persons";
        private static int nextPersonID = 1;

        private static List<Person> LoadPersons(HttpContext httpContext)
        {
            var persons = httpContext.Session.GetObjectFromJson<List<Person>>(SessionKeyPersons);
            return persons ?? new List<Person>();
        }

        private static void SavePersons(HttpContext httpContext, List<Person> persons)
        {
            httpContext.Session.SetObjectAsJson(SessionKeyPersons, persons);
        }

        public static List<Person> GetPersons(HttpContext httpContext)
        {
            return LoadPersons(httpContext);
        }

        public static Person? GetById(HttpContext httpContext, int personId)
        {
            var persons = LoadPersons(httpContext);
            return persons.FirstOrDefault(p => p.PersonID == personId);
        }

        public static void AddPerson(HttpContext httpContext, Person person)
        {
            var persons = LoadPersons(httpContext);
            if (!persons.Any())
            {
                nextPersonID = 1;
            }
            else
            {
                nextPersonID = Math.Max(nextPersonID, persons.Max(p => p.PersonID) + 1);
            }
            person.PersonID = nextPersonID++;

            persons.Add(person);
            SavePersons(httpContext, persons);
        }

        public static void Delete(HttpContext httpContext, int id)
        {
            var persons = GetPersons(httpContext);
            var person = persons.FirstOrDefault(p => p.PersonID == id);

            if (person != null)
            {
                persons.Remove(person);
                SavePersons(httpContext, persons);

                ReindexPersons(httpContext);
            }
        }

        // Indexerar om i samband med delete
        private static void ReindexPersons(HttpContext httpContext)
        {
            var persons = GetPersons(httpContext);
            
            for (int i = 0; i < persons.Count; i++)
            {
                var newId = i + 1;
                var p = persons[i];

                p.PersonID = newId;

                if (p.PersonDishes != null)
                {
                    foreach (var pd in p.PersonDishes)
                    {
                        pd.PersonID = newId;
                    }
                }
            }
            nextPersonID = persons.Count + 1;
            SavePersons(httpContext, persons);
        }

        public static void UpdatePerson(HttpContext httpContext, int id, PersonCreateViewModel vm)
        {
            var persons = GetPersons(httpContext);
            var personToEdit = persons.FirstOrDefault(p => p.PersonID == id);

            if (personToEdit == null) return;
            
            personToEdit.FirstName    = vm.Person.FirstName;
            personToEdit.LastName     = vm.Person.LastName;
            personToEdit.Age          = vm.Person.Age;
            personToEdit.Gender       = vm.Person.Gender;

            var selectedIds = vm.SelectedDishIds ?? new List<int>();

            personToEdit.PersonDishes = selectedIds
                .Select(dishId =>
                {
                    var dish = vm.AvailableDishes.FirstOrDefault(d => d.DishID == dishId);
                    if (dish == null) return null;

                    return new PersonDish
                    {
                        PersonID = personToEdit.PersonID,
                        DishID   = dish.DishID,
                        Dish     = dish
                    };
                })
                .Where(pd => pd != null)
                .ToList()!;

            SavePersons(httpContext, persons);
        }
    }
}