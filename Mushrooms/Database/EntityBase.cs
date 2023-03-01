namespace Mushrooms.Database {
    public class EntityBase {
        public  string  Id { get; }

        protected EntityBase() :
            this( NCuid.Cuid.Generate()) {
        }

        protected EntityBase( string id ) {
            Id = id;
        }
    }

}
