public class BookPageOpenNext : EndAction
{
    public override void Action()
    {
        BookController.instance.ConcludeOpenNextPage();
    }
}