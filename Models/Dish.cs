using System.ComponentModel.DataAnnotations;

public class Dish
{
    [Key]
    public int DishID {get; set;}

    [Required, StringLength(80)]
    public string DishName {get; set;}

    public List<PersonDish>? PersonDishes {get; set;}
}
