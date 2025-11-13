using System.ComponentModel.DataAnnotations;
public class Person {
    public int PersonID { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; }

    [Range(0, 120)]
    public int Age { get; set; }


}