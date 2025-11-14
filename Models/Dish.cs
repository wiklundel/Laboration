using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

public class Dish
{
    [Key]
    public int dishID {get; set;}

    [Required, StringLength(80)]
    public string dishName {get; set;}

    public List<PersonDish> personDishes {get; set;}

    // Konstruktorn
    public Dish()
    {
        dishName = "Unnamed Dish";
        personDishes = new List<PersonDish>();
    }

    public Dish(string dishName)
    {
        this.dishName = dishName;
        personDishes = new List<PersonDish>();
    }

    public Dish (Dish cpy)
    {
        dishID = cpy.dishID;
        dishName = cpy.dishName;
        personDishes = cpy.personDishes;
    }

}
