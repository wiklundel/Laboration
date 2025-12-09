using System.Data;
using Microsoft.Data.SqlClient;
using Laboration.Models;
using Laboration.ViewModels;

namespace Laboration.DAL
{
    public class SqlPersonDal
    {
        private readonly string _connectionString;

        public SqlPersonDal(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));

        }

        // Hämta alla personer (inkl. deras rätter)
        public List<Person> GetPersons()
        {
            var persons = new Dictionary<int, Person>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT p.PersonID, p.FirstName, p.LastName, p.Age, p.Gender,
                       p.ImagePath, d.DishID, d.DishName
                FROM Persons p
                LEFT JOIN PersonDishes pd ON p.PersonID = pd.PersonID
                LEFT JOIN Dishes d        ON pd.DishID   = d.DishID
                ORDER BY p.PersonID;", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var id = reader.GetInt32(0);

                if (!persons.TryGetValue(id, out var person))
                {
                    person = new Person
                    {
                        PersonID = id,
                        FirstName = reader.GetString(1),
                        LastName  = reader.GetString(2),
                        Age       = reader.GetInt32(3),
                        Gender    = Enum.Parse<Gender>(reader.GetString(4)),
                        ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                        PersonDishes = new List<PersonDish>()
                    };
                    persons[id] = person;
                }

                if (!reader.IsDBNull(6))
                {
                    var dish = new Dish
                    {
                        DishID = reader.GetInt32(6),
                        DishName = reader.GetString(7)
                    };

                    person.PersonDishes!.Add(new PersonDish
                    {
                        PersonID = id,
                        DishID   = dish.DishID,
                        Dish     = dish
                    });
                }
            }

            return persons.Values.ToList();
        }

        public Person? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT p.PersonID, p.FirstName, p.LastName, p.Age, p.Gender,
                       p.ImagePath, d.DishID, d.DishName
                FROM Persons p
                LEFT JOIN PersonDishes pd ON p.PersonID = pd.PersonID
                LEFT JOIN Dishes d        ON pd.DishID   = d.DishID
                WHERE p.PersonID = @id;", conn);

            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            Person? person = null;

            while (reader.Read())
            {
                if (person == null)
                {
                    person = new Person
                    {
                        PersonID  = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName  = reader.GetString(2),
                        Age       = reader.GetInt32(3),
                        Gender    = Enum.Parse<Gender>(reader.GetString(4)),
                        ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                        PersonDishes = new List<PersonDish>()
                    };
                }

                if (!reader.IsDBNull(6))
                {
                    var dish = new Dish
                    {
                        DishID = reader.GetInt32(6),
                        DishName = reader.GetString(7)
                    };

                    person.PersonDishes!.Add(new PersonDish
                    {
                        PersonID = person.PersonID,
                        DishID   = dish.DishID,
                        Dish     = dish
                    });
                }
            }

            return person;
        }

        public List<Dish> GetAllDishes()
        {
            var list = new List<Dish>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT DishID, DishName FROM Dishes ORDER BY DishID;", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Dish
                {
                    DishID   = reader.GetInt32(0),
                    DishName = reader.GetString(1)
                });
            }

            return list;
        }

        public void AddPerson(Person person, IEnumerable<int> selectedDishIds)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // INSERT person
                using var cmd = new SqlCommand(@"
                    INSERT INTO Persons (FirstName, LastName, Age, Gender, ImagePath)
                    OUTPUT INSERTED.PersonID
                    VALUES (@fn, @ln, @age, @gender, @imagePath);", conn, tx);

                cmd.Parameters.AddWithValue("@fn", person.FirstName);
                cmd.Parameters.AddWithValue("@ln", person.LastName);
                cmd.Parameters.AddWithValue("@age", person.Age);
                cmd.Parameters.AddWithValue("@gender", person.Gender.ToString());
                cmd.Parameters.AddWithValue("@imagePath", (object?)person.ImagePath ?? DBNull.Value);

                var newId = (int)cmd.ExecuteScalar();
                person.PersonID = newId;

                // INSERT PersonDishes
                foreach (var dishId in selectedDishIds)
                {
                    using var cmdPd = new SqlCommand(@"
                        INSERT INTO PersonDishes (PersonID, DishID)
                        VALUES (@pid, @did);", conn, tx);

                    cmdPd.Parameters.AddWithValue("@pid", newId);
                    cmdPd.Parameters.AddWithValue("@did", dishId);
                    cmdPd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void UpdatePerson(int id, PersonCreateViewModel vm)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // UPDATE person
                using var cmd = new SqlCommand(@"
                    UPDATE Persons
                    SET FirstName = @fn,
                        LastName  = @ln,
                        Age       = @age,
                        Gender    = @gender,
                        ImagePath = @imagePath
                    WHERE PersonID = @id;", conn, tx);

                cmd.Parameters.AddWithValue("@fn", vm.Person.FirstName);
                cmd.Parameters.AddWithValue("@ln", vm.Person.LastName);
                cmd.Parameters.AddWithValue("@age", vm.Person.Age);
                cmd.Parameters.AddWithValue("@gender", vm.Person.Gender.ToString());
                cmd.Parameters.AddWithValue("@imagePath", (object?)vm.Person.ImagePath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();

                // Ta bort gamla kopplingar
                using var cmdDel = new SqlCommand(
                    "DELETE FROM PersonDishes WHERE PersonID = @id;", conn, tx);
                cmdDel.Parameters.AddWithValue("@id", id);
                cmdDel.ExecuteNonQuery();

                // Lägg in nya
                var selectedIds = vm.SelectedDishIds ?? new List<int>();
                foreach (var dishId in selectedIds)
                {
                    using var cmdPd = new SqlCommand(@"
                        INSERT INTO PersonDishes (PersonID, DishID)
                        VALUES (@pid, @did);", conn, tx);

                    cmdPd.Parameters.AddWithValue("@pid", id);
                    cmdPd.Parameters.AddWithValue("@did", dishId);
                    cmdPd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void DeletePerson(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // Ta bort kopplingar först
                using var cmdPd = new SqlCommand(
                    "DELETE FROM PersonDishes WHERE PersonID = @id;", conn, tx);
                cmdPd.Parameters.AddWithValue("@id", id);
                cmdPd.ExecuteNonQuery();

                // Ta bort personen
                using var cmd = new SqlCommand(
                    "DELETE FROM Persons WHERE PersonID = @id;", conn, tx);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
