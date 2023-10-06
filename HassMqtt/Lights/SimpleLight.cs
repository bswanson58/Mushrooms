namespace HassMqtt.Lights {
    public class SimpleLight : LightBase {
        private const string   cDeviceName = "simple light";

        public SimpleLight( string name = cDeviceName ) : 
            base( String.IsNullOrWhiteSpace( name ) ? cDeviceName : name ) {
        }
    }
}
