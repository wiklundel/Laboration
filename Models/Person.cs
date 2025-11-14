using System.ComponentModel.DataAnnotations;
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

    public Person (int personID, string firstName, string lastName, int age)
    {
        this.personID = personID;
        this.firstName = firstName;
        this.lastName = lastName;
        this.age = age;
        personDishes = new List<PersonDish>();
    }
}