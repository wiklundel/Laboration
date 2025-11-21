using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Collections.Generic;

namespace Laboration.Models;

public enum Gender
{
    Female,
    Male,
    Other
}
public class Person {
    public int PersonID { get; set; }

    [Required, MaxLength(50)]
    public string? FirstName { get; set; }

    [Required, MaxLength(50)]
    public string? LastName { get; set; }

    [Range(0, 120)]
    public int? Age { get; set; }

    [Required]
    public Gender Gender { get; set; }

    public List<PersonDish>? PersonDishes { get; set; } = new();

    // Konstruktorn
    public Person () {}

    // Konstruktorn 2
    public Person (string firstName, string lastName, int age)
    {
        FirstName = FormatName(firstName);
        LastName = FormatName(lastName);
        Age = age;
        PersonDishes = new List<PersonDish>();
    }

    // Kopieringskonstruktorn
    public Person (Person cpy)
    {
        PersonID = cpy.PersonID;
        FirstName = cpy.FirstName;
        LastName = cpy.LastName;
        Age = cpy.Age;
        PersonDishes = new List<PersonDish>();
    }

    // Metod för att formatera namn
    private string FormatName(string name)
    {
        if(string.IsNullOrWhiteSpace(name)) return "Unknown";
        return char.ToUpper(name[0]) + name.Substring(1).ToLower();
    }
}