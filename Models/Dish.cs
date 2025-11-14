using System.ComponentModel.DataAnnotations;

public class Dish
{
    [Key]
    public int dishID {get; set;}

    [Required, StringLength(80)]
    public string dishName {get; set;}

    public List<PersonDish>? personDishes {get; set;}

    // Konstruktorn
    public Dish()
    {
        dishName = "Unnamed Dish";
        personDishes = new List<PersonDish>();
    }

    public Dish(int dishID, string dishName)
    {
        this.dishID = dishID;
        this.dishName = dishName;
        personDishes = new List<PersonDish>();
    }

}
