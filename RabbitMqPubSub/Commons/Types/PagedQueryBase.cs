namespace RabbitMqPubSub.Commons.Types
{
    public abstract class PagedQueryBase : IPagedQuery
    {
        public int Page { get; set; } = 1;
        public int Results { get; set; } = 10;
    }
}