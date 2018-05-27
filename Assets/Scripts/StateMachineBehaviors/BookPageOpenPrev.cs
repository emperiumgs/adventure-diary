public class BookPageOpenPrev : EndAction
{
    public override void Action()
    {
        BookController.instance.ConcludeOpenPrevPage();
    }
}