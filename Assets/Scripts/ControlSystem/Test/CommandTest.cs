public class CommandTest
{
    int[] ints = new int[4];
    int[] ints_2;
    int[] ints_3;

    public CommandTest()
    {
        ints_2 = new int[4];
    }

    public static CommandTest Create()
    {
        return new CommandTest()
        {
            ints_3 = new int[4]
        };
    }
}