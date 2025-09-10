// This enumeration represents local flex controllers in the Source Engine.
public enum LocalFlexController_t
{
    DUMMY_FLEX_CONTROLLER = 0x7fffffff
}

// Define extension methods to mimic the increment and decrement behavior
public static class LocalFlexControllerExtensions
{
    public static LocalFlexController_t Increment(this LocalFlexController_t a)
    {
        return (LocalFlexController_t)((int)a + 1);
    }

    public static LocalFlexController_t Decrement(this LocalFlexController_t a)
    {
        return (LocalFlexController_t)((int)a - 1);
    }

    public static LocalFlexController_t PostIncrement(this LocalFlexController_t a)
    {
        LocalFlexController_t t = a;
        a = (LocalFlexController_t)((int)a + 1);
        return t;
    }

    public static LocalFlexController_t PostDecrement(this LocalFlexController_t a)
    {
        LocalFlexController_t t = a;
        a = (LocalFlexController_t)((int)a - 1);
        return t;
    }
}
