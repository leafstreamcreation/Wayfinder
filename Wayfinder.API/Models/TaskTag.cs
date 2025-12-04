namespace Wayfinder.API.Models
{
    /// <summary>
    /// Join entity for many-to-many relationship between Task and Tag
    /// </summary>
    public class TaskTag
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public int TagId { get; set; }
    }
}
