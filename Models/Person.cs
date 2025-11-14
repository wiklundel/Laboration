using System.ComponentModel.DataAnnotations;
using System.Text;
public class Person {
    public int personID { get; set; }

    [Required, MaxLength(50)]
    public string firstName { get; set; }

    [Required, MaxLength(50)]
    public string lastName { get; set; }

    [Range(0, 120)]
    public int age { get; set; }

    public List<PersonDish>? personDishes { get; set; }

    public Person ()
    {
        firstName = "Unknown";
        lastName = "Unknown";
        age = 0;
        personDishes = new List<PersonDish>();
    }

    public Person (string firstName, string lastName, int age)
    {
        this.firstName = FormatName(firstName);
        this.lastName = FormatName(lastName);
        this.age = age;
        personDishes = new List<PersonDish>();
    }

    private string FormatName(string name)
    {
        if(string.IsNullOrWhiteSpace(name)) return "Unknown";
        return char.ToUpper(name[0]) + name.Substring(1).ToLower();
    }
}