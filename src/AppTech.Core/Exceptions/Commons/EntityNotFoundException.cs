namespace AppTech.Core.Exceptions.Commons
{
    public class EntityNotFoundException : Exception
    {
        public string EntityName { get; set; }
        public EntityNotFoundException(string entityName)
        {
            EntityName = entityName;
        }
    }
}
