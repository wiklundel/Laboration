using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

public class Dish
{
    [Key]
    public int DishID {get; set;}

    [Required, StringLength(80)]
    public string DishName {get; set;}

    public List<PersonDish> PersonDishes {get; set;}

    // Konstruktorn
    public Dish()
    {
        DishName = "Unnamed Dish";
        PersonDishes = new List<PersonDish>();
    }

    // Överlagrad konstruktorn
    public Dish(string dishName)
    {
        this.DishName = dishName;
        PersonDishes = new List<PersonDish>();
    }

    // Kopieringskonstruktorn
    public Dish (Dish cpy)
    {
        DishID = cpy.DishID;
        DishName = cpy.DishName;
        PersonDishes = new List<PersonDish>();
    }

}
