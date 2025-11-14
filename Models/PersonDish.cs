public class PersonDish
{
    public int personDishID { get; set; }

    public int dishID { get; set; }
    public Dish? Dish { get; set; }

    public int personID { get; set; }
    public Person? Person { get; set; }

    public PersonDish(int personID, int dishID)
    {
        this.personID = personID;
        this.dishID = dishID;
    }

    public PersonDish(PersonDish cpy)
    {
        personDishID = cpy.personDishID;
        personID = cpy.personID;
        dishID = cpy.dishID;
        Person = cpy.Person;
        Dish = cpy.Dish;
    }
}