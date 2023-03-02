namespace Mushrooms.Database {
    public class EntityBase {
        public  string  Id { get; protected set; }
            
        protected EntityBase( string id ) {
            Id = id;
        }

        protected EntityBase() {
            Id = NCuid.Cuid.Generate();
        }
    }

}
