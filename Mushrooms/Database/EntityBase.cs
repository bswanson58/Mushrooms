namespace Mushrooms.Database {
    public record EntityBase {
        public  string  Id { get; init; } = NCuid.Cuid.Generate();
    }

}
