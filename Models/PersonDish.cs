public class PersonDish
{
    public int personDishID { get; set; }
    public int dishID { get; set; }
    public Dish? Dish { get; set; }
    public int personID { get; set; }
    public Person? Person { get; set; }

    public PersonDish()
    {
        
    }

    public PersonDish(int PersonDishID, int personID, int dishID)
    {
        this.personDishID = personDishID;
        this.personID = personID;
        this.dishID = dishID;

    }
}