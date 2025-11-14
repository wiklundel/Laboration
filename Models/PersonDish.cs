public class PersonDish
{
    public int PersonDishID { get; set; }

    public int DishID { get; set; }
    public Dish? Dish { get; set; }

    public int PersonID { get; set; }
    public Person? Person { get; set; }

    public PersonDish() {}

    public PersonDish(int personID, int dishID)
    {
        this.PersonID = personID;
        this.DishID = dishID;
    }

    public PersonDish(PersonDish cpy)
    {
        PersonDishID = cpy.PersonDishID;
        PersonID = cpy.PersonID;
        DishID = cpy.DishID;
        Person = cpy.Person;
        Dish = cpy.Dish;
    }
}