namespace Blog.Interfaces
{
    public interface IDashboardRepository
    {
        public IDictionary<string, object> GetDashboardData();
    }
}
